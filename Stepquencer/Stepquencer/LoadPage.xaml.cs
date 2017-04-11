using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace Stepquencer
{
    public partial class LoadPage : ContentPage
    {
        private MainPage mainpage;              // Reference to mainpage so it can be changed
        String savePath;
        private String[] songNames;

        private StackLayout masterLayout;       // Layout that holds all UI elements
        private ScrollView scroller;            // Scrollview to accomodate more songs than the screen can handle

        public LoadPage(MainPage mainpage)
        {
            this.mainpage = mainpage;
            String documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);     //* Get path to saved songs on this device
            this.savePath = Path.Combine(documentsPath, "stepsongs/");                              //*

            this.BackgroundColor = Color.FromHex("#2C2C2C");


            // Initialize scrollview

            scroller = new ScrollView();


            // Initialize masterLayout

            masterLayout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                BackgroundColor = Color.FromHex("#2C2C2C"),
                Margin = 7
            };


            // Check to make sure that the stepsongs folder exists, and create it if it doesn't

            if (!Directory.Exists(savePath))           
            {
                Directory.CreateDirectory(savePath);
            }


            // Load in all of the songUIElements

            loadSongUIElements();
        }



        /// <summary>
        /// Method to load in/refresh UI based on which songs are saved
        /// </summary>
        public void loadSongUIElements()
        {

            masterLayout.Children.Clear();                 // Clear out all the songUI elements currently shown

            songNames = Directory.GetFiles(savePath);           // Get all the remaining files


            if (songNames.Length == 0)      // If there are no saved songs, 
            {
                Label noSongsLabel = new Label
                {
                    Text = "No saved songs",
                    FontSize = 40,
                    TextColor = Color.White,
                    HorizontalTextAlignment = TextAlignment.Center,     //*
                    VerticalTextAlignment = TextAlignment.Center,       //* Ensures that the label is in the very center of screen
                    HorizontalOptions = LayoutOptions.FillAndExpand,    //*
                    VerticalOptions = LayoutOptions.FillAndExpand       //*
                };

                masterLayout.Children.Add(noSongsLabel);
            }

            else
            {
                foreach (String name in songNames)
                {
                    SongUIElement songUI = new SongUIElement(name, this);     // Make a new instance of the UI element

                    // Add TapGestureRecognizer
                    TapGestureRecognizer tgr = new TapGestureRecognizer // Make a new TapGestureRecognizer to add to it
                    {
                        CommandParameter = songUI,
                        Command = new Command(OnSongTap)
                    };

                    songUI.AddGestureRecognizers(tgr);          // Add Tapgesture recognizer to diffenent parts of UI element
                    masterLayout.Children.Add(songUI);          // Add UI element to the masterLayout

                }
            }

            scroller.Content = masterLayout;            // Put updated masterLayout in scroller
            Content = scroller;                         // Refresh the page content with the new scroller
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
        /// <param name="songUI">Song user interface.</param>
        void OnSongTap(Object songUI)
        {
            SongUIElement uiElement = (SongUIElement)songUI;
            mainpage.SetSong(uiElement.GetSong());
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


        /// <summary>
        /// Given the name of a song, deletes its corresponding file
        /// </summary>
        /// <param name="songName">Song name.</param>
        private static void DeleteSongFile(String songName)
        {
            File.Delete(MoreOptionsPage.PathToSongFile(songName));
        }
    }
}
