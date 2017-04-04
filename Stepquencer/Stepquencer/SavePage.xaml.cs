using System;
using System.Collections.Generic;
using System.IO;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class SavePage : ContentPage
    {
        private MainPage mainpage;                  // The MainPage this screen came from
        private Song song;                          // The user's current Song
        private StackLayout masterLayout;           // Main layout object
        private StackLayout buttonLayout;           // Layout to hold buttons
        private Label saveLabel;                    // Title above text input
        private Entry songTitleEntry;               // Input Box for name of user's song
        private Button saveButton, cancelButton;    // Buttons to save and cancel

        public SavePage(MainPage mainpage, Song passedSong)
        {
            this.mainpage = mainpage;                           //* Need to pass in main page and song in order to change them on this page
            this.song = passedSong;                             //* 
            this.BackgroundColor = Color.FromHex("#2C2C2C");    // Set the background color of the page
            NavigationPage.SetHasNavigationBar(this, false);    // Make sure navigation bar doesn't show up on this one


            // Initialize masterLayout

            masterLayout = new StackLayout { Spacing = 30};


            // Initialize saveLabel

            saveLabel = new Label
            {
                Text = "Save your song",
                TextColor = Color.White,
                BackgroundColor = Color.FromHex("#2C2C2C"),
                FontSize = 40,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };



            // Initialize songTitleEntry

            songTitleEntry = new Entry 
            { 
                Placeholder = "Enter your song's name",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };


            // Initialize buttonLayout

            buttonLayout = new StackLayout();
            buttonLayout.Orientation = StackOrientation.Horizontal;
            buttonLayout.VerticalOptions = LayoutOptions.CenterAndExpand;


            // Initialize save and cancel buttons

            saveButton = new Button 
            { 
                Text = "SAVE", 
                TextColor = Color.White, 
                BackgroundColor = Color.Black,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            cancelButton = new Button 
            { 
                Text = "CANCEL", 
                TextColor = Color.White, 
                BackgroundColor = Color.Black,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };


            // Initialize event handlers for buttons

            saveButton.Clicked += OnButtonClicked;
            cancelButton.Clicked += OnButtonClicked;


            // Add buttons to buttonLayout

            buttonLayout.Children.Add(cancelButton);
            buttonLayout.Children.Add(saveButton);


            // Add label, entry, buttonLayout to masterLayout

            masterLayout.Children.Add(saveLabel);
            masterLayout.Children.Add(songTitleEntry);
            masterLayout.Children.Add(buttonLayout);


            Content = masterLayout;
        }

        async void OnButtonClicked(Object sender, EventArgs e)
        {
            Button buttonPressed = (Button)sender;

            if (buttonPressed.Equals(this.cancelButton))
            {                                                                       // If the cancel button was pushed, send user
                await Navigation.PopToRootAsync();    // back to MoreOptionsPage.
            }
            else
            {
                SaveSongToFile(mainpage.song, songTitleEntry.Text);
                await Navigation.PopToRootAsync();
            }
        }


        private void SaveSongToFile(Song songToSave, String songName)
        {
            String filePath = MoreOptionsPage.PathToSongFile(songName);

            using (StreamWriter file = File.CreateText(filePath))
            {
                file.WriteLine($"{songToSave.BeatCount} total beats");
                for (int i = 0; i < songToSave.BeatCount; i++)
                {
                    Instrument.Note[] notes = songToSave.NotesAtBeat(i);
                    file.WriteLine($"Beat {i}|{notes.Length}");
                    foreach (Instrument.Note note in songToSave.NotesAtBeat(i))
                    {
                        file.WriteLine($"{note.instrument.instrumentName}:{note.semitoneShift}");
                    }
                }
            }
        }
    }
}
