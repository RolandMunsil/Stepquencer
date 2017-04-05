﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Stepquencer
{
    public partial class MainPage : ContentPage
    {
        const int NumRows = 12;
        const int NumColumns = 8;
        const int NumInstruments = 4;
        const double brightnessIncrease = 0.25;						// Amount to increase the red, green, and blue values of each button when it's highlighted

        public readonly static Color Grey = Color.FromHex("#606060");
        readonly static Color Red = Color.FromHex("#ff0000");
        readonly static Color Blue = Color.FromHex("#3333ff");
        readonly static Color Green = Color.FromHex("#33ff33");
        readonly static Color Yellow = Color.FromHex("#ffff00");

        public Grid mastergrid;
        public Grid stepgrid;                                       // Grid for whole screen
        Grid sidebar;						                 // Grid for sidebar

        public ScrollView scroller;                                 // ScrollView that will be used to scroll through stepgrid
        public int currentTempo;

        public Song song;                                    // Array of HashSets of Songplayer notes

        public Dictionary<Color, Instrument> colorMap;       // Dictionary mapping colors to instrument
        public Dictionary<Instrument, Color> instrumentMap;       // Dictionary mapping instrument to color

        public Color sideBarColor = Red;
        Button selectedInstrButton = null;

        BoxView highlight;

        SongPlayer player;


        public MainPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("#000000");     // Make background color black
            NavigationPage.SetHasNavigationBar(this, false);    // Make sure navigation bar doesn't show up on this screen

            if (currentTempo == 0)          
            {
                currentTempo = 240;                   // If the tempo hasn't been changed, initialize it to 240
            }

            // Initialize the highlight box
            highlight = new BoxView() { Color = Color.White, Opacity = brightnessIncrease };
            highlight.InputTransparent = true;

            // Initializing the song player and noteArray
            song = new Song(NumColumns);
            player = new SongPlayer(song);

            // Initializing the colorMap
            colorMap = new Dictionary<Color, Instrument>();

            colorMap[Red] = Instrument.LoadByName("Snare");                 //Red = Snare
            colorMap[Blue] = Instrument.LoadByName("YRM1x Atmosphere");     //Blue = Synth
            colorMap[Green] = Instrument.LoadByName("Slap Bass Low");           //Green = Bass Drum
            colorMap[Yellow] = Instrument.LoadByName("Hi-Hat");             //Yellow = Hi-Hat


            //Initialize instrument map
            instrumentMap = new Dictionary<Instrument, Color>();
            foreach (var kvPair in colorMap)
            {
                instrumentMap.Add(kvPair.Value, kvPair.Key);
            }



            //Set up a master grid with 2 columns to eventually place stepgrid and sidebar in.
            mastergrid = new Grid { ColumnSpacing = 2};
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Make the stepgrid and fill it with boxes
            MakeNewStepGrid();


            // Make the sidebar
            sidebar = new Grid { ColumnSpacing = 1, RowSpacing = 1 };

            for (int i = 0; i < NumInstruments + 2; i++)
            {
                sidebar.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            sidebar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });



            // Fill sidebar it with buttons
            Color[] colors = new Color[] { Red, Blue, Green, Yellow };      // Array of colors
            for (int i = 1; i <= colors.Length; i++)
            {
                Button button = new Button { Font = Font.SystemFontOfSize(10), BackgroundColor = colors[i-1], BorderColor = Color.Black, BorderWidth = 3 };     // Make a new button
                sidebar.Children.Add(button, 0, i);                                 // Add it to the sidebar
                button.Clicked += OnSidebarClicked;                                 // Add to sidebar event handler

                if (i == 1)
                {
                    button.Image = "editedsnaree.png";   //Show snare image w/ red
                }
                if (i == 2)
                {
                    button.Image = "editedpiano.png";
                }
                if (i == 3)
                {
                    button.Image = "editedbasss.png";
                }
                if (i == 4)
                {
                    button.Image = "editedhihat.png";
                }

                if (button.BackgroundColor.Equals(Color.Red))       // Initialize red sidebar button to be highlighted
                {
                    button.BorderColor = Color.White;
                    selectedInstrButton = button;                         //Button now in use
                }
            }

            // More options button
            Button moreOptionsButton = new Button
            {
                BackgroundColor = Color.Black,
                Font = Font.SystemFontOfSize(40),
                Text = "\u2699",
                TextColor = Color.White
            };
            sidebar.Children.Add(moreOptionsButton, 0, 0);
            moreOptionsButton.Clicked += OnMoreOptionsClicked;

            // Play/stop button
            Button playStopButton = new Button
            {
                BackgroundColor = Color.Black,
                Font = Font.SystemFontOfSize(40),
                TextColor = Color.White,
                BorderRadius = 0,
            };
            playStopButton.Text = "\u25BA";
            sidebar.Children.Add(playStopButton, 0, 5);
            playStopButton.Clicked += OnPlayStopClicked;

            // Set up scroll view and put grid inside it
            scroller = new ScrollView {
                Orientation = ScrollOrientation.Vertical  //Both vertical and horizontal orientation
            };
            scroller.Content = stepgrid;

            // Add the scroller (which contains stepgrid) and sidebar to mastergrid
            mastergrid.Children.Add(scroller, 0, 0); // Add scroller to first column of mastergrid
            mastergrid.Children.Add(sidebar, 1, 0);  // Add sidebar to final column of mastergrid
                                                     //Grid.SetRowSpan(sidebar, NumRows);                  // Make sure that it spans the whole column

            Content = mastergrid;
            player.BeatStarted += HighlightColumns;
        }


        /// <summary>
        /// Method to make a fresh stepGrid
        /// </summary>
        public void MakeNewStepGrid()
        {

            //Set up grid of note squares
            Grid tempGrid = new Grid { ColumnSpacing = 4, RowSpacing = 4 };


            //Initialize the number of rows and columns for the tempGrid
            for (int i = 0; i < NumRows; i++)
            {
                tempGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < NumColumns; i++)
            {
                tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }


            //Add grids to the tempGrid, and give each 2 columns, two rows and a BoxView
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    int semitoneShift = (NumRows - 1) - i;
                    tempGrid.Children.Add(new MiniGrid(this, semitoneShift), j, i);
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
                        tempGrid.Children.Add(noteLabel, j, i);
                    }
                }
            }

            stepgrid = tempGrid;
        }

        public void SetSong(Song song)
        {
            this.song = song;
            this.player.Song = song;

            Dictionary<int, List<Color>>[] colorsAtShiftAtBeat = new Dictionary<int, List<Color>>[song.BeatCount];
            for (int i = 0; i < song.BeatCount; i++)
            {
                Instrument.Note[] notes = song.NotesAtBeat(i);
                colorsAtShiftAtBeat[i] = notes.GroupBy(n => n.semitoneShift)
                                              .ToDictionary(
                                                     g => g.Key,
                                                     g => g.Select(n => instrumentMap[n.instrument]).ToList()
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
        /// Event handler for the Play/Stop button
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnPlayStopClicked(object sender, EventArgs e)
        {
            if(player.IsPlaying)
            {
                player.StopPlaying();
                ((Button)sender).Text = "\u25BA";

                stepgrid.Children.Remove(highlight);
            }
            else
            {
                stepgrid.Children.Add(highlight, 0, 0);
                Grid.SetRowSpan(highlight, NumRows);
                player.BeginPlaying(currentTempo);
                ((Button)sender).Text = "■";
            }
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
                player.StopPlaying();
                Button PlayButton = (Button)sidebar.Children.ElementAt(5);     // Stop the song, adjust play button appropriately
                PlayButton.Text = "\u25BA";
                stepgrid.Children.Remove(highlight);
            }

            await Navigation.PushAsync(new MoreOptionsPage(this, song));

        }


        /// <summary>
        /// Event handler for buttons in the sidebar
        /// </summary>
        /// Clicking new button if button is in use (dehighlighting old button) does not work quite yet,
        /// 
        void OnSidebarClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            sideBarColor = button.BackgroundColor;


            if (!player.IsPlaying)  // So long as the music isn't currently playing, the sidebar buttons play their sound when clicked
            {
                SongPlayer.PlayNote(colorMap[sideBarColor].AtPitch(3));
            }

            if (button.BorderColor == Color.Black)
            {
                //Remove border fom previously selected instrument
                if(selectedInstrButton != null)
                {
                    selectedInstrButton.BorderColor = Color.Black;
                }

                button.BorderColor = Color.White;   //Change border highlight to yellow
                selectedInstrButton = button;				     //Set this button to be the currently selected button

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
