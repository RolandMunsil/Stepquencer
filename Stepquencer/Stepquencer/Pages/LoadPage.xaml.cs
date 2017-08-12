using Plugin.Share;
using Plugin.Share.Abstractions;
using System;
using System.IO;
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
                        AskBeforeDeletion(elem);   // Delete song file, refresh page
                    };
                    songUI.ShareClicked += delegate (LoadUIElement elem) 
                    {
                        ShareSongFile(elem.filePath);                   // Prompt user to share this song
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
        async void OnSongTap(LoadUIElement uiElement)
        {
            if (mainpage.loadedSongChanged)
            {               
                bool load = await DisplayAlert("Load Warning", "You will lose unsaved data if you load this song. Load anyway?", "Load without saving", "Cancel");

                if (!load) // Save first
                {
                    return;
                }
            }

            mainpage.clearedSong = null;
            mainpage.SetSong(FileUtilities.LoadSongFromFile(uiElement.filePath));
            mainpage.lastLoadedSongName = FileUtilities.SongNameFromFilePath(uiElement.filePath);
            //Does not let users undo clear after loading a song
            mainpage.clearedSong = null;
            ReturnToMainPage();
        }

        async void AskBeforeDeletion(LoadUIElement elem)
        {
            string songName = FileUtilities.SongNameFromFilePath(elem.filePath);
            bool answer = await DisplayAlert("Are you sure you want to delete?", "If you click yes, '" + songName + "' will be lost forever.", "Yes", "No");

            if (answer)
            {
                FileUtilities.DeleteSongFile(songName);     // Delete the file
                masterLayout.Children.Remove(elem);         // Refresh the page
			}
        }

        /// <summary>
        /// Called when share button on a song is tapped, prompts user to share
        /// </summary>
        /// <param name="songName">Song name.</param>
        private void ShareSongFile(String songFilePath)
        {
			if (!CrossShare.IsSupported)
                //TODO: Pop up a message instead
                throw new Exception();

			CrossShare.Current.Share(new ShareMessage
			{
				Title = "Check out my song!",
				Text = "I made a sweet song in Stepquencer!",
                Url = FileUtilities.GetShareableSongURL(FileUtilities.LoadSongFromFile(songFilePath))
			});
        }
    }
}
