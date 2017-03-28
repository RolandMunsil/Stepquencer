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
    class SongPlayer : IDisposable
    {
#if __IOS__
        const String resourcePrefix = "Stepquencer.iOS.Instruments.";
        OutputAudioQueue audioQueue;
        AudioStreamBasicDescription streamDesc;


#endif
#if __ANDROID__
        const String resourcePrefix = "Stepquencer.Droid.Instruments.";
        AudioTrack playingTrack;
#endif

        object trackDisposedOfSyncObject = new object();


        readonly Assembly assembly;
        const int playbackRate = 44100;
        object startStopSyncObject = new object();

        public delegate void OnBeatDelegate(int beatNum, bool firstBeat);
        public event OnBeatDelegate BeatStarted;

        HashSet<Note>[] songDataReference;
        int samplesPerBeat;
        int nextBeat;
        int totalBeats;

        /// <summary>
        /// Represents a specific instrument played a specific pitch
        /// </summary>
        public struct Note
        {
            public readonly short[] data;

            public Note(short[] data)
            {
                this.data = data;
            }
        }

        public class Instrument
        {
            private short[] unpitchedData;
            private Dictionary<int, Note> pitchedNotes;

            public Instrument(short[] unpitchedData)
            {
                this.unpitchedData = unpitchedData;
                pitchedNotes = new Dictionary<int, Note>(72);
            }

            /// <summary>
            /// Returns a note representing this instrument at the given pitch
            /// </summary>
            /// <param name="semitoneShift">The number of semitones the returned note will differ from the base audio file</param>
            public Note AtPitch(int semitoneShift)
            {
                if (semitoneShift == 0)
                    return new Note(unpitchedData);

                //Either return an already generated note or generate the note
                Note pitchedNote;
                if (pitchedNotes.TryGetValue(semitoneShift, out pitchedNote))
                {
                    return pitchedNote;
                }
                else
                {
                    Note note = new Note(Resample(semitoneShift));
                    pitchedNotes[semitoneShift] = note;
                    return note;
                }
            }

            private short[] Resample(int semitoneShift)
            {
                double mult = Math.Pow(2, -semitoneShift / 12.0);
                int outputSize = (int)(unpitchedData.Length * mult);
                short[] output = new short[outputSize];
                for (int s = 0; s < outputSize; s++)
                {
                    double sourcePos = s / mult;
                    int left = (int)Math.Floor(sourcePos);
                    output[s] = (short)(unpitchedData[left] + ((sourcePos % 1) * (unpitchedData[left + 1] - unpitchedData[left])));
                }
                return output;
            }
        }

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

        public SongPlayer(HashSet<Note>[] songDataReference)
        {
            this.songDataReference = songDataReference;
            assembly = typeof(MainPage).GetTypeInfo().Assembly;

#if __IOS__
            streamDesc = AudioStreamBasicDescription.CreateLinearPCM(44100, 1, 16, false);  // Might need to check if little or big endian
#endif
        }

        public void Dispose()
        {
#if __ANDROID__
            playingTrack.Release();
            playingTrack.Dispose();
            playingTrack = null;
#endif
        }

        public Instrument LoadInstrument(String instrName)
        {
            String resourceString = $"{resourcePrefix}{instrName}.wav";

            //Read in data
            byte[] rawInstrumentData;
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceString))
            {
                int streamLength = (int)stream.Length;
                rawInstrumentData = new byte[streamLength];
                stream.Read(rawInstrumentData, 0, streamLength);
            }

            //Convert to short
            short[] dataAsShorts = new short[(rawInstrumentData.Length - 44) / 2];
            //Start at 22 since 22 is 2 * the size of the WAV header.
            for (int i = 22; i < dataAsShorts.Length; i++)
            {
                dataAsShorts[i - 22] = (short)(rawInstrumentData[2 * i] | (rawInstrumentData[2 * i + 1] << 8));
            }

            return new Instrument(dataAsShorts);
        }



        public void BeginPlaying(int bpm)
        {
            lock (startStopSyncObject) //Use lock so track is not stopped while it is being started
            {
                if (IsPlaying)
                {
                    throw new InvalidOperationException("Audio is already playing.");
                }

                //TODO: this takes a long time (~100ms) - perhaps start it on a different thread?
                samplesPerBeat = ((60 * playbackRate) / bpm);
                totalBeats = songDataReference.GetLength(0);
                short[] beat0 = MixBeat(GetNotes(0));
                short[] beat1 = MixBeat(GetNotes(1));
#if __ANDROID__
                StartStreamingAudio(beat0);
                AppendStreamingAudio(beat1);
#endif
#if __IOS__
                StartStreamingAudio(beat0, beat1);
#endif
                nextBeat = 2;
                BeatStarted?.Invoke(0, true);
            }
        }

        public void StopPlaying()
        {
            StopStreamingAudio();
        }

        private Note[] GetNotes(int beat)
        {
            HashSet<Note> beatData = songDataReference[beat];
            Note[] notes;
            lock (songDataReference)
            {
                notes = new Note[beatData.Count];
                songDataReference[beat].CopyTo(notes);
            }
            return notes;
        }

        private short[] MixBeat(Note[] notes)
        {
            short[] beatData = new short[samplesPerBeat * 2];
            if (notes.Length > 0)
            {
                //TODO: Use Parallel.For()?
                for (int i = 0; i < samplesPerBeat; i++)
                {
                    int sampleSum = 0;
                    for (int n = 0; n < notes.Length; n++)
                    {
                        if (i < notes[n].data.Length)
                            sampleSum += notes[n].data[i];
                    }
                    //Reduce audio clipping.
                    sampleSum /= 4;
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

#if __ANDROID__
        private void OnStreamingAudioPeriodicNotification(object sender, AudioTrack.PeriodicNotificationEventArgs args)
#endif
#if __IOS__
        private void OnStreamingAudioPeriodicNotification(object sender, BufferCompletedEventArgs args)
#endif
        {
            BeatStarted?.BeginInvoke(((nextBeat - 1) + totalBeats) % totalBeats, false, null, null);

#if __ANDROID__
            AppendStreamingAudio(MixBeat(GetNotes(nextBeat)));
#endif
#if __IOS__
            unsafe
            {
                AppendStreamingAudio(args.UnsafeBuffer, MixBeat(GetNotes(nextBeat)));
            }
#endif
            nextBeat = (nextBeat + 1) % totalBeats;
        }

#if __ANDROID__
        private void StartStreamingAudio(short[] initialData)
#endif
#if __IOS__
        private void StartStreamingAudio(short[] beat0, short[] beat1)
#endif
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
                (initialData.Length * 2) * 2, // Double buffering
                // Mode. Stream or static.
                AudioTrackMode.Stream);

            playingTrack.PeriodicNotification += OnStreamingAudioPeriodicNotification;
            playingTrack.SetPositionNotificationPeriod(initialData.Length);

            playingTrack.Write(initialData, 0, initialData.Length);
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
                AppendStreamingAudio(buffer0, beat0);
                AppendStreamingAudio(buffer1, beat1);
            }

            audioQueue.BufferCompleted += OnStreamingAudioPeriodicNotification;
            audioQueue.Start();
#endif
        }
#if __ANDROID__
        private void AppendStreamingAudio(short[] data)
#endif
#if __IOS__
        private unsafe void AppendStreamingAudio(AudioQueueBuffer* buffer, short[] data)
#endif
        {

            lock (trackDisposedOfSyncObject)
            {
                if (IsPlaying)
                {
#if __ANDROID__
                    playingTrack.Write(data, 0, data.Length);
#endif
#if __IOS__

                    fixed (short* beatData = data)
                    {
                        buffer->CopyToAudioData((IntPtr)beatData, data.Length * 2);
                    }


                    audioQueue.EnqueueBuffer((IntPtr) buffer, data.Length * 2, null);
#endif
                }
            }

        }

        private void StopStreamingAudio()
        {

            lock (startStopSyncObject) //Use lock so track is not stopped while it is being started or begins stopping twice
            {
                if (!IsPlaying)
                    throw new InvalidOperationException("Audio is not playing");
#if __ANDROID__
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
