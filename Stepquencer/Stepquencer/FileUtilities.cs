using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

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

        /// <summary>
        /// Loads a resource that has been embedded in the cross-platform Stepquencer project
        /// </summary>
        public static Stream LoadEmbeddedResource(String resourceName)
        {
            return assembly.GetManifestResourceStream(resourcePrefix + resourceName);
        }

        /// <summary>
        /// The path to the directory where the user's songs are stored
        /// </summary>
        public static String PathToSongDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "stepsongs/");
            }
        }

        /// <summary>
        /// Given the name of a song, returns the full path to the song
        /// </summary>
        public static String PathToSongFile(String songName)
        {
            return Path.Combine(PathToSongDirectory, $"{songName}.txt");
        }

        /// <summary>
        /// Given a file path, returns just the name of the song
        /// </summary>
        public static String SongNameFromFilePath(String path)
        {
            int start = path.IndexOf("stepsongs") + "stepsongs".Length + 1;
            int end = path.LastIndexOf('.');
            return path.Substring(start, end - start);
        }

        /// <summary>
        /// Loads a song from the local filesystem
        /// </summary>
        /// <param name="path">The path to the song</param>
        public static Song LoadSongFromFile(String path)
        {
            using (StreamReader file = File.OpenText(path))
            {
                return LoadSongFromStream(file.BaseStream);
            }
        }

        /// <summary>
        /// Generates a Song object from a stream containing a text representation of a song
        /// </summary>
        public static Song LoadSongFromStream(Stream stream)
        {
            //Use StreamReader because the songs are in text foma
            StreamReader file = new StreamReader(stream);

            String versionString = file.ReadLine();
            if (versionString != "VERSION 1" && versionString != "VERSION 1.0.2")
            {
                throw new FileLoadException("Save file is in an invalid format");
            }

            //The first line of a song file contains the number of beats
            //and the song's instruments. It usually looks something like
            //16 beats|Snare|BassDrum|Piano|Clap

            //Get total beats
            String[] firstLineParts = file.ReadLine().Split('|');
            int totalBeats = int.Parse(firstLineParts[0].Split(' ')[0]);

            //Load instruments
            Instrument[] songInstruments = new Instrument[firstLineParts.Length - 1];
            for (int i = 1; i < firstLineParts.Length; i++)
            {
                songInstruments[i - 1] = Instrument.GetByName(firstLineParts[i]);
            }

            //Tempo is stored at the end of the file, so for now set the tempo to be -1
            Song loadedSong = new Song(totalBeats, songInstruments, -1);

            //Load notes
            for (int i = 0; i < totalBeats; i++)
            {
                //Each beat starts with a header of the form
                //Beat <beat number>|<number of notes>
                String header = file.ReadLine();
                int numNotes = int.Parse(header.Split('|')[1]);

                for (int n = 0; n < numNotes; n++)
                {
                    //Each note is stored like so:
                    //<instrument name>:<semitone shift>
                    String[] noteStringParts = file.ReadLine().Split(':');
                    String instrName = noteStringParts[0];
                    int semitoneShift = int.Parse(noteStringParts[1]);

                    Instrument.Note note = Instrument.GetByName(instrName).AtPitch(semitoneShift);
                    loadedSong.AddNote(note, i);
                }
            }
            //The last line of the file is the tempo
            loadedSong.Tempo = int.Parse(file.ReadLine());
            return loadedSong;
        }

        /// <summary>
        /// Saves a song to the user's local filesystem
        /// </summary>
        public static void SaveSongToFile(Song song, String songName)
        {
            String filePath = PathToSongFile(songName);

            using (StreamWriter file = File.CreateText(filePath))
            {
                WriteSongToStream(song, file);
            }
        }

        /// <summary>
        /// Delete the song file with the name <paramref name="songName"/>
        /// </summary>
        public static void DeleteSongFile(String songName)
		{
			File.Delete(PathToSongFile(songName));
		}

        /// <summary>
        /// Writes a text representation of <paramref name="song"/> and writes it to <paramref name="stream"/>
        /// </summary>
        public static void WriteSongToStream(Song song, StreamWriter stream)
        {
            stream.WriteLine("VERSION 1");

            //Format instrument names like so:
            //Instrument1|Instrument2|Instrument3|etc.
            String instrumentNames = String.Join("|", song.Instruments.Select(instr => instr.name));
            stream.WriteLine($"{song.BeatCount} total beats|{instrumentNames}");

            for (int i = 0; i < song.BeatCount; i++)
            {
                Instrument.Note[] notes = song.NotesAtBeat(i);
                stream.WriteLine($"Beat {i}|{notes.Length}");
                foreach (Instrument.Note note in song.NotesAtBeat(i))
                {
                    stream.WriteLine($"{note.instrument.name}:{note.semitoneShift}");
                }
            }
            stream.WriteLine(song.Tempo);
        }

        /// <summary>
        /// Gets a string representing the given Song that can be used in a sharing URL
        /// </summary>
        private static String GetSongString(Song song)
        {
            byte[] compressedData;
            using (MemoryStream compressStream = new MemoryStream())
            {
                using (DeflateStream compressor = new DeflateStream(compressStream, CompressionMode.Compress))
                {
                    using (StreamWriter writer = new StreamWriter(compressor))
                    {
                        WriteSongToStream(song, writer);
                    }
                    compressedData = compressStream.ToArray();
                }
            }
            //Equals signs are not URL-safe, so replace them with tildes instead
            return Convert.ToBase64String(compressedData).Replace('=', '~');
        }

        /// <summary>
        /// Creates a Song object from a song string
        /// </summary>
        public static Song GetSongFromSongString(String songString)
        {
            byte[] compressedData = Convert.FromBase64String(songString.Replace('~', '='));

            using (MemoryStream decompressStream = new MemoryStream(compressedData))
            {
                using (DeflateStream decompressor = new DeflateStream(decompressStream, CompressionMode.Decompress))
                {
                    return LoadSongFromStream(decompressor);
                }
            }
        }

        /// <summary>
        /// Given a Song, returns a url string that represents an easily shareable version of that song
        /// </summary>
        public static String GetShareableSongURL(Song song)
        {
            return $"https://rolandmunsil.github.io/Sharequencer?s={GetSongString(song)}";
        }
    }
}
