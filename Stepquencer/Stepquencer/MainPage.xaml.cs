using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using Xamarin.Forms;

namespace Stepquencer
{
    /// <summary>
    /// Main page of the application. A place where users can make music, play and pause it, and go into another menu for more actions
    /// </summary>

    public partial class MainPage : ContentPage
    {

        const int NumRows = 13;                     // Number of rows of MiniGrids that users can tap and add sounds to
        const int NumColumns = 16;                   // Number of columns of Minigrids
        public const int NumInstruments = 4;               // Number of instruments on the sidebar
        const double brightnessIncrease = 0.25;		// Amount to increase the red, green, and blue values of each button when it's highlighted

        public ScrollView scroller;                     // ScrollView that will be used to scroll through stepgrid
        RelativeLayout verticalBarArea;                 // Area on the left sie of the screen for the vertical scroll bar
        RelativeLayout horizontalBarArea;               // Area on the bottom of the screen for the horizontal scroll bar
        BoxView verticalScrollBar;                      // Scroll bar to show position on vertical scroll
        BoxView horizontalScrollBar;                    // Scroll bar to show position on horizontal scroll


        SongPlayer player;                              // Plays the notes loaded into the current Song object
        public Song song;                               // Array of HashSets of Songplayer notes that holds the current song
        public Song clearedSong;

        public InstrumentButton[] instrumentButtons;    // An array of the instrument buttons on the sidebar
        public InstrumentButton selectedInstrButton;           // Currently selected sidebar button

        public Grid mastergrid;                         // Grid for whole screen
        public Grid stepgrid;                           // Grid to hold MiniGrids
        Grid sidebar;                                   // Grid for sidebar

        BoxView highlight;                              // A transparent View object that takes up a whole column, moves to indicate beat
        Button playStopButton;                          // Button to play and stop the music.

        public MainPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("#000000");         //* Set page style 
            NavigationPage.SetHasNavigationBar(this, false);    //*

            // Initialize the SongPlayer
            player = new SongPlayer();

            // Make the stepgrid and fill it with boxes
            MakeStepGrid();

            //Load default tempo and instruments, and if this is the first time the user has launched the app, show the startup song
            Song startSong = FileUtilities.LoadSongFromStream(FileUtilities.LoadEmbeddedResource("firsttimesong.txt"));

            if (!Directory.Exists(FileUtilities.PathToSongDirectory))
            {
                Directory.CreateDirectory(FileUtilities.PathToSongDirectory);
                this.song = startSong;
                UpdateStepGridToMatchSong();
                FileUtilities.SaveSongToFile(startSong, "Initial beat");
            }
            else
            {
                song = new Song(NumColumns, startSong.Instruments, startSong.Tempo);
            }

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


                sidebar.Children.Add(button, 0, i+1);    // Add button to sidebar
                instrumentButtons[i] = button;           // and instrumentButtons  

            }

