﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;


namespace Stepquencer
{
    /// <summary>
    /// Main page of the application. A place where users can make music, play and pause it, and go into another menu for more actions
    /// </summary>

    public partial class MainPage : ContentPage
    {
        const int NumRows = 25;                         // Number of rows of MiniGrids that users can tap and add sounds to
        const int RowWithSemitoneShiftOfZero = 12;
        const int NumColumns = 16;                      // Number of columns of Minigrids
        public const int NumInstruments = 4;            // Number of instruments on the sidebar
        const double brightnessIncrease = 0.25;		    // Amount to increase the red, green, and blue values of each button when it's highlighted

        ScrollView scroller;                            // ScrollView that will be used to scroll through stepgrid
        RelativeLayout verticalBarArea;                 // Area on the left sie of the screen for the vertical scroll bar
        RelativeLayout horizontalBarArea;               // Area on the bottom of the screen for the horizontal scroll bar
        BoxView verticalScrollBar;                      // Scroll bar to show position on vertical scroll
        BoxView horizontalScrollBar;                    // Scroll bar to show position on horizontal scroll

        SongPlayer player;                              // Plays the notes loaded into the current Song object
        public Song song;                               // Array of HashSets of Songplayer notes that holds the current song
        public Song clearedSong;                        // Song object that is cleared when user presses "CLEAR ALL" 

        public InstrumentButton[] instrumentButtons;    // An array of the instrument buttons on the sidebar
        public InstrumentButton selectedInstrButton;    // Currently selected sidebar button

        Grid mastergrid;                                // Grid for whole screen
        Grid stepgrid;                                  // Grid to hold MiniGrids
        Grid sidebar;                                   // Grid for sidebar

        int miniGridWidth = 57;                         // width of each minigrid
        int miniGridHeight = 54;                        // height of each minigrid
        const int stepGridSpacing = 4;                  // spacing between each minigrid on the stepgrid

        double scrollerActualWidth;
        double scrollerActualHeight;
        double startScale;                              // Used for pinch gestures
        double currentScale = 1;                        //
        double leftLimit;
        double rightLimit;
        double downLimit;
        double upLimit;

        //We need to use multiple highlights because on android they can't be more than a certain height
        int highlight2StartRow = 13;
        BoxView[] highlights;                           // Transparent View objects that takes up a whole column, moves to indicate beat
        Button playStopButton;                          // Button to play and stop the music.
        string playImageName;                               // Name of play button image file (depends on whether or not device is tablet)
        string stopImageName;                               // Name of stop button image file (depends on whether or not device is tablet)

		public readonly bool firstTime;                 // Indicates whether this is the first time user is opening app
        public bool loadedSongChanged = false;   // Indicates whether the user has changed a song since they loaded a new one 
        public string lastLoadedSongName = null;

        public MainPage(String songStringFromUrl = null)
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("#000000");          //* Set page style 
            NavigationPage.SetHasNavigationBar(this, false);     //*
            DependencyService.Get<IStatusBar>().HideStatusBar(); // Make sure status bar is hidden

            MakeStepGrid();

            //PinchGestureRecognizer pinchRecognizer = new PinchGestureRecognizer();  //
            //pinchRecognizer.PinchUpdated += HandlePinch;                            // Handle when user pinches
            //stepgrid.GestureRecognizers.Add(pinchRecognizer);                       //

            // Initialize the SongPlayer
            player = new SongPlayer();

            //Load default tempo and instruments, and if this is the first time the user has launched the app, show the startup song
            Song startSong = FileUtilities.LoadSongFromStream(FileUtilities.LoadEmbeddedResource("firsttimesong.txt"));

            if (!Directory.Exists(FileUtilities.PathToSongDirectory))
            {
                Directory.CreateDirectory(FileUtilities.PathToSongDirectory);
                this.song = startSong;
                lastLoadedSongName = "Initial beat";
                UpdateStepGridToMatchSong();
                FileUtilities.SaveSongToFile(startSong, "Initial beat");
                firstTime = true;
            }
            else
            {
                song = new Song(NumColumns, startSong.Instruments, startSong.Tempo);
                firstTime = false;
            }

            if(songStringFromUrl != null)
            {
                firstTime = false;

                song = FileUtilities.GetSongFromSongString(songStringFromUrl);
                UpdateStepGridToMatchSong();
            }

