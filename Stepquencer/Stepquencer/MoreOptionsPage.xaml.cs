using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace Stepquencer
{
    public partial class MoreOptionsPage : ContentPage
    {
        const double minBPM = 100;          // Minimum BPM user can change to
        const double maxBPM = 480;          // Maximum BPM user can go to


        private StackLayout masterLayout;                   // Overall layout (stacks tempo stuff on top of grid holding the buttons)
        private Grid tempoGrid;                             // Layout to hold tempo label and slider
        private Grid buttonGrid;                            // Grid to hold save, load, clear, and other button
        private Label tempoLabel, bpmLabel;                 // Labels to show the current BPM
        private Slider tempoSlider;                         // Slider that user can interact with to change BPM
        private Button saveButton, loadButton;              // Takes user to saving and loading pages respectively
        private Button clearAllButton, undoClearButton;     // Buttons to clear and undo clear
        private Button changeInstrumentsButton;             // Takes user to changeInstruments page
        private Style buttonStyle;                          // Style for the buttons on this page
        private MainPage mainpage;                          // The MainPage this screen came from
        private Song song;                                  // The user's current Song


        public MoreOptionsPage(MainPage passedpage, Song passedSong)
        {

            this.mainpage = passedpage;         // Need to pass in main page and song in order to change them on this page
            this.song = passedSong;             //

            NavigationPage.SetHasNavigationBar(this, true);     // Make sure navigation bar (with back button) shows up
            this.Title = "More Options";                        // Set title of page
            this.BackgroundColor = Color.FromHex("#2C2C2C");    // Set the background color of the page


            // Initialize style for buttons on this page

            buttonStyle = new Style(typeof(Button))
            {
                Setters = 
                {
                    new Setter { Property = Button.TextColorProperty, Value = Color.White },        // Buttons will have white text,
                    new Setter { Property = Button.BackgroundColorProperty, Value = Color.Black },  // a black background,
                    new Setter { Property = Button.FontSizeProperty, Value = 20 }                   // and a font size of 20
                }
            };


            // Initialize tempoGrid to hold tempo and slider

            tempoGrid = new Grid { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            tempoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            tempoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Column for tempo label should take up 1/6 of horizontal space
            tempoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Column for BPM label should take up 1/6 of horizontal space
            tempoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) }); // Column for slider should take up 2/3 of horizontal space


            // Initialize tempo label

            tempoLabel = new Label
            {
                Text = "Tempo: ",
                TextColor = Color.White,
                FontSize = 24,
                HorizontalTextAlignment = TextAlignment.Center,     //*
                VerticalTextAlignment = TextAlignment.Center,       //* Spacing/Alignment options
                HorizontalOptions = LayoutOptions.EndAndExpand      //* 
            };


            // Initialize the BPM label

            bpmLabel = new Label
            {
                Text = mainpage.currentTempo + "  " + "BPM",
                TextColor = Color.White,
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.End,     //*
                VerticalTextAlignment = TextAlignment.Center,       //* Spacing/Alignment options
                HorizontalOptions = LayoutOptions.CenterAndExpand    //*
            };


            // Initialize tempo slider

            tempoSlider = new Slider(minBPM, maxBPM, mainpage.currentTempo);    //*
            tempoSlider.HorizontalOptions = LayoutOptions.FillAndExpand;        //* Spacing/Alignment options
            tempoSlider.VerticalOptions = LayoutOptions.FillAndExpand;          //*


            // Add tempo label, BPM label and slider to tempoGrid

            tempoGrid.Children.Add(tempoLabel, 0, 0);
            tempoGrid.Children.Add(bpmLabel, 1, 0);
            tempoGrid.Children.Add(tempoSlider, 2, 0);


            // Initialize buttonGrid to hold buttons (2 rows and 2 columns)

            buttonGrid = new Grid { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand};

            buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            // Initialize buttons and add them to buttonGrid

            saveButton = new Button { Text = "SAVE", Style = buttonStyle };
            loadButton = new Button { Text = "LOAD", Style = buttonStyle };
            clearAllButton = new Button { Text = "CLEAR ALL", Style = buttonStyle, BackgroundColor = Color.Red };
            undoClearButton = new Button { Text = "UNDO CLEAR", Style = buttonStyle };
            changeInstrumentsButton = new Button { Text = "CHANGE INSTRUMENTS", Style = buttonStyle, BackgroundColor = Color.Blue};

            buttonGrid.Children.Add(saveButton, 0, 0);
            buttonGrid.Children.Add(loadButton, 0, 1);
            buttonGrid.Children.Add(clearAllButton, 1, 0);
            buttonGrid.Children.Add(undoClearButton, 2, 0);
            buttonGrid.Children.Add(changeInstrumentsButton, 1, 1);

            Grid.SetColumnSpan(changeInstrumentsButton, 2);


            // Add grids to masterLayout

            masterLayout = new StackLayout();
            masterLayout.Children.Add(tempoGrid);           
            masterLayout.Children.Add(buttonGrid);


            // Add event listeners

            tempoSlider.ValueChanged += OnSliderChanged;                     // Event listener for slider
            saveButton.Clicked += OnSaveButtonClicked;                       // Event listener for save button
            loadButton.Clicked += OnLoadButtonClicked;                       // Event listener for load button
            clearAllButton.Clicked += OnClearAllClicked;                     // Event listener for clear all button
            undoClearButton.Clicked += OnUndoClearClicked;                   // Event listener for undo clear button
            changeInstrumentsButton.Clicked += OnChangeInstrumentsClicked;   // Event listener for change instruments button


            Content = masterLayout;             // Put masterLayout on page

        }


        /// <summary>
        /// Changes the BPM visualization when slider changes
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnSliderChanged(object sender, ValueChangedEventArgs e)
        {

            int newTempo = (int)e.NewValue;          // Cast new BPM value to an int
            bpmLabel.Text = newTempo + "  " + "BPM"; // Change the label to reflect the new BPM
            mainpage.currentTempo = newTempo;        // Update the value stored by the mainpage

        }


        /// <summary>
        /// Event listener for save button
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SavePage(mainpage, song));   // Send to SavePage
        }


        /// <summary>
        /// Event listener for the load button
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnLoadButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoadPage(mainpage));   // Send to LoadPage
        }


        /// <summary>
        /// Clears the stepgrid and audio data when ClearAllButton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClearAllClicked(object sender, EventArgs e)
        {
            mainpage.ClearStepGrid();
            song.ClearAllBeats();
        }


        /// <summary>
        /// Event listener for undo clear button
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnUndoClearClicked(object sender, EventArgs e)
        {
            // TODO: Implement this
        }



        async void OnChangeInstrumentsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChangeInstrumentsPage());
        }


        public static String PathToSongFile(String songName)
        {
            String documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            String savePath = Path.Combine(documentsPath, "stepsongs/");
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            return Path.Combine(savePath, $"{songName}.txt");
        }
    }
}
