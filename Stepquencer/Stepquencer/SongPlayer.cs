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
            public short[] data;

            public Note(short[] data)
            {
                this.data = data;
            }
        }

        Dictionary<String, Note> namesToNotes;

        public SongPlayer()
        {
            assembly = typeof(MainPage).GetTypeInfo().Assembly;

            namesToNotes = new Dictionary<string, Note>();
        }

        public void LoadInstrument(String instrName)
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
            short[] dataAsShorts = new short[rawInstrumentData.Length / 2];
            //Start at 44 since 44 is the size of the WAV header.
            for(int i = 44; i < dataAsShorts.Length; i++)
            {
                dataAsShorts[i] = (short)(rawInstrumentData[2 * i] | (rawInstrumentData[2 * i + 1] << 8));
            }
            namesToNotes[instrName] = new Note(dataAsShorts);
        }

        public Note[][] MakeSimpleSong()
        {
            Note[][] song = new Note[16][];
            for (int t = 0; t < 16; t++)
            {
                List<Note> notesThisTimestep = new List<Note>();
                notesThisTimestep.Add(namesToNotes["Hi-Hat"]);
                if (t % 2 == 0)
                    notesThisTimestep.Add(namesToNotes["Bass Drum"]);
                if (t % 4 == 2)
                    notesThisTimestep.Add(namesToNotes["Snare"]);

                song[t] = notesThisTimestep.ToArray();
            }
            return song;
        }

        public short[] Mix(Note[][] song, int bpm)
        {
            int samplesPerBeat = ((60 * playbackRate) / bpm);

            short[] songData = new short[samplesPerBeat * song.Length];
            for (int b = 0; b < song.Length; b++)
            {
                Note[] notes = song[b];
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

        public void PlayAudio(short[] songData)
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