            playImageName = App.isTablet ? "play_Tab.png" : "play.png";
            stopImageName = App.isTablet ? "stop_Tab.png" : "stop.png";
            MakeSideBar();

            // Initialize the highlight box
            highlights = new BoxView[2];
            for (int i = 0; i < 2; i++)
            {
                highlights[i] = new BoxView() { Color = Color.White, Opacity = brightnessIncrease, InputTransparent = true };
            }
            player.BeatStarted += HighlightColumns;         // Add an event listener to keep highlight in time with beat


            //Set up a master grid with 3 columns and 2 rows to eventually place stepgrid, sidebar, and scrollbars in.
            mastergrid = new Grid { ColumnSpacing = 2, RowSpacing = 2 };
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14, GridUnitType.Absolute) });  //spot for up-down scrollbar
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });  // spot for stepgrid
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });  // spot for sidebar
            mastergrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });  // horizontal spot for everything, but side scrollbar
            mastergrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(14, GridUnitType.Absolute) });  // spot for side scrollbar


            // Make vertical scrollbar, size doesn't matter because constraints are used to size it later
            verticalScrollBar = new BoxView
            {
                BackgroundColor = Color.FromHex("D3D3D3"),
                WidthRequest = 1,
                HeightRequest = 1,
            };


            // Make vertical scrollbar
            horizontalScrollBar = new BoxView
            {
                BackgroundColor = Color.FromHex("D3D3D3"),
                WidthRequest = 1,
                HeightRequest = 1,
            };


            // This calculates the pixel values for the width and height of the whole scroller (including offscreen)
            scrollerActualHeight = (miniGridHeight + stepGridSpacing) * NumRows - stepGridSpacing;
            scrollerActualWidth = (miniGridWidth + stepGridSpacing) * NumColumns - stepGridSpacing;


            // Initialize scrollview and put stepgrid inside it
            scroller = new ScrollView
            {
                Orientation = ScrollOrientation.Both  //Both vertical and horizontal orientation
            };

            scroller.Content = stepgrid;

            mastergrid.Children.Add(scroller, 1, 0);

            // Make the Relative layouts that will hold the sidebars and place them in their proper positions in mastergrid
            verticalBarArea = new RelativeLayout();
            mastergrid.Children.Add(verticalBarArea, 0, 0);

            horizontalBarArea = new RelativeLayout();
            mastergrid.Children.Add(horizontalBarArea, 1, 1);

            DrawScrollBar(horizontalScrollBar);
            DrawScrollBar(verticalScrollBar);


            //scroller.Scrolled += UpdateScrollBars;              // Scrolled event that calls method to update scrollbars
            scroller.Scrolled += BoundedScroll;

            mastergrid.Children.Add(sidebar, 2, 0);             // Add sidebar to final column of mastergrid
            Grid.SetRowSpan(sidebar, 2);                        //make sidebar take up both rows in rightmost column


            Content = mastergrid;
        }


        private void BoundedScroll(Object o, ScrolledEventArgs e)
        {
            UpdateScrollBars();

            //leftLimit = ((stepgrid.Width / 2) - (stepgrid.Width * stepgrid.Scale / 2));      //
            //rightLimit = (stepgrid.Width / 2) + (stepgrid.Width * stepgrid.Scale / 2) - scroller.Width;     // Might not want to declare
            //downLimit = ((stepgrid.Height / 2) - (stepgrid.Height * stepgrid.Scale / 2));    // new ones every time?
            //upLimit = (stepgrid.Height / 2) + (stepgrid.Height * stepgrid.Scale / 2) - scroller.Height;    //

            //if (leftLimit > rightLimit)
            //{
            //    leftLimit = rightLimit - (leftLimit - rightLimit);  // If floating point conversion errors put leftlimit to the right of rightlimit, set it back
            //}
            //if (downLimit > upLimit)
            //{
            //    downLimit = upLimit - (downLimit - upLimit);
            //}

            //double newX = Clamp(scroller.ScrollX, leftLimit, rightLimit);
            //double newY = Clamp(scroller.ScrollY, downLimit, upLimit);

            //System.Diagnostics.Debug.WriteLine("\nScrollX: " + scroller.ScrollX + " ScrollY: " + scroller.ScrollY);
            //System.Diagnostics.Debug.WriteLine("leftLimit: " + leftLimit + " rightLimit: " + rightLimit);
            //System.Diagnostics.Debug.WriteLine("downLimit: " + downLimit + " uplimit: " + upLimit);
            //System.Diagnostics.Debug.WriteLine("width: " + stepgrid.Width + " height: " + stepgrid.Height);
            //System.Diagnostics.Debug.WriteLine("currentScale: " + currentScale + "\n");

            //if (scroller.ScrollX < leftLimit || scroller.ScrollY < downLimit || scroller.ScrollX > rightLimit || scroller.ScrollY > upLimit)
            //{
            //    scroller.ScrollToAsync(newX, newY, false);   // might want to make false
            //}
        }

    
        /// <summary>
        /// This method redraws the scrollbars when the scroller.Scrolled event is raised
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void UpdateScrollBars()
        {
            if (scroller.ScrollX !=  0)   //when scroller.ScrollX == 0 there is some odd, glitchy, jumpy behavior with scrollbars.
            {
                DrawScrollBar(horizontalScrollBar);
            }

            if (scroller.ScrollY !=  0)
            {
                DrawScrollBar(verticalScrollBar);
            }

        }



        /// <summary>
        /// Determines the location to draw the scrollbars on the screen and draws them
        /// </summary>
        /// <param name="scrollBar"></param>
        /// <param name="isVertical"></param>
        private void DrawScrollBar(BoxView scrollBar)
        {
            //TODO: make sure it takes scale into account and adjusts size

            if (scrollBar == verticalScrollBar)
            {
                verticalBarArea.Children.Remove(scrollBar);
                verticalBarArea.Children.Add(scrollBar, Constraint.RelativeToParent((parent)=>
                {
                    return 0.1 * parent.Width + 1; // x location to place bar
                }), Constraint.RelativeToParent((parent) =>
                {
                    return (parent.Height - parent.Height * parent.Height / scrollerActualHeight) * scroller.ScrollY / (scrollerActualHeight - parent.Height); // y location to place bar, updated by pos in scroll
                }), Constraint.RelativeToParent((parent) => {
                    return parent.Width * 0.8;      //width of barrelative to the scroll bar area
                }), Constraint.RelativeToParent((parent) => {
                    return parent.Height * parent.Height / scrollerActualHeight; //height of bar

                }));
            }
            else 
            {
                horizontalBarArea.Children.Remove(scrollBar);
                horizontalBarArea.Children.Add(scrollBar, Constraint.RelativeToParent((parent) =>
                {
                    return (parent.Width - parent.Width * parent.Width / scrollerActualWidth) * scroller.ScrollX / (scrollerActualWidth - parent.Width); // x location to place bar, updated by pos in scroll
                }), Constraint.RelativeToParent((parent) =>
                {
                    return (0.1 * parent.Height - 1); // y location to place bar
                }), Constraint.RelativeToParent((parent) => {
                    return parent.Width * parent.Width / scrollerActualWidth; //width of bar
                }), Constraint.RelativeToParent((parent) => {
                    return parent.Height * 0.8; //height of bar relative to height
                }));
            }
        }



        /// <summary>
        /// Replaces the instruments in sidebar with new ones selected in ChangeInstrumentPage. 
        /// For instance, 
        /// </summary>
        /// <param name="instruments">Instruments.</param>
        //When a new instrument is selected for a sidebar button, all sections of the
        //minigrids that correspond to that particular sidebar button will be changed
        //to the new instrument sound. For instance, if 'Clap' is selected to be in 
        //'Hi-Hat''s place on the sidebar, all hi-hat sections of a song will be replaced
        //with claps.
        public void ReplaceInstruments(Instrument[] instruments)
        {           
            List<Instrument> oldInstruments = new List<Instrument>();           //
            List<Instrument> newInstruments = new List<Instrument>();           //
            for (int i = 0; i < instruments.Length; i++)                        //
            {                                                                   //
                Instrument oldInstr = song.Instruments[i];                      //
                Instrument newInstr = instruments[i];                           // Figures out which instruments change
                if (oldInstr != newInstr)                                       //
                {                                                               //
                    oldInstruments.Add(oldInstr);                               //
                    newInstruments.Add(newInstr);                               //
                }                                                               //
            }                                                                   //

            song.ReplaceInstruments(oldInstruments, newInstruments);         
            SetSong(song);
        }



        /// <summary>
        /// Method to make a new, empty stepGrid
        /// </summary>
        private void MakeStepGrid()
        {
            //Set up grid of note squares
            stepgrid = new Grid { ColumnSpacing = stepGridSpacing, RowSpacing = stepGridSpacing };

            //Initialize the number of rows and columns for the stepgrid
            for (int i = 0; i < NumRows; i++)
            {
                stepgrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(miniGridHeight, GridUnitType.Absolute) });
            }
            for (int i = 0; i < NumColumns; i++)
            {      
                stepgrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(miniGridWidth, GridUnitType.Absolute) });
            }
            
            //Add grids to the stepgrid, and give each 2 columns, two rows and a BoxView
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    int semitoneShift = RowWithSemitoneShiftOfZero - i;
                    MiniGrid miniGrid = new MiniGrid(semitoneShift);
                    miniGrid.Tap += OnMiniGridTapped;
                    stepgrid.Children.Add(miniGrid, j, i);

                    if(j % 8 == 0)
                    {
                        Label noteLabel = MakeTextlessLabel();
                        noteLabel.Text = Instrument.SemitoneShiftToString(semitoneShift);

                        stepgrid.Children.Add(noteLabel, j, i);
                    }

                    //Measure labels appear every 6 rows, 4 columns
                    if ((i % 6 == 0) && (j % 4 == 3)) //See if this works
                    {
                        Label measureLabel = MakeTextlessLabel();
                        measureLabel.Text = (j + 1).ToString();

                        stepgrid.Children.Add(measureLabel, j, i);
                    }  
                }
            }
        }

        private void MakeSideBar()
        {
            // Make the sidebar
            sidebar = new Grid { ColumnSpacing = 1, RowSpacing = 1 };

            sidebar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < NumInstruments + 2; i++)
            {
                sidebar.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }


            // Fill sidebar with buttons and put them in the instrumentButtons array
            instrumentButtons = new InstrumentButton[song.Instruments.Length];
            for (int i = 0; i < song.Instruments.Length; i++)
            {
                InstrumentButton button = new InstrumentButton(song.Instruments[i]);   // Make a new button

                if (i == 0)                         //* 
                {                                   //*
                    button.Selected = true;         //* Initialize first sidebar button to be highlighted
                    selectedInstrButton = button;   //*
                }                                   //*
                button.Clicked += OnSidebarClicked; // Add button to an event handler                                 


                sidebar.Children.Add(button, 0, i + 1);    // Add button to sidebar
                instrumentButtons[i] = button;           // and instrumentButtons  

            }


            // Add a button to get to the MoreOptionsPage
            Button moreOptionsButton = new Button
            {
                BackgroundColor = Color.Black,
                Font = Font.SystemFontOfSize(40),
                Image = App.isTablet ? "settings_Tab.png" : "settings.png",
                TextColor = Color.White
            };
            sidebar.Children.Add(moreOptionsButton, 0, 0);
            moreOptionsButton.Clicked += OnMoreOptionsClicked;


            // Add a button to play/stop the SongPlayer
            playStopButton = new Button
            {
                BackgroundColor = Color.Black,
                Font = Font.SystemFontOfSize(40),
                TextColor = Color.White,
                BorderRadius = 0,
            };
            playStopButton.Image = playImageName;              //* Initialize playStop button as
            sidebar.Children.Add(playStopButton, 0, 5);     //* stopped, add it to the sidebar,
            playStopButton.Clicked += OnPlayStopClicked;    //* and add an event listener
        }

        /// <summary>
        /// Makes a textless label that can be used as either a notelabel or measurelabel for the stepgrid.
        /// </summary>
        /// <returns></returns>
        private Label MakeTextlessLabel()
        {
            Label textlessLabel = new Label
            {
                //VerticalTextAlignment = TextAlignment.Center,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = new Color(0.18),
                Margin = new Thickness(4, 3, 0, 0),
                InputTransparent = true
            };
            return textlessLabel;
        }



        /// <summary>
        /// Clears all colors/sounds from mastergrid. 
        /// </summary>
        public void ClearStepGridAndSong()
        {
            foreach (MiniGrid miniGrid in stepgrid.Children.OfType<MiniGrid>())
            {
                miniGrid.SetColors(new List<Color>());
            }
            clearedSong = song;
            loadedSongChanged = false;
            lastLoadedSongName = null;
            song = new Song(NumColumns, song.Instruments, song.Tempo);
        }

        public void UndoClear()
        {
            SetSong(clearedSong);
            clearedSong = null;
        }



        /// <summary>
        /// Sets the current song (and grid represntation) to be the given song.
        /// </summary>
        public void SetSong(Song newSong)
        {
            this.song = newSong;
            loadedSongChanged = false;

            UpdateStepGridToMatchSong();

            for (int i = 0; i < instrumentButtons.Length; i++)
            {
                instrumentButtons[i].Instrument = song.Instruments[i];
            }
        }

        private void UpdateStepGridToMatchSong()
        {
            Dictionary<int, List<Color>>[] colorsAtShiftAtBeat = new Dictionary<int, List<Color>>[this.song.BeatCount];
            for (int i = 0; i < this.song.BeatCount; i++)
            {
                Instrument.Note[] notes = this.song.NotesAtBeat(i);
                colorsAtShiftAtBeat[i] = notes.GroupBy(n => n.semitoneShift)
                                              .ToDictionary(
                                                     g => g.Key,
                                                     g => g.Select(n => n.instrument.color).ToList()
                                              );
            }

            foreach (MiniGrid miniGrid in stepgrid.Children.OfType<MiniGrid>())
            {
                int beat = Grid.GetColumn(miniGrid);

                List<Color> colorsOnThisButton;
                if (colorsAtShiftAtBeat[beat].TryGetValue(miniGrid.semitoneShift, out colorsOnThisButton))
                {
                    miniGrid.SetColors(colorsOnThisButton);
                }
                else
                {
                    miniGrid.SetColors(new List<Color>());
                }
            }
        }



        /// <summary>
        /// Event handler for individual miniGrid (holding 1-4 sounds) in mastergrid
        /// </summary>
        void OnMiniGridTapped(MiniGrid miniGrid)
        {
            Instrument selectedInstrument = selectedInstrButton.Instrument;

            // Changes UI represenation and returns new set of colors on this grid
            bool added = miniGrid.ToggleColor(selectedInstrument.color);

            // If sidebar color isn't part of button's new set of colors, remove it
            Instrument.Note toggledNote = selectedInstrument.AtPitch(miniGrid.semitoneShift);

            //If sidebar button color = clicked button color
            if (added)
            {
                song.AddNote(toggledNote, Grid.GetColumn(miniGrid));        // Add the note

                if (!player.IsPlaying)
                {
                    SingleNotePlayer.PlayNote(selectedInstrument.AtPitch(miniGrid.semitoneShift));   // Play note so long as not already playing a song
                }
            }
            else
            {
                song.RemoveNote(toggledNote, Grid.GetColumn(miniGrid));
            }

            //Undo clear stops woking when user adds stuff to grid so they don't accidentally undo clear
            clearedSong = null;
            loadedSongChanged = true;  // If song is empty, no need to worry about changes, otherwise 
        }



        /// <summary>
        /// Event handler for the Play/Stop button
        /// </summary>
        void OnPlayStopClicked(object sender, EventArgs e)
        {
            if (player.IsPlaying)
            {
                StopPlayingSong();
            }
            else
            {
                StartPlayingSong();
            }
        }



        /// <summary>
        /// Starts playing the song
        /// </summary>
        private void StartPlayingSong()
        {
            stepgrid.Children.Add(highlights[0], 0, 0);
            Grid.SetRowSpan(highlights[0], highlight2StartRow);
            stepgrid.Children.Add(highlights[1], 0, highlight2StartRow);
            Grid.SetRowSpan(highlights[1], NumRows - highlight2StartRow);
            player.BeginPlaying(song);
            //We call this after BeginPlaying because we know no more notes will try to play.
            //If we did it before, the user could theoretically start a note playing in between StopPlayingNote and BeginPlaying and it
            //wouldn't be stopped
            SingleNotePlayer.StopPlayingNote();
            playStopButton.Image = stopImageName;
        }



        /// <summary>
        /// Stops playing the song
        /// </summary>
        public void StopPlayingSong()
        {
            if (player.IsPlaying)
            {
                player.StopPlaying();
                playStopButton.Image = playImageName;
                foreach (BoxView highlight in highlights)
                {
                    stepgrid.Children.Remove(highlight);
                }
            }
        }



        /// <summary>
        /// Event handler for the more options button. Sends user to more options page.
        /// </summary>
        async void OnMoreOptionsClicked(object sender, EventArgs e)
        {
            if (player.IsPlaying)
            {
                StopPlayingSong();
            }
            SingleNotePlayer.StopPlayingNote();

            await Navigation.PushAsync(new MoreOptionsPage(this));
        }



        /// <summary>
        /// Event handler for buttons in the sidebar
        /// </summary>
        void OnSidebarClicked(object sender, EventArgs e)
        {
            InstrumentButton button = (InstrumentButton)sender;

            if (!player.IsPlaying)  // So long as the music isn't currently playing, the sidebar buttons play their sound when clicked
            {
                SingleNotePlayer.PlayNote(button.Instrument.AtPitch(0));
            }
            if (button != selectedInstrButton)
            {
                SetSelectedSidebarButton(button);
            }
        }

        public void SetSelectedSidebarButton(InstrumentButton button)
        {
            //Remove border fom previously selected instrument
            selectedInstrButton.Selected = false;
            button.Selected = true;             //Change border highlight to yellow
            selectedInstrButton = button;       //Set this button to be the currently selected button
        }



        /// <summary>
        /// Highlights the current column (beat) and de-highlights the previous column so long as this isn't the first note played
        /// </summary>
        void HighlightColumns(int currentBeat, bool firstBeat)
        {
            Device.BeginInvokeOnMainThread(delegate ()
            {
                for (int i = 0; i < highlights.Length; i++)
                {
                    Grid.SetColumn(highlights[i], currentBeat);
                }
            });
        }



        /// <summary>
        /// Displays a popup with instructions for a first-time user
        /// </summary>
        public async void DisplayInstructions()
        {
            await DisplayAlert("Welcome to Stepquencer!", "Tap on a square to place a sound, and hit play to hear your masterpiece.", "Get Started");
        }


        /// <summary>
        /// Handles when user pinches stepgrid. Should scale it accordingly (to create zoom effect)
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void HandlePinch(object sender, PinchGestureUpdatedEventArgs e)
        {

            // TODO: figure what other types of bounds are needed
            if (e.Status == GestureStatus.Started) {
                // Store the current scale factor applied to the wrapped user interface element
                startScale = Content.Scale;
            }
            if (e.Status == GestureStatus.Running)
            {
                // Calculate the scale factor to be applied.
                currentScale += (e.Scale - 1) * startScale;
                currentScale = Math.Max(0.5, currentScale);
                currentScale = Math.Min(1, currentScale);

                System.Diagnostics.Debug.WriteLine("Pinch event scale: " + e.Scale);

                stepgrid.Scale = currentScale;  // Apply scale factor

                //scroller.ScrollToAsync(stepgrid, ScrollToPosition.Center, false);   // Scroll so stepgrid is in center
                scroller.ScrollToAsync(e.ScaleOrigin.X, e.ScaleOrigin.Y, false);
            }
        }

		// <summary>
		/// Displays a popup if user tries to load song from outside source, to prevent them from overwriting an unsaved song
		/// </summary>
        public async void LoadWarning(Song songToBeLoaded)
		{
            bool load = true;
            if (loadedSongChanged)
            {
                load = await DisplayAlert("Overwrite Warning", "You will lose your current song if you haven't saved first. Are you sure you want to load?", "Load without saving", "Don't load");
            }

            if (load)
            {
                SetSong(songToBeLoaded);
            }
        }

        /// <summary>
        /// Returns a version of the given value restricted between low and high
        /// </summary>
        /// <returns>The clamp.</returns>
        /// <param name="val">Value.</param>
        /// <param name="low">Low.</param>
        /// <param name="high">High.</param>
        private double Clamp(double val, double low, double high)
        {
            val = (val < low) ? low : ((val > high) ? high : val);
            return val;
        }
    }
}
