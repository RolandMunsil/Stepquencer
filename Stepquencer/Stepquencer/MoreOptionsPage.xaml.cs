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


        private StackLayout masterLayout;   // Overall layout (stacks tempo stuff on top of grid holding the buttons)
        private Grid tempoGrid;             // Layout to hold tempo label and slider
        private Grid buttonGrid;            // Grid to hold save, load, clear, and other button
        private Label tempoLabel, bpmLabel; // Labels to show the current BPM
        private Slider tempoSlider;         // Slider that user can interact with to change BPM
        private Button saveButton, loadButton, clearAllButton, undoClearButton;     // Buttons to save, load, clear, etc
        private Style buttonStyle;          // Style for the buttons on this page
        private MainPage mainpage;          // The MainPage this screen came from
        private Song song;                  // The user's current Song


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
            tempoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Column for BPM label should take up 1/6 of horizontal space
            tempoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) }); // Column for slider should take up 2/3 of horizontal space


            // Initialize tempo label

            tempoLabel = new Label
            {
                Text = "Tempo: ",
                TextColor = Color.White,
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.Center,     //*
                VerticalTextAlignment = TextAlignment.Center,       //* Spacing/Alignment options
                HorizontalOptions = LayoutOptions.EndAndExpand      //* 
            };


            // Initialize the BPM label

            bpmLabel = new Label
            {
                Text = mainpage.currentTempo + "\nBPM",
                TextColor = Color.White,
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.Center,     //*
                VerticalTextAlignment = TextAlignment.Center,       //* Spacing/Alignment options
                HorizontalOptions = LayoutOptions.StartAndExpand    //*
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
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            // Initialize buttons and add them to buttonGrid

            saveButton = new Button { Text = "SAVE", Style = buttonStyle };
            loadButton = new Button { Text = "LOAD", Style = buttonStyle };
            clearAllButton = new Button { Text = "CLEAR ALL", Style = buttonStyle };
            undoClearButton = new Button { Text = "UNDO CLEAR", Style = buttonStyle };

            buttonGrid.Children.Add(saveButton, 0, 0);
            buttonGrid.Children.Add(loadButton, 0, 1);
            buttonGrid.Children.Add(clearAllButton, 1, 0);
            buttonGrid.Children.Add(undoClearButton, 1, 1);


            // Add grids to masterLayout

            masterLayout = new StackLayout();
            masterLayout.Children.Add(tempoGrid);           
            masterLayout.Children.Add(buttonGrid);


            // Add event listeners

            tempoSlider.ValueChanged += OnSliderChanged;        // Event listener for slider
            saveButton.Clicked += OnSaveButtonClicked;          // Event listener for save button
            loadButton.Clicked += OnLoadButtonClicked;          // Event listener for load button
            clearAllButton.Clicked += OnClearAllClicked;        // Event listener for clear all button


            Content = masterLayout;             // Put masterLayout on page

        }


        /// <summary>
        /// Changes the BPM visualization when slider changes
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnSliderChanged(object sender, ValueChangedEventArgs e)
        {
            int newTempo = (int)e.NewValue;
            bpmLabel.Text = newTempo + "\nBPM";
            mainpage.currentTempo = newTempo;
        }


        /// <summary>
        /// Event listener for save button
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnSaveButtonClicked(object sender, EventArgs e)
        {
            String documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            String savePath = Path.Combine(documentsPath, "stepsongs/");
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            String filePath = Path.Combine(savePath, "TEST.txt");

            using (StreamWriter file = File.CreateText(filePath))
            {
                Song song = mainpage.song;
                file.WriteLine($"{song.BeatCount} total beats");
                for (int i = 0; i < song.BeatCount; i++)
                {
                    Instrument.Note[] notes = song.NotesAtBeat(i);
                    file.WriteLine($"Beat {i}|{notes.Length}");
                    foreach(Instrument.Note note in song.NotesAtBeat(i))
                    {
                        file.WriteLine($"{note.instrument.instrumentName}:{note.semitoneShift}");
                    }
                }
            }
        }


        /// <summary>
        /// Event listener for the load button
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnLoadButtonClicked(object sender, EventArgs e)
        {
            String documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            String savePath = Path.Combine(documentsPath, "stepsongs/");

            String filePath = Path.Combine(savePath, "TEST.txt");

            Song loadedSong;

            using (StreamReader file = File.OpenText(filePath))
            {
                int totalBeats = int.Parse(file.ReadLine().Split(' ')[0]);
                loadedSong = new Song(totalBeats);
                for (int i = 0; i < totalBeats; i++)
                {
                    String header = file.ReadLine();
                    if(!header.Contains($"Beat {i}"))
                        throw new Exception("Invalid file or bug in file loader");
                    int numNotes = int.Parse(header.Split('|')[1]);

                    for(int n = 0; n < numNotes; n++)
                    {
                        String[] noteStringParts = file.ReadLine().Split(':');
                        String instrName = noteStringParts[0];
                        int semitoneShift = int.Parse(noteStringParts[1]);

                        Instrument.Note note = Instrument.loadedInstruments[instrName].AtPitch(semitoneShift);
                        loadedSong.AddNote(note, i);
                    }
                }
            }

            mainpage.SetSong(loadedSong);
        }


        /// <summary>
        /// Clears the stepgrid and audio data when ClearAllButton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClearAllClicked(object sender, EventArgs e)
        {
            mainpage.MakeNewStepGrid();
            song.ClearAllBeats();
            mainpage.scroller.Content = mainpage.stepgrid;

            // Add the scroller (which contains stepgrid) and sidebar to mastergrid
            mainpage.mastergrid.Children.Add(mainpage.scroller, 0, 0); // Add scroller to first column of mastergrid
            mainpage.Content = mainpage.mastergrid;
        }
    }
}
