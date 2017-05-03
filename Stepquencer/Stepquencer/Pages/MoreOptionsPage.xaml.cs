using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace Stepquencer
{
    /// <summary>
    /// A page that allows user to change tempo and serves as a hub for saving, loading, clearing, undoing a clear, and switching instruments
    /// </summary>

    public partial class MoreOptionsPage : ContentPage
    {
        const double MIN_BPM = 100;          // Minimum BPM user can change to
        const double MAX_BPM = 480;          // Maximum BPM user can go to
        const double BPM_SLIDER_STEP = 10;

        private Label bpmLabel;                             // Label to show the current BPM
        private Slider tempoSlider;                         // Slider that user can interact with to change BPM
        private MainPage mainpage;                          // The MainPage this screen came from
        private Button undoClearButton;

        public MoreOptionsPage(MainPage passedpage)
        {
            this.mainpage = passedpage;         // Need to pass in main page and song in order to change them on this page

            NavigationPage.SetHasNavigationBar(this, true);     // Make sure navigation bar (with back button) shows up
            this.Title = "More Options";                        // Set title of page
            this.BackgroundColor = Color.FromHex("#2C2C2C");    // Set the background color of the page


            // Initialize style for buttons on this page
            Style buttonStyle = new Style(typeof(Button))
            {
                Setters =
                {
                    new Setter { Property = Button.TextColorProperty, Value = Color.White },        // Buttons will have white text,
                    new Setter { Property = Button.BackgroundColorProperty, Value = Color.Black },  // a black background,
                    new Setter { Property = Button.FontSizeProperty, Value = 20 }                   // and a font size of 20
                }
            };


            // Initialize grid to hold tempo and slider
            Grid tempoGrid = new Grid { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            tempoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            tempoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Column for tempo label should take up 1/6 of horizontal space
            tempoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Column for BPM label should take up 1/6 of horizontal space
            tempoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) }); // Column for slider should take up 2/3 of horizontal space


            // Initialize tempo label
            Label tempoLabel = new Label
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
                Text = mainpage.song.Tempo + " BPM",
                TextColor = Color.White,
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.End,        //*
                VerticalTextAlignment = TextAlignment.Center,       //* Spacing/Alignment options
                HorizontalOptions = LayoutOptions.CenterAndExpand   //*
            };


            // Initialize tempo slider
            tempoSlider = new Slider(MIN_BPM, MAX_BPM, mainpage.song.Tempo);  //*
            tempoSlider.HorizontalOptions = LayoutOptions.FillAndExpand;        //* Spacing/Alignment options
            tempoSlider.VerticalOptions = LayoutOptions.FillAndExpand;          //*
            tempoSlider.Margin = 7;                                             //*


            // Add tempo label, BPM label and slider to tempoGrid
            tempoGrid.Children.Add(tempoLabel, 0, 0);
            tempoGrid.Children.Add(bpmLabel, 1, 0);
            tempoGrid.Children.Add(tempoSlider, 2, 0);


            // Initialize buttonGrid to hold all buttons 
            Grid buttonGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                RowSpacing = 6,
                ColumnSpacing = 6
            };

            buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            // Initialize buttons and add them to buttonGrid
            Button saveButton = new Button { Text = "SAVE", Style = buttonStyle };                                       // Takes user to SavePage
            Button loadButton = new Button { Text = "LOAD", Style = buttonStyle };                                       // Takes user to LoadPage
            Button clearAllButton = new Button { Text = "CLEAR ALL", Style = buttonStyle, BackgroundColor = Color.Red }; // Clears notes and resets UI on main screen
            undoClearButton = new Button { Text = "UNDO CLEAR", Style = buttonStyle };                            // Undos a recent clear
            Button changeInstrumentsButton = new Button                                                                  // Takes user to ChangeInstrumentsPage
            {
                Style = buttonStyle,
                BackgroundColor = Color.Blue
            };

#if __IOS__
            changeInstrumentsButton.Image = "swapinstr.png";        // Accounts for smaller iPhone screens
#endif
#if __ANDROID__
            changeInstrumentsButton.Text = "SWAP INSTRUMENTS";
#endif

            if (mainpage.clearedSong == null)
            {
                undoClearButton.TextColor = Color.Gray;
            }

            buttonGrid.Children.Add(saveButton, 0, 0);
            buttonGrid.Children.Add(loadButton, 0, 1);
            buttonGrid.Children.Add(clearAllButton, 1, 0);
            buttonGrid.Children.Add(undoClearButton, 1, 1);
            buttonGrid.Children.Add(changeInstrumentsButton, 2, 0);

            Grid.SetRowSpan(changeInstrumentsButton, 2);


            // Add grids to masterLayout

            StackLayout masterLayout = new StackLayout();       // Overall layout (stacks tempo stuff on top of grid holding the buttons)
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
        private void OnSliderChanged(object sender, ValueChangedEventArgs e)
        {
            int newTempo = (int)(Math.Round(e.NewValue / BPM_SLIDER_STEP) * BPM_SLIDER_STEP);          // Cast new BPM value to an int
            tempoSlider.Value = newTempo;
            bpmLabel.Text = newTempo + " BPM"; // Change the label to reflect the new BPM
            mainpage.song.Tempo = newTempo;        // Update the value stored by the mainpage
        }

        /// <summary>
        /// Event listener for save button
        /// </summary
        async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SavePage(mainpage));   // Send to SavePage
        }

        /// <summary>
        /// Event listener for the load button
        /// </summary>
        async void OnLoadButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoadPage(mainpage));   // Send to LoadPage
        }

        /// <summary>
        /// Clears the stepgrid and audio data when ClearAllButton is clicked
        /// </summary>
        private void OnClearAllClicked(object sender, EventArgs e)
        {
            undoClearButton.TextColor = Color.White;
            mainpage.ClearStepGridAndSong();
        }

        /// <summary>
        /// Event listener for undo clear button
        /// </summary>
        private void OnUndoClearClicked(object sender, EventArgs e)
        {
            if (mainpage.clearedSong != null)
            {
                mainpage.UndoClear();
                undoClearButton.TextColor = Color.Gray;
            }
        }

        async void OnChangeInstrumentsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChangeInstrumentsPage(this.mainpage));
        }
    }
}