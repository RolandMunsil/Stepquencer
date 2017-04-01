﻿using System;
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
        public class Note
        {
            /// <summary>
            /// The raw audio data representing this note
            /// </summary>
            public readonly short[] data;

            #region Used for constructing button layouts from a Song
            /// <summary>
            /// The instrument that generated this note.
            /// </summary>
            public readonly Instrument instrument;

            /// <summary>
            /// The pitch shift of this note from the original audio file
            /// </summary>
            public readonly int semitoneShift;
            #endregion

            public Note(short[] data, Instrument instrument, int semitoneShift)
            {
                this.data = data;
                this.instrument = instrument;
                this.semitoneShift = semitoneShift;
            }
        }

        /// <summary>
        /// Used to get the audio files embedded in our program
        /// </summary>
        private static Assembly assembly = typeof(MainPage).GetTypeInfo().Assembly;
#if __IOS__
        const String resourcePrefix = "Stepquencer.iOS.Instruments.";
#endif
#if __ANDROID__
        const String resourcePrefix = "Stepquencer.Droid.Instruments.";
#endif
        /// <summary>
        /// The name of the audio file this instrument is based on
        /// </summary>
        public String instrumentName;

        /// <summary>
        /// The raw audio data for the original audio file
        /// </summary>
        private short[] unpitchedData;

        /// <summary>
        /// A dictionary mapping pitches to Notes. Used so that notes are not regenerated every time they are needed
        /// </summary>
        private Dictionary<int, Note> pitchedNotes;

        private Instrument(short[] unpitchedData, String instrumentName)
        {
            this.unpitchedData = unpitchedData;
            pitchedNotes = new Dictionary<int, Note>(72);
            pitchedNotes.Add(0, new Note(unpitchedData, this, 0));
            this.instrumentName = instrumentName;
        }

        /// <summary>
        /// Creates an instrument based on the file Instruments/<code>instrName</code>.wav
        /// </summary>
        /// <param name="instrName">The name of the wav file the created instrument will be based on</param>
        /// <returns>An instrument</returns>
        public static Instrument LoadByName(String instrName)
        {
            //Generate the path to the audio file
            String resourceString = $"{resourcePrefix}{instrName}.wav";

            //Read in data
            byte[] rawInstrumentData;
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceString))
            {
                int streamLength = (int)stream.Length;
                rawInstrumentData = new byte[streamLength];
                stream.Read(rawInstrumentData, 0, streamLength);
            }

            //Convert to shorts
            //Note that we skip 44 bytes because that is the size of the WAV header
            short[] dataAsShorts = new short[(rawInstrumentData.Length - 44) / 2];
            for (int i = 22; i < dataAsShorts.Length; i++)
            {
                dataAsShorts[i - 22] = (short)(rawInstrumentData[2 * i] | (rawInstrumentData[2 * i + 1] << 8));
            }

            return new Instrument(dataAsShorts, instrName);
        }

        /// <summary>
        /// Returns a note representing this instrument at the given pitch
        /// </summary>
        /// <param name="semitoneShift">The number of semitones the returned note will differ from the base audio file</param>
        public Note AtPitch(int semitoneShift)
        {
            //Either return an already generated note or generate the note
            Note pitchedNote;
            if (pitchedNotes.TryGetValue(semitoneShift, out pitchedNote))
            {
                return pitchedNote;
            }
            else
            {
                Note note = new Note(Resample(semitoneShift), this, semitoneShift);
                pitchedNotes[semitoneShift] = note;
                return note;
            }
        }

        /// <summary>
        /// Generates audio data for this instrument at the given pitch by resampling the audio data
        /// </summary>
        /// <param name="semitoneShift">Semitones to shift the base audio file by</param>
        /// <returns></returns>
        private short[] Resample(int semitoneShift)
        {
            //The amount to stretch the audio by
            double mult = Math.Pow(2, -semitoneShift / 12.0);

            int outputSize = (int)(unpitchedData.Length * mult);
            short[] output = new short[outputSize];
            for (int s = 0; s < outputSize; s++)
            {
                //The position to sample from
                double sourcePos = s / mult;

                //Linearly interpolate between the samples before and after the given position
                int left = (int)Math.Floor(sourcePos);
                short rightSample = left + 1 < unpitchedData.Length ? unpitchedData[left + 1] : (short)0;

                output[s] = (short)(unpitchedData[left] + ((sourcePos % 1) * (rightSample - unpitchedData[left])));
            }
            return output;
        }
    }
}