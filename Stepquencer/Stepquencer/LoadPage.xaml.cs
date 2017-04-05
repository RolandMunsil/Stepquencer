using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace Stepquencer
{
    public partial class LoadPage : ContentPage
    {
        private MainPage mainpage;
        private Song song;

        String documentsPath;
        String savePath;
        private String[] songNames;

        private StackLayout songsLayout;

        public LoadPage(MainPage mainpage, Song song)
        {
            this.mainpage = mainpage;
            this.song = song;
            this.documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);     //* Get path to saved songs on this device
            this.savePath = Path.Combine(documentsPath, "stepsongs/");                              //*


            // Get all of the song names and store them in an array of Strings

            songNames = Directory.GetFiles(savePath);

            foreach (String name in songNames)
            {
                System.Diagnostics.Debug.WriteLine(GetSongNameFromFilePath(name) + "\n");
            }


            // Temporary, to be moved to an OnButtonClicked method or something along those lines

            Song loadedSong = LoadSongFromFile("TEST");
            mainpage.SetSong(loadedSong);

            Button test = new Button { Text = "Load TEST" };
            test.Clicked += ReturnToMainPage;
            Content = test;
        }

        async void ReturnToMainPage(Object sender, EventArgs e)
        {
            //Navigation.InsertPageBefore(mainpage, this);
            await Navigation.PopToRootAsync();
        }


        /// <summary>
        /// Given a file path, returns the name of the actual song
        /// </summary>
        /// <returns>The song name from file path.</returns>
        /// <param name="path">Path.</param>
        private String GetSongNameFromFilePath(String path)
        {
            // NOTE: TrimStart seems to get overzealous and remove first word of song when using savePath, so this janky way is necessary
            String name = path.TrimStart(documentsPath.ToCharArray()).TrimStart('g', 's', '/');  // Remove all the nonsense at the beginning so it's just "name.txt"
            name = name.TrimEnd(".txt".ToCharArray());      // Remove ".txt" at end

            return name;
        }

        /// <summary>
        /// Loads a song given its name
        /// </summary>
        /// <returns>The song from file.</returns>
        /// <param name="songName">Song name.</param>
        private static Song LoadSongFromFile(String songName)
        {
            String filePath = MoreOptionsPage.PathToSongFile(songName);

            Song loadedSong;

            using (StreamReader file = File.OpenText(filePath))
            {
                int totalBeats = int.Parse(file.ReadLine().Split(' ')[0]);
                loadedSong = new Song(totalBeats);
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

                        Instrument.Note note = Instrument.loadedInstruments[instrName].AtPitch(semitoneShift);
                        loadedSong.AddNote(note, i);
                    }
                }
            }

            return loadedSong;
        }

        private static void DeleteSongFile(String songName)
        {
            File.Delete(MoreOptionsPage.PathToSongFile(songName));
        }
    }
}
