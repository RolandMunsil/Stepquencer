using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Stepquencer
{
    /// <summary>
    /// Main page of the application. A place where users can make music, play and pause it, and go into another menu for more actions
    /// </summary>

    public partial class MainPage : ContentPage
    {
        const int NumRows = 13;                     // Number of rows of MiniGrids that users can tap and add sounds to
        const int NumColumns = 8;                   // Number of columns of Minigrids
        const int NumInstruments = 4;               // Number of instruments on the sidebar
        const double brightnessIncrease = 0.25;		// Amount to increase the red, green, and blue values of each button when it's highlighted

        public static readonly String[] INITIAL_INSTRUMENTS = { "Snare", "YRM1xAtmosphere", "SlapBassLow", "HiHat" };   // The set of instruments availble when first loading up the app

        SongPlayer player;                              // Plays the notes loaded into the current Song object
        public Song song;                               // Array of HashSets of Songplayer notes that holds the current song
        public int currentTempo = 240;                  // Keeps track of tempo, initialized at 240

        public InstrumentButton[] instrumentButtons;    // An array of the instrument buttons on the sidebar
        InstrumentButton selectedInstrButton;           // Currently selected sidebar button

        public ScrollView scroller;                     // ScrollView that will be used to scroll through stepgrid
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


            // Initialize the SongPlayer and noteArray
            song = new Song(NumColumns);
            player = new SongPlayer();

            //Set up a master grid with 2 columns to hold stepgrid and sidebar.
            mastergrid = new Grid { ColumnSpacing = 2 };
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Make the stepgrid and fill it with boxes
            MakeStepGrid();


            // Make the sidebar
            sidebar = new Grid { ColumnSpacing = 1, RowSpacing = 1 };

            sidebar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < NumInstruments + 2; i++)
            {
                sidebar.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            // Fill sidebar with buttons and put them in the instrumentButtons array
            instrumentButtons = new InstrumentButton[INITIAL_INSTRUMENTS.Length];
            for (int i = 0; i < INITIAL_INSTRUMENTS.Length; i++)
            {
                InstrumentButton button = new InstrumentButton(Instrument.GetByName(INITIAL_INSTRUMENTS[i]));   // Make a new button

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
                Orientation = ScrollOrientation.Vertical  //Both vertical and horizontal orientation
            };
            scroller.Content = stepgrid;

            // Add the scroller (which contains stepgrid) and sidebar to mastergrid
            mastergrid.Children.Add(scroller, 0, 0); 
            mastergrid.Children.Add(sidebar, 1, 0);  
                                                     
            Content = mastergrid;
        }

        public void SetSidebarInstruments(Instrument[] instruments)
        {
            for (int i = 0; i < instrumentButtons.Length; i++)
            {
                instrumentButtons[i].Instrument = instruments[i];
            }
        }

        /// <summary>
        /// Method to make a fresh stepGrid
        /// </summary>
        public void MakeStepGrid()
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
                stepgrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
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
            this.song = new Song(song.BeatCount);
        }


        /// <summary>
        /// Sets the current song (and grid represntation) to be the given song.
        /// </summary>
        public void SetSong(Song song)
        {
            this.song = song;

            Dictionary<int, List<Color>>[] colorsAtShiftAtBeat = new Dictionary<int, List<Color>>[song.BeatCount];
            for (int i = 0; i < song.BeatCount; i++)
            {
                Instrument.Note[] notes = song.NotesAtBeat(i);
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
        /// <param name="miniGrid">miniGrid.</param>
        void OnMiniGridTapped(MiniGrid miniGrid)
        {
            Instrument selectedInstrument = selectedInstrButton.Instrument;

            // Changes UI represenation and returns new set of colors on this grid
            List<Color> colors = miniGrid.ToggleColor(selectedInstrument.color);

            // If sidebar color isn't part of button's new set of colors, remove it
            Instrument.Note toggledNote = selectedInstrument.AtPitch(miniGrid.semitoneShift);

            //If sidebar button color = clicked button color
            if (!colors.Contains(selectedInstrument.color))
            {
                song.RemoveNote(toggledNote, Grid.GetColumn(miniGrid));
            }

            //If sidebar button color != clicked button color 
            else if (colors.Contains(selectedInstrument.color))
            {
                song.AddNote(toggledNote, Grid.GetColumn(miniGrid));        // Add the note

                if (!player.IsPlaying)
                {
                    SongPlayer.PlayNote(selectedInstrument.AtPitch(miniGrid.semitoneShift));   // Play note so long as not already playing a song
                }
            }

            //Undo clear stops woking when user adds stuff to grid so they don't accidentally undo clear
            MoreOptionsPage.clearedSong = null;
        }


        /// <summary>
        /// Event handler for the Play/Stop button
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnPlayStopClicked(object sender, EventArgs e)
        {
            if(player.IsPlaying)
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
            player.BeginPlaying(song, currentTempo);
            playStopButton.Image = "stop.png";
        }


        /// <summary>
        /// Stops playing the song
        /// </summary>
        private void StopPlayingSong()
        {
            player.StopPlaying();
            playStopButton.Image = "play.png";
            stepgrid.Children.Remove(highlight);
        }


        /// <summary>
        /// Event handler for the more options button. Sends user to more options page.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
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
        /// Clicking new button if button is in use (dehighlighting old button) does not work quite yet,
        /// 
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
        /// <param name="currentBeat">Current beat.</param>
        void HighlightColumns(int currentBeat, bool firstBeat)
        {
            Device.BeginInvokeOnMainThread(delegate ()
            {
                Grid.SetColumn(highlight, currentBeat);
            });
        }
    }
}
