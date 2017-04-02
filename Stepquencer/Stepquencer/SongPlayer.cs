using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Diagnostics;

#if __ANDROID__
using Android.Media;
#endif

#if __IOS__
using AVFoundation;
using AudioToolbox;
using AudioUnit;
#endif

namespace Stepquencer
{
    class SongPlayer
    {
#if __IOS__
        OutputAudioQueue audioQueue;
        AudioStreamBasicDescription streamDesc;
#endif
#if __ANDROID__
        AudioTrack playingTrack;
#endif
        //Used to synchronize multithreaded accesses
        object trackDisposedOfSyncObject = new object();
        object startStopSyncObject = new object();

        const int PLAYBACK_RATE = 44100;

        public delegate void OnBeatDelegate(int beatNum, bool firstBeat);
        public event OnBeatDelegate BeatStarted;

        Song song;
        int samplesPerBeat;

        //The next beat to generate
        int nextBeat;

        /// <summary>
        /// Whether the song is currently being played.
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                lock (trackDisposedOfSyncObject)
                {
#if __ANDROID__
                    return playingTrack != null;
#endif
#if __IOS__
                    return audioQueue != null;
#endif
                }
            }
        }

        public SongPlayer(Song song)
        {
            this.song = song;
#if __IOS__
            streamDesc = AudioStreamBasicDescription.CreateLinearPCM(PLAYBACK_RATE, 1, 16, false);  // Might need to check if little or big endian
#endif
        }

        /// <summary>
        /// Begins playing at the specified BPM
        /// </summary>
        public void BeginPlaying(int bpm)
        {
            lock (startStopSyncObject) //Use lock so track is not stopped while it is being started
            {
                if (IsPlaying)
                {
                    throw new InvalidOperationException("Audio is already playing.");
                }
                
                samplesPerBeat = ((60 * PLAYBACK_RATE) / bpm);
                short[] beat0 = MixBeat(song.NotesAtBeat(0));
                short[] beat1 = MixBeat(song.NotesAtBeat(1));

                StartStreamingAudio(beat0, beat1);

                //Beats 0 and 1 have already been generated, so the next beet to generate is 2
                nextBeat = 2;
                BeatStarted?.Invoke(0, true);
            }
        }

        /// <summary>
        /// Combines the given set of notes into a single array of audio data
        /// </summary>
        private short[] MixBeat(Instrument.Note[] notes)
        {
            short[] beatData = new short[samplesPerBeat];
            if (notes.Length > 0)
            {
                for (int i = 0; i < samplesPerBeat; i++)
                {
                    int sampleSum = 0;
                    //Add together the samples from each note
                    for (int n = 0; n < notes.Length; n++)
                    {
                        if (i < notes[n].data.Length)
                            sampleSum += notes[n].data[i];
                    }
                    //Divide sample sum by 4 to reduce audio clipping.
                    sampleSum /= 4;

                    //Clamp to short range
                    short asShort;
                    if (sampleSum >= short.MaxValue)
                        asShort = short.MaxValue;
                    else if (sampleSum <= short.MinValue)
                        asShort = short.MinValue;
                    else
                        asShort = (short)sampleSum;

                    beatData[i] = asShort;
                }
            }
            return beatData;
        }

        /// <summary>
        /// Called every time a beat is completed
        /// </summary>
#if __ANDROID__
        private void OnStreamingAudioPeriodicNotification(object sender, AudioTrack.PeriodicNotificationEventArgs args)
#endif
#if __IOS__
        private void OnStreamingAudioPeriodicNotification(object sender, BufferCompletedEventArgs args)
