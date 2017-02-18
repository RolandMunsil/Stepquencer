using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;

namespace Stepquencer
{
    class SongPlayer
    {
#if __IOS__
        const String resourcePrefix = "Stepquencer.iOS.Instruments.";
#endif
#if __ANDROID__
        const String resourcePrefix = "Stepquencer.Droid.Instruments.";
#endif
        readonly Assembly assembly;
        const int playbackRate = 44100;

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

        public SongPlayer()
        {
            assembly = typeof(MainPage).GetTypeInfo().Assembly;
        }

        /// <summary>
        /// Loads the instrument with the given name and returns an array containing the
        /// note at various pitches
        /// </summary>
        /// <param name="instrName"></param>
        /// <returns>An array of notes. The index corresponds to the number of semitones the pitch of the
        /// original has been increased by. So if the original instrument was a C note, array[1] would be a C# version</returns>
        public Note[] LoadInstrument(String instrName)
        {
            String resourceString = $"{resourcePrefix}{instrName}.wav";

            //Read in data
            byte[] rawInstrumentData;
            using (Stream stream = assembly.GetManifestResourceStream(resourceString))
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
            //namesToNotes[instrName] = new Note(dataAsShorts);

            //Create pitched versions
            Note[] notes = new Note[13];
            notes[0] = new Note(dataAsShorts);
            for(int i = 1; i < 21; i++)
            {
                notes[i] = new Note(Resample(dataAsShorts, i));
            }
            return notes;
        }

        private short[] Resample(short[] instrData, int semitoneShift)
        {
            double mult = Math.Pow(2, -semitoneShift / 12.0);
            int outputSize = (int)(instrData.Length * mult);
            short[] output = new short[outputSize];
            for(int s = 0; s < outputSize; s++)
            {
                double sourcePos = s / mult;
                int left = (int)Math.Floor(sourcePos);
                output[s] = (short)(instrData[left] + ((sourcePos % 1) * (instrData[left + 1] - instrData[left])));
            }
            return output;
        }

        public void PlaySong(Note[][] song, int bpm)
        {
            PlayAudioData(Mix(song, bpm));
        }

        private short[] Mix(Note[][] song, int bpm)
        {
            int samplesPerBeat = ((60 * playbackRate) / bpm);

            short[] songData = new short[samplesPerBeat * song.Length];
            for (int b = 0; b < song.Length; b++)
            {
                List<Note> notesList = new List<Note>(song[b]);
                notesList.RemoveAll((no) => no.Equals(Note.None));
                Note[] notes = notesList.ToArray();
                for(int i = 0; i < samplesPerBeat; i++)
                {
                    int sampleSum = 0;
                    for(int n = 0; n < notes.Length; n++)
                    {
                        if(i < notes[n].data.Length)
                            sampleSum += notes[n].data[i];
                    }
                    short asShort;
                    if (sampleSum >= short.MaxValue)
                        asShort = short.MaxValue;
                    else if (sampleSum <= short.MinValue)
                        asShort = short.MinValue;
                    else
                        asShort = (short)sampleSum;

                    songData[i + (samplesPerBeat * b)] = asShort;
                }
            }
            return songData;
        }

        private void PlayAudioData(short[] songData)
        {
#if __IOS__
            throw new NotImplementedException();
#endif
#if __ANDROID__
            Android.Media.AudioTrack audioTrack = new Android.Media.AudioTrack(
                // Stream type
                Android.Media.Stream.Music,
                // Frequency
                44100,
                // Mono or stereo
                Android.Media.ChannelOut.Mono,
                // Audio encoding
                Android.Media.Encoding.Pcm16bit,
                // Length of the audio clip.
                songData.Length,
                // Mode. Stream or static.
                Android.Media.AudioTrackMode.Static);

            audioTrack.Write(songData, 0, songData.Length, Android.Media.WriteMode.Blocking);
            audioTrack.SetLoopPoints(0, audioTrack.BufferSizeInFrames, -1);
            audioTrack.Play();
#endif
        }
    }
}
