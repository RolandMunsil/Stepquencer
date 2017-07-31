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
        //The ios object that manages audio playback
        OutputAudioQueue audioQueue;

        //Describes the format of the data we will be giving to audioQueue
        AudioStreamBasicDescription streamDesc;
#endif
#if __ANDROID__
        //The android object that manages audio playback
        AudioTrack playingTrack;
#endif
        //Used to synchronize multithreaded accesses
        object trackDisposedOfSyncObject = new object();
        object startStopSyncObject = new object();

        //Audio playback rate in Hz
        public const int PLAYBACK_RATE = 44100;

        //An event triggered at the start of every beat
        public delegate void OnBeatDelegate(int beatNum, bool firstBeat);
        public event OnBeatDelegate BeatStarted;

        //The song currently being played
        Song song;

        //The length, in samples, of beats
        int samplesPerBeat;

        //The next beat to generate
        int nextBeat;

        /// <summary>
        /// This class is used to make notes play for longer than a beat
        /// </summary>
        private class PlayingNote
        {
            public Instrument.Note note;

            //Where to start playback of the note
            public int startSample;

            public PlayingNote(Instrument.Note note, int startSample)
            {
                this.note = note;
                this.startSample = startSample;
            }
        }
        //A list of notes that are currently playing
        List<PlayingNote> playingNotes;

        /// <summary>
        /// Whether the song is currently being played.
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                //Lock so that the track is not disposed as we are checking to see if it's null.
                //Probably not necessary but better safe than sorry
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

        public SongPlayer()
        {
            playingNotes = new List<PlayingNote>();
#if __IOS__
            streamDesc = AudioStreamBasicDescription.CreateLinearPCM(PLAYBACK_RATE, 1, 16, false);  
#endif
        }

        /// <summary>
        /// Begins playing a song
        /// </summary>
        public void BeginPlaying(Song song)
        {
            //This lock is used to ensure that starting and stopping of songs do not happen at the same time.
            lock (startStopSyncObject)
            {
                if (IsPlaying)
                {
                    throw new InvalidOperationException("Audio is already playing.");
                }
                //Need to keep track of the playing song beyond this method
                this.song = song;

                //Get rid of any old playing notes left over from the last time a song was played
                playingNotes.Clear();

                //(60 secs/min * PLAYBACK_RATE samples/sec) / song.Tempo beats/min = samples/beat
                samplesPerBeat = ((60 * PLAYBACK_RATE) / song.Tempo);

                //To start playback we need some initial data
                short[] beat0 = MixBeat(song.NotesAtBeat(0));
                short[] beat1 = MixBeat(song.NotesAtBeat(1));

                StartStreamingAudio(beat0, beat1);

                //Beats 0 and 1 have already been generated, so the next beet to generate is 2
                nextBeat = 2;
                //Call the BeatStarted event, since playback has started
                BeatStarted?.Invoke(0, true);
            }
        }

        /// <summary>
        /// Combines the given set of notes into a single buffer of audio data
        /// </summary>
        private short[] MixBeat(Instrument.Note[] notes)
        {
            //Figure out which instruments will begin being played this beat.
            HashSet<Instrument> instrumentsStartingThisBeat = new HashSet<Instrument>();
            for(int i = 0; i < notes.Length; i++)
            {
                instrumentsStartingThisBeat.Add(notes[i].instrument);
            }
            //Stop any playing notes of instruments playing this beat. This means notes will play until
            //another note of the same instrument begins playing.
            for(int i = 0; i < playingNotes.Count; i++)
            {
                if(instrumentsStartingThisBeat.Contains(playingNotes[i].note.instrument))
                {
                    playingNotes.RemoveAt(i);
                    i--;
                }
            }

            //The array we will write the mixed beat data into
            short[] beatData = new short[samplesPerBeat];

            //To mix the data we simply add together all of the samples from all of the notes
            for (int i = 0; i < samplesPerBeat; i++)
            {
                int sampleSum = 0;
                //Add the samples from the notes starting this beat
                for (int n = 0; n < notes.Length; n++)
                {
                    if (i < notes[n].data.Length)
                        sampleSum += notes[n].data[i];
                }
                //Add the samples from notes that are playing from previous beats
                for (int n = 0; n < playingNotes.Count; n++)
                {
                    int s = playingNotes[n].startSample + i;
                    if (s < playingNotes[n].note.data.Length)
                        sampleSum += playingNotes[n].note.data[s];
                }

                //Clamp to the range of a short
                short asShort;
                if (sampleSum >= short.MaxValue)
                    asShort = short.MaxValue;
                else if (sampleSum <= short.MinValue)
                    asShort = short.MinValue;
                else
                    asShort = (short)sampleSum;

                beatData[i] = asShort;
            }

            //Remove any playing notes that have no more data to play
            for(int i = 0; i < playingNotes.Count; i++)
            {
                if(playingNotes[i].startSample + samplesPerBeat >= playingNotes[i].note.data.Length)
                {
                    playingNotes.RemoveAt(i);
                    i--;
                }
                else
                {
                    playingNotes[i].startSample += samplesPerBeat;
                }
            }
            //Add all of the notes that started this beat to the list of playing notes
            foreach (Instrument.Note note in notes)
            {
                if(note.data.Length > samplesPerBeat)
                {
                    playingNotes.Add(new PlayingNote(note, samplesPerBeat));
                }
            }

            return beatData;
        }

        /// <summary>
        /// Called at the start of every beat
        /// </summary>
