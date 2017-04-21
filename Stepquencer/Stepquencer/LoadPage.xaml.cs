using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace Stepquencer
{
    public partial class LoadPage : ContentPage
    {
        private MainPage mainpage;              // Reference to mainpage so it can be changed

        private StackLayout masterLayout;       // Layout that holds all UI elements
        private ScrollView scroller;            // Scrollview to accomodate more songs than the screen can handle
        private SearchBar searchbar;
        private Label resultsLabel;


        public LoadPage(MainPage mainpage) 
        {
            this.mainpage = mainpage;
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

            //Initialize resultsLabel
            resultsLabel = new Label
            {
            Text = "Result will appear here.",
            VerticalOptions = LayoutOptions.FillAndExpand,
            FontSize = 25
            };

            //Initialize searchbar
            searchbar = new SearchBar
            {
                Placeholder = "Enter search term",
                SearchCommand = new Command(() => { resultsLabel.Text = "Result: " + searchbar.Text + " is what you asked for."; })

            };

            // Check to make sure that the stepsongs folder exists, and create it if it doesn't
            if (!Directory.Exists(SongFileUtilities.PathToSongDirectory))           
            {
                Directory.CreateDirectory(SongFileUtilities.PathToSongDirectory);
            }

            // Load in all of the songUIElements
            LoadSongUIElements();
        }



        /// <summary>
        /// Method to load in/refresh UI based on which songs are saved
        /// </summary>
        public void LoadSongUIElements()
        {
            String[] songNames = Directory.GetFiles(SongFileUtilities.PathToSongDirectory);           // Get all the remaining files

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
                    SongUIElement songUI = new SongUIElement(name);     // Make a new instance of the UI element

                    songUI.Tap += OnSongTap;
                    songUI.DeleteClicked += delegate(SongUIElement elem)          
                    {
                        File.Delete(elem.filePath);             // Delete song file
                        masterLayout.Children.Remove(elem);     // Refresh the page
                    };

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
        void OnSongTap(SongUIElement uiElement)
        {
            Instrument[] songInstruments;
            mainpage.SetSong(LoadSongFromFile(uiElement.filePath, out songInstruments));
            mainpage.SetSidebarInstruments(songInstruments);
            ReturnToMainPage();
        }

        /// <summary>
        /// Loads a song given its name
        /// </summary>
        /// <returns>The song from file.</returns>
        /// <param name="songName">Song name.</param>
        public static Song LoadSongFromFile(String path, out Instrument[] songInstruments)
        {
            String filePath = path;
            Song loadedSong;

            using (StreamReader file = File.OpenText(filePath))
            {
                String[] firstLineParts = file.ReadLine().Split('|');
                int totalBeats = int.Parse(firstLineParts[0].Split(' ')[0]);
                loadedSong = new Song(totalBeats);

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
            }

            return loadedSong;
        }


        /// <summary>
        /// Given the name of a song, deletes its corresponding file
        /// </summary>
        /// <param name="songName">Song name.</param>
        private static void DeleteSongFile(String songName)
        {
            File.Delete(SongFileUtilities.PathToSongFile(songName));
        }
    }
}
