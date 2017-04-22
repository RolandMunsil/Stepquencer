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

            // Load in all of the songUIElements
            LoadSongUIElements();
        }



        /// <summary>
        /// Method to load in/refresh UI based on which songs are saved
        /// </summary>
        public void LoadSongUIElements()
        {
            String[] songNames = Directory.GetFiles(FileUtilities.PathToSongDirectory);           // Get all the remaining files

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
            mainpage.SetSong(FileUtilities.LoadSongFromFile(uiElement.filePath));
            //Don't let users undo clear after loading a song
            mainpage.clearedSong = null;
            ReturnToMainPage();
        }

        /// <summary>
        /// Given the name of a song, deletes its corresponding file
        /// </summary>
        /// <param name="songName">Song name.</param>
        private static void DeleteSongFile(String songName)
        {
            File.Delete(FileUtilities.PathToSongFile(songName));
        }
    }
}