#endif
        {
            int prevBeat = ((nextBeat - 1) + song.BeatCount) % song.BeatCount;
            //Call the BeatStarted callback on a separate thread
            BeatStarted?.BeginInvoke(prevBeat, false, null, null);

            short[] data = MixBeat(song.NotesAtBeat(nextBeat));

            //Make sure the track isnt disposed of after we check if it's playing
            lock (trackDisposedOfSyncObject)
            {
                if (IsPlaying)
                {
#if __ANDROID__
                    playingTrack.Write(data, 0, data.Length);
#endif
#if __IOS__
                    unsafe
                    {
                        fixed (short* beatData = data)
                        {
                            args.UnsafeBuffer->CopyToAudioData((IntPtr)beatData, data.Length * 2);
                        }
                    }
                    audioQueue.EnqueueBuffer(args.IntPtrBuffer, data.Length * 2, null);
#endif
                }
            }

            nextBeat = (nextBeat + 1) % song.BeatCount;
        }

        /// <summary>
        /// Plays a single note
        /// </summary>
        /// <param name="note"></param>
        public static void PlayNote(Instrument.Note note)
        {
#if __ANDROID__
            AudioTrack track = new AudioTrack(
                // Stream type
                Android.Media.Stream.Music,
                // Frequency
                44100,
                // Mono or stereo
                ChannelOut.Mono,
                // Audio encoding
                Android.Media.Encoding.Pcm16bit,
                // Length of the audio clip.
                (note.data.Length * 2),
                // Mode. Stream or static.
                AudioTrackMode.Static);

            track.Write(note.data, 0, note.data.Length);

            //Release the track after it's done playing
            track.SetPositionNotificationPeriod(track.BufferSizeInFrames);
            track.PeriodicNotification += (sender, args) =>
            {
                track.Release();
                track.Dispose();
            };

            track.Play();
#endif
#if __IOS__
            OutputAudioQueue queue = new OutputAudioQueue(AudioStreamBasicDescription.CreateLinearPCM(PLAYBACK_RATE, 1, 16, false));
            unsafe
            {
                AudioQueueBuffer* buffer;
                queue.AllocateBuffer(note.data.Length * 2, out buffer);

                fixed (short* beatData = note.data)
                {
                    buffer->CopyToAudioData((IntPtr)beatData, note.data.Length * 2);
                }

                queue.EnqueueBuffer((IntPtr)buffer, note.data.Length * 2, null);
            }

            queue.BufferCompleted += (sender, args) =>
            {
                queue.Dispose();
            };
            queue.Start();
#endif
        }

        private void StartStreamingAudio(short[] beat0, short[] beat1)
        {
#if __ANDROID__
            playingTrack = new AudioTrack(
                // Stream type
                Android.Media.Stream.Music,
                // Frequency
                44100,
                // Mono or stereo
                ChannelOut.Mono,
                // Audio encoding
                Android.Media.Encoding.Pcm16bit,
                // Length of the audio clip.
                (samplesPerBeat * 2) * 2, // Double buffering
                // Mode. Stream or static.
                AudioTrackMode.Stream);

            //Setup notifications at the end of beats
            playingTrack.PeriodicNotification += OnStreamingAudioPeriodicNotification;
            playingTrack.SetPositionNotificationPeriod(samplesPerBeat);

            playingTrack.Write(beat0, 0, beat0.Length);
            playingTrack.Write(beat1, 0, beat1.Length);
            playingTrack.Play();
#endif
#if __IOS__

            audioQueue = new OutputAudioQueue(streamDesc);
            unsafe
            {
                AudioQueueBuffer* buffer0;
                AudioQueueBuffer* buffer1;
                audioQueue.AllocateBuffer(beat0.Length * 2, out buffer0);
                audioQueue.AllocateBuffer(beat1.Length * 2, out buffer1);

                fixed (short* beatData0 = beat0)
                {
                    buffer0->CopyToAudioData((IntPtr)beatData0, beat0.Length * 2);
                }
                fixed (short* beatData1 = beat1)
                {
                    buffer1->CopyToAudioData((IntPtr)beatData1, beat1.Length * 2);
                }


                audioQueue.EnqueueBuffer((IntPtr)buffer0, beat0.Length * 2, null);
                audioQueue.EnqueueBuffer((IntPtr)buffer1, beat1.Length * 2, null);
            }

            audioQueue.BufferCompleted += OnStreamingAudioPeriodicNotification;
            audioQueue.Start();
#endif
        }

        /// <summary>
        /// Stops the currently playing audio
        /// </summary>
        public void StopPlaying()
        {
            lock (startStopSyncObject) //Use lock so track is not stopped while it is being started or begins stopping twice
            {
                if (!IsPlaying)
                    throw new InvalidOperationException("Audio is not playing");
#if __ANDROID__
                //We use pause instead of stop because stop waits a bit before ending audio playing
                playingTrack.Pause();

                lock (trackDisposedOfSyncObject)
                {
                    playingTrack.Release();
                    playingTrack.Dispose();
                    playingTrack = null;
                }
#endif
#if __IOS__
                audioQueue.Stop(true);
                lock (trackDisposedOfSyncObject)
                {
                    audioQueue.Dispose();
                    audioQueue = null;
                }
#endif
            }
        }
    }
}