            // Add a button to get to the MoreOptionsPage
            Button moreOptionsButton = new Button
            {
                BackgroundColor = Color.Black,
                Font = Font.SystemFontOfSize(40),
                Image = "settings.png",
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
            playStopButton.Image = "play.png";              //* Initialize playStop button as
            sidebar.Children.Add(playStopButton, 0, 5);     //* stopped, add it to the sidebar,
            playStopButton.Clicked += OnPlayStopClicked;    //* and add an event listener


            // Initialize the highlight box
            highlight = new BoxView() { Color = Color.White, Opacity = brightnessIncrease };
            highlight.InputTransparent = true;
            player.BeatStarted += HighlightColumns;         // Add an event listener to keep highlight in time with beat


            // Initialize scrollview and put stepgrid inside it
            scroller = new ScrollView
            {
                Orientation = ScrollOrientation.Both  //Both vertical and horizontal orientation
            };

            scroller.Content = stepgrid;

            //Set up a master grid with 3 columns and 2 rows to eventually place stepgrid, sidebar, and scrollbars in.
            mastergrid = new Grid { ColumnSpacing = 2, RowSpacing = 2 };
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14, GridUnitType.Absolute) });  //spot for up-down scrollbar
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });  // spot for stepgrid
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });  // spot for sidebar
            mastergrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });  // horizontal spot for everything, but side scrollbar
            mastergrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(14, GridUnitType.Absolute) });  // spot for side scrollbar
            
            //make vertical scrollbar, size doesn't matter because constraints are used to size it later
            verticalScrollBar = new BoxView
            {
                BackgroundColor = Color.FromHex("D3D3D3"),
                WidthRequest = 1,
                HeightRequest = 1,
            };

            //make vertical scrollbar, size doesn't matter because constraints are used to size it later
            horizontalScrollBar = new BoxView
            {
                BackgroundColor = Color.FromHex("D3D3D3"),
                WidthRequest = 1,
                HeightRequest = 1,
            };

            verticalBarArea = new RelativeLayout();

            verticalBarArea.Children.Add(verticalScrollBar, Constraint.RelativeToParent((parent) => {
                return (parent.Width * 0.1);
            }), Constraint.RelativeToParent((parent) => {
                return (0);
            }), Constraint.RelativeToParent((parent) => {
                return parent.Width * 0.8;
            }), Constraint.RelativeToParent((parent) => {
                return parent.Height * parent.Height / (58 * 12 - 4);
            }));

            horizontalBarArea = new RelativeLayout();

            horizontalBarArea.Children.Add(horizontalScrollBar, Constraint.RelativeToParent((parent) => {
                return (0);
            }), Constraint.RelativeToParent((parent) => {
                return (parent.Height * 0.1);
            }), Constraint.RelativeToParent((parent) => {
                return parent.Width * parent.Width / (58 * 16 - 4);
            }), Constraint.RelativeToParent((parent) => {
                return parent.Height * 0.8;
            }));

            

            //Add the Relative layouts that will hold the sidebars to the mastergrid
            mastergrid.Children.Add(verticalBarArea, 0, 0); 
            mastergrid.Children.Add(horizontalBarArea, 1, 1);

            // Add the scroller (which contains stepgrid) and sidebar to mastergrid

            mastergrid.Children.Add(scroller, 1, 0); // Add scroller to first column of mastergrid
            mastergrid.Children.Add(sidebar, 2, 0);  // Add sidebar to final column of mastergrid
            Grid.SetRowSpan(sidebar, 2);  //make sidebar take up both rows in rightmost column


            // Tried using a timer to call updateScrollBars, is not reliable
            //Timer timer = new Timer();


            //timer.Elapsed += updateScrollBars;
            //timer.Interval = 30;
            //timer.Start();

            scroller.Scrolled += updateScrollBars;     //scrolled event that calls method to update scrollbars.

            Content = mastergrid;            
        }

        
        /// <summary>
        /// This method redraws the scrollbars at a location that properly represents the location in the scrollview
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void updateScrollBars(Object o, ScrolledEventArgs e)//object sender, EventArgs e)
        {           
            {
                //uncomment the line below and uncomment ");" near the end of this method to try method with timers
                //also change ScrolledEventArgs to EventArgs in paramaters of this method to test with timer.
                //also comment out adding this method to event listener scroller.Scrolled (code near end of

                //Device.BeginInvokeOnMainThread(delegate ()               
                {
                    if (scroller.ScrollX != 0)   //when scroller.ScrollX = 0, the code is really glitchy. Same goes for scroller.ScrollY
                    {
                        horizontalBarArea.Children.Remove(horizontalScrollBar);
                        horizontalBarArea.Children.Add(horizontalScrollBar, Constraint.RelativeToParent((parent) =>
                        {
                            return (parent.Width - parent.Width * parent.Width / (58 * 16 - 4)) * scroller.ScrollX / ((58 * 16 - 4) - parent.Width); // x location to place bar, updated by pos in scroll
                        }), Constraint.RelativeToParent((parent) =>
                        {
                            return (0.1 * parent.Height - 1); // y location to place bar
                        }), Constraint.RelativeToParent((parent) => {
                            return parent.Width * parent.Width / (58 * 16 - 4); //width of bar
                        }), Constraint.RelativeToParent((parent) => {
                            return parent.Height * 0.8; //height of bar
                        }));
                        
                    }
                    if (scroller.ScrollY != 0)
                    {
                        verticalBarArea.Children.Remove(verticalScrollBar);
                        verticalBarArea.Children.Add(verticalScrollBar, Constraint.RelativeToParent((parent) =>
                        {
                            return 0.1 * parent.Width + 1; // x location to place bar
                        }), Constraint.RelativeToParent((parent) =>
                        {
                            return (parent.Height - parent.Height * parent.Height / (58 * 12 - 4)) * scroller.ScrollY / ((58 * 12 - 4) - parent.Height); // y location to place bar, updated by pos in scroll
                        }), Constraint.RelativeToParent((parent) => {
                            return parent.Width * 0.8;      //width of bar
                        }), Constraint.RelativeToParent((parent) => {
                            return parent.Height * parent.Height / (58 * 12 - 4); //height of bar
                        }));              
                    }

                }//);
            }
            
        }

        public void ReplaceInstruments(Instrument[] instruments)
        {           
            List<Instrument> oldInstruments = new List<Instrument>();           //
            List<Instrument> newInstruments = new List<Instrument>();           //
            for (int i = 0; i < instruments.Length; i++)                        //
            {                                                                   //
                Instrument oldInstr = song.Instruments[i];                      //
                Instrument newInstr = instruments[i];                           // Figure out which instruments have changed
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
            stepgrid = new Grid { ColumnSpacing = 4, RowSpacing = 4 };

            //Initialize the number of rows and columns for the tempGrid
            for (int i = 0; i < NumRows; i++)
            {

                stepgrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(54, GridUnitType.Absolute) });
            }
            for (int i = 0; i < NumColumns; i++)
            {
                //tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(54, GridUnitType.Absolute) });
                stepgrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(68, GridUnitType.Absolute) });

            }

            //Add grids to the tempGrid, and give each 2 columns, two rows and a BoxView
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    int semitoneShift = (NumRows - 1) - i;
                    MiniGrid miniGrid = new MiniGrid(semitoneShift);
                    miniGrid.Tap += OnMiniGridTapped;
                    stepgrid.Children.Add(miniGrid, j, i);
                    if(j % 8 == 0)
                    {
                        Label noteLabel = new Label
                        {
                            Text = Instrument.SemitoneShiftToString(semitoneShift),
                            //VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 17,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = new Color(0.18),
                            Margin = new Thickness(4, 3, 0, 0),
                            InputTransparent = true
                        };
                        stepgrid.Children.Add(noteLabel, j, i);
                    }
                }
            }
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
                    SongPlayer.PlayNote(selectedInstrument.AtPitch(miniGrid.semitoneShift));   // Play note so long as not already playing a song
                }
            }
            else
            {
                song.RemoveNote(toggledNote, Grid.GetColumn(miniGrid));
            }

            //Undo clear stops woking when user adds stuff to grid so they don't accidentally undo clear
            clearedSong = null;
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
            stepgrid.Children.Add(highlight, 0, 0);
            Grid.SetRowSpan(highlight, NumRows);
            player.BeginPlaying(song);
            playStopButton.Image = "stop.png";
        }

        /// <summary>
        /// Stops playing the song
        /// </summary>
        public void StopPlayingSong()
        {
            if (player.IsPlaying)
            {
                player.StopPlaying();
                playStopButton.Image = "play.png";
                stepgrid.Children.Remove(highlight);
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
                SongPlayer.PlayNote(button.Instrument.AtPitch(0));
            }
            if (button != selectedInstrButton)
            {
                //Remove border fom previously selected instrument
                selectedInstrButton.Selected = false;
                button.Selected = true;             //Change border highlight to yellow
                selectedInstrButton = button;		//Set this button to be the currently selected button
            }
        }            

        /// <summary>
        /// Highlights the current column (beat) and de-highlights the previous column so long as this isn't the first note played
        /// </summary>
        void HighlightColumns(int currentBeat, bool firstBeat)
        {
            Device.BeginInvokeOnMainThread(delegate ()
            {
                Grid.SetColumn(highlight, currentBeat);
            });
        }
    }
}
