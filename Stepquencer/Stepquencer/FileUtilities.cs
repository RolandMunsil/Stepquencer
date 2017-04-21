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
        public static Song LoadSongFromFile(String path)
        {
            using (StreamReader file = File.OpenText(path))
            {
                return LoadSongFromStream(file.BaseStream);
            }
        }

        public static Song LoadSongFromStream(Stream stream)
        {
            StreamReader file = new StreamReader(stream);

            String[] firstLineParts = file.ReadLine().Split('|');
            int totalBeats = int.Parse(firstLineParts[0].Split(' ')[0]);


            //Load instruments
            Instrument[] songInstruments;
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

            Song loadedSong = new Song(totalBeats, songInstruments, 240);

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
            int tempo = -1;
            if (int.TryParse(lastLine, out tempo))
            {
                loadedSong.Tempo = tempo;
            }

            //Copy 8-beat songs to each set of 8 beats
            if (loadedSong.BeatCount == 8)
            {
                Song copiedSong = new Song(16, loadedSong.Instruments, loadedSong.Tempo);
                for (int i = 0; i < 8; i++)
                {
                    foreach (Instrument.Note note in loadedSong.NotesAtBeat(i))
                    {
                        //Iterate over each set of 8 beats
                        for (int e = 0; e < 16 / 8; e++)
                        {
                            copiedSong.AddNote(note, i + (e * 8));
                        }
                    }
                }
                return copiedSong;
            }
            else
            {
                return loadedSong;
            }
        }

        public static void SaveSongToFile(Song songToSave, String songName)
        {
            String filePath = FileUtilities.PathToSongFile(songName);

            using (StreamWriter file = File.CreateText(filePath))
            {
                String instrumentNames = String.Join("|", songToSave.Instruments.Select(instr => instr.name));
                file.WriteLine($"{songToSave.BeatCount} total beats|{instrumentNames}");

                for (int i = 0; i < songToSave.BeatCount; i++)
                {
                    Instrument.Note[] notes = songToSave.NotesAtBeat(i);
                    file.WriteLine($"Beat {i}|{notes.Length}");
                    foreach (Instrument.Note note in songToSave.NotesAtBeat(i))
                    {
                        file.WriteLine($"{note.instrument.name}:{note.semitoneShift}");
                    }
                }
                file.WriteLine(songToSave.Tempo);
            }
        }
    }
}
