using System;
using System.Collections.Generic;
using System.IO;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class SavePage : ContentPage
    {
        private MainPage mainpage;                  // The MainPage this screen came from
        private Entry songTitleEntry;               // Input Box for name of user's song
        private Button saveButton, cancelButton;    // Buttons to let user save or go back

        public SavePage(MainPage mainpage)
        {
            this.mainpage = mainpage;                           // Need to pass in main page in order to change it on this page
            this.BackgroundColor = Color.FromHex("#2C2C2C");    // Set the background color of the page
            NavigationPage.SetHasNavigationBar(this, false);    // Make sure navigation bar doesn't show up on this one


            // Initialize scrollView

            ScrollView scroller = new ScrollView();


            // Initialize masterLayout

            StackLayout masterLayout = new StackLayout { Spacing = 30};


            // Initialize saveLabel

            Label saveLabel = new Label
            {
                Text = "Song Title",
                TextColor = Color.White,
                BackgroundColor = Color.FromHex("#2C2C2C"),
                FontSize = 40,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };



            // Initialize songTitleEntry

            songTitleEntry = new Entry 
            { 
                Placeholder = "Enter song title",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };


            // Initialize buttonLayout

            StackLayout buttonLayout = new StackLayout();
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

            scroller.Content = masterLayout;
            Content = scroller;
        }

        async void OnButtonClicked(Object sender, EventArgs e)
        {
            Button buttonPressed = (Button)sender;

            if (buttonPressed.Equals(cancelButton))
            {                                                                       // If the cancel button was pushed, send user
                await Navigation.PopToRootAsync();    // back to MoreOptionsPage.
            }
            else
            {
                SaveSongToFile(mainpage.song, songTitleEntry.Text);
                await Navigation.PopToRootAsync();
            }
        }


        //TODO: Add method
        //If song file name already exists, warn user


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
                        file.WriteLine($"{note.instrument.name}:{note.semitoneShift}");
                    }
                }
            }
        }
    }
}
