using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Stepquencer
{
    static class FileUtilities
    {
        /// <summary>
        /// Used to get embedded resources
        /// </summary>
        private static Assembly assembly = typeof(MainPage).GetTypeInfo().Assembly;
#if __IOS__
        private const String resourcePrefix = "Stepquencer.iOS.";
#endif
#if __ANDROID__
        private const String resourcePrefix = "Stepquencer.Droid.";
#endif

        public static Stream LoadEmbeddedResource(String resourceName)
        {
            return assembly.GetManifestResourceStream(resourcePrefix + resourceName);
        }

        public static String PathToSongDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "stepsongs/");
            }
        }


        public static String PathToSongFile(String songName)
        {
            String documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            String savePath = Path.Combine(documentsPath, "stepsongs/");
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            return Path.Combine(savePath, $"{songName}.txt");
        }

        /// <summary>
        /// Given a file path, returns the name of the actual song
        /// </summary>
        /// <returns>The song name from file path.</returns>
        public static String SongNameFromFilePath(String path)
        {
            int start = path.IndexOf("stepsongs") + "stepsongs".Length + 1;
            int end = path.LastIndexOf('.');
            return path.Substring(start, end - start);
        }

        /// <summary>
        /// Loads a song given its name
        /// </summary>
        /// <returns>The song from file.</returns>
        public static Song LoadSongFromFile(String path, out Instrument[] songInstruments, out int tempo)
        {
            using (StreamReader file = File.OpenText(path))
            {
                return LoadSongFromStream(file.BaseStream, out songInstruments, out tempo);
            }
        }

        public static Song LoadSongFromStream(Stream stream, out Instrument[] songInstruments, out int tempo)
        {
            StreamReader file = new StreamReader(stream);

            String[] firstLineParts = file.ReadLine().Split('|');
            int totalBeats = int.Parse(firstLineParts[0].Split(' ')[0]);
            Song loadedSong = new Song(totalBeats);

            //Load instruments
            if (firstLineParts.Length > 1)
            {
                songInstruments = new Instrument[firstLineParts.Length - 1];
                for (int i = 1; i < firstLineParts.Length; i++)
                {
                    songInstruments[i - 1] = Instrument.GetByName(firstLineParts[i]);
                }
            }
            else
            {
                String[] oldDefaultInstruments = { "Snare", "YRM1xAtmosphere", "SlapBassLow", "HiHat" };
                //If no instruments, use default instruments (for backwards compatability)
                songInstruments = oldDefaultInstruments.Select(str => Instrument.GetByName(str)).ToArray();
            }

            for (int i = 0; i < totalBeats; i++)
            {
                String header = file.ReadLine();
                if (!header.Contains($"Beat {i}"))
                    throw new Exception("Invalid file or bug in file loader");
                int numNotes = int.Parse(header.Split('|')[1]);

                for (int n = 0; n < numNotes; n++)
                {
                    String[] noteStringParts = file.ReadLine().Split(':');
                    String instrName = noteStringParts[0];
                    int semitoneShift = int.Parse(noteStringParts[1]);

                    //To allow loading of songs saved before a lot of instrument names were changed
                    if (instrName.Contains(' ') || instrName.Contains('-'))
                    {
                        instrName = instrName.Replace(" ", "").Replace("-", "");
                    }

                    Instrument.Note note = Instrument.GetByName(instrName).AtPitch(semitoneShift);
                    loadedSong.AddNote(note, i);
                }
            }
            String lastLine = file.ReadLine();
            tempo = -1;
            int.TryParse(lastLine, out tempo);

            return loadedSong;
        }
    }
}
