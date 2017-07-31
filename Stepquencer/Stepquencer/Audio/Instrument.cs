using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using Xamarin.Forms;

namespace Stepquencer
{
    [System.Diagnostics.DebuggerDisplay("{instrumentName}")]
    public class Instrument
    {
        [System.Diagnostics.DebuggerDisplay("{instrument.instrumentName} @ {semitoneShift}")]
        /// <summary>
        /// Represents a specific instrument played a specific pitch
        /// </summary>
        public class Note
        {
            /// <summary>
            /// The raw audio data representing this note
            /// </summary>
            public readonly short[] data;

            /// <summary>
            /// The instrument that generated this note.
            /// </summary>
            public readonly Instrument instrument;

            /// <summary>
            /// The pitch shift of this note from the original audio file
            /// </summary>
            public readonly int semitoneShift;

            public Note(short[] data, Instrument instrument, int semitoneShift)
            {
                this.data = data;
                this.instrument = instrument;
                this.semitoneShift = semitoneShift;
            }
        }

        #region Static variables
        /// <summary>
        /// Maps instrument names to colors. Loaded from the _colors.txt file
        /// </summary>
        public static Dictionary<String, Color> colorMap;

        /// <summary>
        /// Stores all instruments that have been loaded.  Used so that instruments are not loaded every time they are needed
        /// </summary>
        private static Dictionary<String, Instrument> loadedInstruments = new Dictionary<string, Instrument>();

        /// <summary>
        /// The string representations of different pitches
        /// </summary>
        private static readonly String[] NOTE_NAMES = { "C", "C♯/D♭", "D", "D♯/E♭", "E", "F", "F♯/G♭", "G", "G♯/A♭", "A", "A♯/B♭", "B" };
        #endregion

        /// <summary>
        /// The name of the audio file this instrument is based on
        /// </summary>
        public String name;

        /// <summary>
        /// The color that this instrument is represented with
        /// </summary>
        public Color color;

        /// <summary>
        /// The raw audio data for the original audio file
        /// </summary>
        private short[] unpitchedData;

        /// <summary>
        /// A dictionary mapping pitches to notes. Used so that notes are not regenerated every time they are needed
        /// </summary>
        private Dictionary<int, Note> pitchedNotes;

        private Instrument(short[] unpitchedData, String instrumentName)
        {
            this.unpitchedData = unpitchedData;
            pitchedNotes = new Dictionary<int, Note>(72);
            pitchedNotes.Add(0, new Note(unpitchedData, this, 0));
            this.name = instrumentName;
            this.color = colorMap[instrumentName];
        }

        /// <summary>
        /// Loads information about instrument<->color mappings from the map file
        /// </summary>
        public static void LoadColorMap()
        {
            colorMap = new Dictionary<String, Color>();

            //Read in data
            using (StreamReader stream = new StreamReader(FileUtilities.LoadEmbeddedResource("Instruments._colors.txt")))
            {
                while (stream.Peek() != -1) //While there are no more lines left to read
                {
                    string[] nameAndColor = stream.ReadLine().Split('|');
                    colorMap.Add(nameAndColor[0], Color.FromHex("#" + nameAndColor[1]));
                }
            }
        }

        /// <summary>
        /// Loads or gets an instrument based on the file Instruments/<code>instrName</code>.wav
        /// </summary>
        public static Instrument GetByName(String instrName)
        {
            if(colorMap == null)
            {
                LoadColorMap();
            }

            //If the instument has already been loaded, return it.
            Instrument dictInstr;
            if(loadedInstruments.TryGetValue(instrName, out dictInstr))
            {
                return dictInstr;
            }

            //Read in audio file as raw bytes
            byte[] rawInstrumentData;
            using (System.IO.Stream stream = FileUtilities.LoadEmbeddedResource($"Instruments.{instrName}.wav"))
            {
                int streamLength = (int)stream.Length;
                rawInstrumentData = new byte[streamLength];
                stream.Read(rawInstrumentData, 0, streamLength);
            }

            //Reinterperet bytes as shorts. We do this because the wav data is in 16-bit unsigned format
            //Note that we skip the first 44 bytes because that is the size of the WAV header
            short[] dataAsShorts = new short[(rawInstrumentData.Length - 44) / 2];
            for (int i = 0; i < dataAsShorts.Length; i++)
            {
                int srcPos = (i * 2) + 44;
                dataAsShorts[i] = (short)(rawInstrumentData[srcPos] | (rawInstrumentData[srcPos + 1] << 8));

                //Divide samples by 4 to reduce clipping
                dataAsShorts[i] /= 4;
            }

            Instrument instrument = new Instrument(dataAsShorts, instrName);
            loadedInstruments.Add(instrName, instrument);
            return instrument;
        }

        /// <summary>
        /// Maps a semitone shift to a note string
        /// </summary>
        public static String SemitoneShiftToString(int semitoneShift)
        {
            //This modulo code makes it so the shifts loop around, i.e. -12, 0, and 12 all map to the same string
            int index = (((semitoneShift % 12) + 12) % 12);
            return NOTE_NAMES[index];
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
                //Resample the base audio data to pitch it up or down
                Note note = new Note(Resample(semitoneShift), this, semitoneShift);
                pitchedNotes[semitoneShift] = note;
                return note;
            }
        }

        /// <summary>
        /// Generates audio data for this instrument at the given pitch by resampling the audio data
        /// </summary>
        /// <param name="semitoneShift">Semitones to shift the base audio file by</param>
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

                //Special case for when the sample is past the end of the original data
                short rightSample = left + 1 < unpitchedData.Length ? unpitchedData[left + 1] : (short)0;

                output[s] = (short)(unpitchedData[left] + ((sourcePos % 1) * (rightSample - unpitchedData[left])));
            }
            return output;
        }
    }
}
