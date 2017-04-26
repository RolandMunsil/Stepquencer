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

            //Get total beats
            String[] firstLineParts = file.ReadLine().Split('|');
            int totalBeats = int.Parse(firstLineParts[0].Split(' ')[0]);

            //Load instruments
            Instrument[] songInstruments = new Instrument[firstLineParts.Length - 1];
            for (int i = 1; i < firstLineParts.Length; i++)
            {
                songInstruments[i - 1] = Instrument.GetByName(firstLineParts[i]);
            }

            //Make song with default tempo
            Song loadedSong = new Song(totalBeats, songInstruments, -1);

            //Load notes
            for (int i = 0; i < totalBeats; i++)
            {
                String header = file.ReadLine();
                if (!header.Contains($"Beat {i}")) throw new Exception("Invalid file or bug in file loader");
                int numNotes = int.Parse(header.Split('|')[1]);

                for (int n = 0; n < numNotes; n++)
                {
                    String[] noteStringParts = file.ReadLine().Split(':');
                    String instrName = noteStringParts[0];
                    int semitoneShift = int.Parse(noteStringParts[1]);

                    Instrument.Note note = Instrument.GetByName(instrName).AtPitch(semitoneShift);
                    loadedSong.AddNote(note, i);
                }
            }
            loadedSong.Tempo = int.Parse(file.ReadLine());
            return loadedSong;
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
