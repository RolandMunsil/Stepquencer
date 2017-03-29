using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Stepquencer
{
    public class Instrument
    {
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

        private static Assembly assembly = typeof(MainPage).GetTypeInfo().Assembly;
#if __IOS__
        const String resourcePrefix = "Stepquencer.iOS.Instruments.";
#endif
#if __ANDROID__
        const String resourcePrefix = "Stepquencer.Droid.Instruments.";
#endif

        private short[] unpitchedData;
        private Dictionary<int, Note> pitchedNotes;

        private Instrument(short[] unpitchedData)
        {
            this.unpitchedData = unpitchedData;
            pitchedNotes = new Dictionary<int, Note>(72);
        }

        public static Instrument LoadByName(String instrName)
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
}
