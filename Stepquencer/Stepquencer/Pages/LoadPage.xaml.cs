using Plugin.Share;
using Plugin.Share.Abstractions;
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
    

        public LoadPage(MainPage mainpage) 
        {
            this.mainpage = mainpage;
            this.Title = "Songs";
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

            // Load in all of the songUIElements
            LoadSongUIElements();
        }



        /// <summary>
        /// Method to load in/refresh UI based on which songs are saved
        /// </summary>
        public void LoadSongUIElements()
        {
            // Get all the remaining files
            String[] songNames = Directory.GetFiles(FileUtilities.PathToSongDirectory); 

            if (songNames.Length == 0)                                  // If there are no saved songs
            {
                Label noSongsLabel = new Label
                {
                    Text = "No saved songs",
                    FontSize = 40,
                    TextColor = Color.White,
                    HorizontalTextAlignment = TextAlignment.Center,     //
                    VerticalTextAlignment = TextAlignment.Center,       // Ensures that the label is in the 
                                                                        // very center of screen
                    HorizontalOptions = LayoutOptions.FillAndExpand,    //
                    VerticalOptions = LayoutOptions.FillAndExpand       //
                };

                masterLayout.Children.Add(noSongsLabel);
            }

            else
            {
                foreach (String name in songNames)
                {
                    LoadUIElement songUI = new LoadUIElement(name);     // Make a new instance of the UI element

                    songUI.Tap += OnSongTap;
                    songUI.DeleteClicked += delegate(LoadUIElement elem)          
                    {
                        File.Delete(elem.filePath);                     // Delete song file
                        masterLayout.Children.Remove(elem);             // Refresh the page
                    };

                    masterLayout.Children.Add(songUI);                  // Add UI element to the masterLayout
                }
            }

            scroller.Content = masterLayout;                            // Put updated masterLayout in scroller
            Content = scroller;                                         // Refresh the page content with the new scroller
        }



        /// <summary>
        /// Asynchronous function that returns user to main page
        /// </summary>
        async void ReturnToMainPage()
        {
            await Navigation.PopToRootAsync();
        }



        /// <summary>
        /// Event handler for a SongUIElement being tapped
        /// </summary>
        /// <param name="uiElement">Song user interface.</param>
        void OnSongTap(LoadUIElement uiElement)
        {
            //Uncomment this code to make the song share when the user taps
            /*
            if (!CrossShare.IsSupported)
                throw new Exception();

            CrossShare.Current.Share(new ShareMessage
            {
                Title = "Check out my song!",
                Text = "I made a sweet song in Stepquencer!",
                Url = FileUtilities.GetShareableSongURL(FileUtilities.LoadSongFromFile(uiElement.filePath))
            });
            */

            if (mainpage.loadedSongChanged)
            {
                mainpage.clearedSong = null;
                LoadWarning(FileUtilities.LoadSongFromFile(uiElement.filePath));
            }
            else
            {
                mainpage.SetSong(FileUtilities.LoadSongFromFile(uiElement.filePath));

                //Does not let users undo clear after loading a song
                mainpage.clearedSong = null;
                ReturnToMainPage();
            }
        }



        /// <summary>
        /// Given the name of a song, deletes its corresponding file
        /// </summary>
        /// <param name="songName">Song name.</param>
        private static void DeleteSongFile(String songName)
        {
            File.Delete(FileUtilities.PathToSongFile(songName));
        }

		/// <summary>
		/// Displays a popup if user tries to load song from outside source, to prevent them from overwriting an unsaved song
		/// </summary>
        public async void LoadWarning(Song songToBeLoaded)
        {
            var answer = await DisplayAlert("Overwrite Warning", "You could lose your current song if you haven't saved first. Would you like to save now?", "Load without saving", "Save first");

            if (answer == false) // Save first
            {
                await Navigation.PushAsync(new SavePage(mainpage, songToBeLoaded));
            }
            else
            {
                mainpage.SetSong(songToBeLoaded);
                await Navigation.PopToRootAsync();
            }
        }
    }
}
