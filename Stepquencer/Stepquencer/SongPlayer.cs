using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Diagnostics;

#if __ANDROID__
using Android.Media;
#endif

namespace Stepquencer
{
    class SongPlayer : IDisposable
    {
#if __IOS__
        const String resourcePrefix = "Stepquencer.iOS.Instruments.";
#endif
#if __ANDROID__
        const String resourcePrefix = "Stepquencer.Droid.Instruments.";
        AudioTrack playingTrack;
#endif
        readonly Assembly assembly;
        const int playbackRate = 44100;

        public delegate void OnBeatDelegate(int beatNum, bool firstBeat);
        public event OnBeatDelegate BeatStarted;

        HashSet<Note>[] songDataReference;
        int samplesPerBeat;
        int nextBeat;
        int totalBeats;

        public struct Note
        {
            //Use to specify that there is no note corresponding to a given beat & pitch
            public static Note None = new Note(null);

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

            public Note AtPitch(int semitoneShift)
            {
                if (semitoneShift == 0)
                    return new Note(unpitchedData);

                Note pitchedNote;
                if(pitchedNotes.TryGetValue(semitoneShift, out pitchedNote))
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

        public SongPlayer(HashSet<Note>[] songDataReference)
        {
            this.songDataReference = songDataReference;
            assembly = typeof(MainPage).GetTypeInfo().Assembly;
        }

        public void Dispose()
        {
#if __ANDROID__
            playingTrack.Release();
            playingTrack.Dispose();
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
            for(int i = 22; i < dataAsShorts.Length; i++)
            {
                dataAsShorts[i - 22] = (short)(rawInstrumentData[2 * i] | (rawInstrumentData[2 * i + 1] << 8));
            }

            return new Instrument(dataAsShorts);
        }



        public void BeginPlaying(int bpm)
        {
            //TODO: this takes a long time (~100ms) - perhaps start it on a different thread?
            samplesPerBeat = ((60 * playbackRate) / bpm);
            totalBeats = songDataReference.GetLength(0);
            short[] beat0 = MixBeat(GetNotes(0));
            short[] beat1 = MixBeat(GetNotes(1));

            StartStreamingAudio(beat0);
            AppendStreamingAudio(beat1);
            nextBeat = 2;
            BeatStarted?.Invoke(0, true);
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
        {
            BeatStarted?.BeginInvoke(((nextBeat - 1) + totalBeats) % totalBeats, false, null, null);

            AppendStreamingAudio(MixBeat(GetNotes(nextBeat)));
            nextBeat = (nextBeat + 1) % totalBeats;
        }
#endif

        private void StartStreamingAudio(short[] initialData)
        {
#if __ANDROID__
            if (playingTrack != null)
            {
                if (playingTrack.PlayState == PlayState.Playing)
                    throw new InvalidOperationException("Audio is already playing.");
            }

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
        }

        private void AppendStreamingAudio(short[] data)
        {
#if __ANDROID__
            if (playingTrack == null || playingTrack.PlayState != PlayState.Playing)
                throw new InvalidOperationException("playingTrack has not been initialized. Call StartStreamingAudio() first");

            playingTrack.Write(data, 0, data.Length);
#endif
        }

        private void StopStreamingAudio()
        {
#if __ANDROID__
            if (playingTrack == null || playingTrack.PlayState != PlayState.Playing)
                throw new InvalidOperationException("Audio is not playing");

            playingTrack.Pause();
            //playingTrack.Flush();
            playingTrack.Release();
            playingTrack.Dispose();
#endif
        }
    }
}
