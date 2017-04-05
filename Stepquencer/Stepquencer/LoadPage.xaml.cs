using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace Stepquencer
{
    public partial class LoadPage : ContentPage
    {
        private MainPage mainpage;

        String documentsPath;
        String savePath;
        private String[] songNames;

        private StackLayout masterLayout;
        private ScrollView scroller;

        public LoadPage(MainPage mainpage)
        {
            this.mainpage = mainpage;
            this.documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);     //* Get path to saved songs on this device
            this.savePath = Path.Combine(documentsPath, "stepsongs/");                              //*


            // Initialize scrollview

            scroller = new ScrollView();


            // Initialize masterLayout

            masterLayout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                BackgroundColor = Color.FromHex("#2C2C2C")
            };

            // Get all of the song names and store them in an array of Strings

            songNames = Directory.GetFiles(savePath);

            foreach (String name in songNames)          //TODO: show something if there are no saved songs
            {
                SongUIElement songUI = new SongUIElement(name);     // Make a new instance of the UI element

                // Add TapGestureRecognizer
                TapGestureRecognizer tgr = new TapGestureRecognizer // Make a new TapGestureRecognizer to add to it
                {
                    CommandParameter = songUI.GetSong(),
                    Command = new Command(OnSongTap)
                };

                songUI.AddGestureRecognizers(tgr);                  // Add Tapgesture recognizer to diffenent parts of UI element
                masterLayout.Children.Add(songUI);          // Add UI element to the masterLayout

            }

            scroller.Content = masterLayout;

            Content = scroller;
        }

        /// <summary>
        /// Asynchronus function that returns user to main page
        /// </summary>
        async void ReturnToMainPage()
        {
            await Navigation.PopToRootAsync();
        }


        /// <summary>
        /// Event handler for a SongUIElement being tapped
        /// </summary>
        /// <param name="song">Song.</param>
        void OnSongTap(Object song)
        {
            Song loadedSong = (Song)song;
            mainpage.SetSong(loadedSong);
            ReturnToMainPage();
        }




        /// <summary>
        /// Loads a song given its name
        /// </summary>
        /// <returns>The song from file.</returns>
        /// <param name="songName">Song name.</param>
        public static Song LoadSongFromFile(String path)
        {
            String filePath = path;
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