#if __ANDROID__
        private void OnStreamingAudioPeriodicNotification(object sender, AudioTrack.PeriodicNotificationEventArgs args)
#endif
#if __IOS__
        private void OnStreamingAudioPeriodicNotification(object sender, BufferCompletedEventArgs args)
#endif
        {
            //nextBeat refers to the next beat to generate - the current beat is the one before nextBeat
            int currentBeat = ((nextBeat - 1) + song.BeatCount) % song.BeatCount;
            //Call the BeatStarted callback on a separate thread
            BeatStarted?.BeginInvoke(currentBeat, false, null, null);

            //Mix the audio data for this beat
            short[] data = MixBeat(song.NotesAtBeat(nextBeat));

            //Make sure the track isnt disposed of after we check if it's playing
            lock (trackDisposedOfSyncObject)
            {
                if (IsPlaying)
                {
#if __ANDROID__
                    //Add the audio data to the track
                    playingTrack.Write(data, 0, data.Length);
#endif
#if __IOS__
                    //Copy the audio data to the buffer passed in to this method
                    unsafe
                    {
                        fixed (short* beatData = data)
                        {
                            args.UnsafeBuffer->CopyToAudioData((IntPtr)beatData, data.Length * 2);
                        }
                    }
                    //Add the buffer to the audio queue
                    audioQueue.EnqueueBuffer(args.IntPtrBuffer, data.Length * 2, null);
#endif
                }
            }

            nextBeat = (nextBeat + 1) % song.BeatCount;
        }

        

        /// <summary>
        /// Start the platform-specific audio object and give it some initial data (beat0 and beat1)
        /// </summary>
        private void StartStreamingAudio(short[] beat0, short[] beat1)
        {
#if __ANDROID__
            playingTrack = new AudioTrack(
                // Stream type
                Android.Media.Stream.Music,
                // Frequency
                PLAYBACK_RATE,
                // Mono or stereo
                ChannelOut.Mono,
                // Audio encoding
                Android.Media.Encoding.Pcm16bit,
                // Length of the audio clip  in bytes
                (samplesPerBeat * 2) * 2, //Multiply by 2 because we want two beats to fit in the playingTrack's memory
                // Mode. Stream or static.
                AudioTrackMode.Stream);

            //Set up notifications at the end of beats
            playingTrack.PeriodicNotification += OnStreamingAudioPeriodicNotification;
            playingTrack.SetPositionNotificationPeriod(samplesPerBeat);

            //Write the initial data and begin playing
            playingTrack.Write(beat0, 0, beat0.Length);
            playingTrack.Write(beat1, 0, beat1.Length);
            playingTrack.Play();
#endif
#if __IOS__

            audioQueue = new OutputAudioQueue(streamDesc);
            unsafe
            {
                //Allocate two buffers to store audio data
                AudioQueueBuffer* buffer0;
                AudioQueueBuffer* buffer1;
                audioQueue.AllocateBuffer(beat0.Length * 2, out buffer0);
                audioQueue.AllocateBuffer(beat1.Length * 2, out buffer1);

                //Copy initial audio data to the buffers
                fixed (short* beatData0 = beat0)
                {
                    buffer0->CopyToAudioData((IntPtr)beatData0, beat0.Length * 2);
                }
                fixed (short* beatData1 = beat1)
                {
                    buffer1->CopyToAudioData((IntPtr)beatData1, beat1.Length * 2);
                }
                
                //Add the buffers to the queue
                audioQueue.EnqueueBuffer((IntPtr)buffer0, beat0.Length * 2, null);
                audioQueue.EnqueueBuffer((IntPtr)buffer1, beat1.Length * 2, null);
            }

            //Set up periodic notifications
            audioQueue.BufferCompleted += OnStreamingAudioPeriodicNotification;
            audioQueue.Start();
#endif
        }

        /// <summary>
        /// Stops the currently playing audio
        /// </summary>
        public void StopPlaying()
        {
            //This lock is used to ensure that starting and stopping of songs do not happen at the same time.
            lock (startStopSyncObject)
            {
                if (!IsPlaying)
                    throw new InvalidOperationException("Audio is not playing");
#if __ANDROID__
                //We use pause instead of stop because pause stops playing immediately
                playingTrack.Pause();

                //Lock track disposal so the track is never in a state where it is disposed/released but not null
                lock (trackDisposedOfSyncObject)
                {
                    playingTrack.Release();
                    playingTrack.Dispose();
                    playingTrack = null;
                }
#endif
#if __IOS__
                //Pass true to stop immediately
                audioQueue.Stop(true);

                //Lock track disposal so the track is never in a state where it is disposed but not null
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
