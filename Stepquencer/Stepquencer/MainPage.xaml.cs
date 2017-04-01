using System;
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

        Grid mastergrid;
        Grid stepgrid;                                       // Grid for whole screen
        Grid sidebar;						                 // Grid for sidebar
        ScrollView scroller;                                 // ScrollView that will be used to scroll through stepgrid

        public Song song;                                    // Array of HashSets of Songplayer notes
        public Song bass;
        public Song synth;
        public Song snare;
        public Song hihat;
        public Dictionary<Color, Instrument> colorMap;       // Dictionary mapping colors to instrument

        public Color sideBarColor = Red;
        Button selectedInstrButton = null;

        BoxView highlight;

        SongPlayer player;
        SongPlayer sidebarBass;
        SongPlayer sidebarSynth;
        SongPlayer sidebarSnare;
        SongPlayer sidebarHiHat;
        object highlightingSyncObject = new object();


        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            highlight = new BoxView() { Color = Color.White, Opacity = brightnessIncrease };
            highlight.InputTransparent = true;
            highlight.IsVisible = false;


            // Initializing the song player and noteArray
            song = new Song(NumColumns);
            player = new SongPlayer(song);


            // Initializing the colorMap
            colorMap = new Dictionary<Color, Instrument>();

            colorMap[Red] = Instrument.LoadByName("Bass Drum");
            colorMap[Blue] = Instrument.LoadByName("YRM1x Atmosphere");
            colorMap[Green] = Instrument.LoadByName("Snare");
            colorMap[Yellow] = Instrument.LoadByName("Hi-Hat");

            BackgroundColor = Color.FromHex("#000000");     // Make background color black


            //Set up a master grid with 2 columns to eventually place stepgrid and sidebar in.
            mastergrid = new Grid { ColumnSpacing = 2};
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            //Set up grid of note squares
            stepgrid = new Grid { ColumnSpacing = 4, RowSpacing = 4 };


            //Initialize the number of rows and columns for the stepgrid
            for (int i = 0; i < NumRows; i++)
            {
                stepgrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < NumColumns; i++)
            {
                stepgrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }


            //Add grids to the stepgrid, and give each 2 columns, two rows and a BoxView
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    stepgrid.Children.Add(new MiniGrid(this,(NumRows - 1) - i), j, i);
                }
            }

            stepgrid.Children.Add(highlight, 0, 0);
            Grid.SetRowSpan(highlight, NumRows);



            // Make the sidebar
            sidebar = new Grid { ColumnSpacing = 1, RowSpacing = 1 };

            for (int i = 0; i < NumInstruments + 2; i++)
            {
                sidebar.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            sidebar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });



            //Define sounds in sidebar
            bass = new Song(1);
            bass.AddNote(colorMap[Red].AtPitch(3), 0);          //Add note to song + Not sure how to define pitch
            sidebarBass = new SongPlayer(bass);

            synth = new Song(1);
            //synth.AddNote(colorMap[Blue].AtPitch(3), 1);
            sidebarSynth = new SongPlayer(synth);

            snare = new Song(1);
            sidebarSnare = new SongPlayer(snare);

            hihat = new Song(1);
            sidebarHiHat = new SongPlayer(hihat);




            // Fill sidebar it with buttons
            Color[] colors = new Color[] { Green, Blue, Red, Yellow };      // Array of colors
            for (int i = 1; i <= colors.Length; i++)
            {
                Button button = new Button { Font = Font.SystemFontOfSize(10), BackgroundColor = colors[i-1], BorderColor = Color.Black, BorderWidth = 3 };     // Make a new button
                sidebar.Children.Add(button, 0, i);                                 // Add it to the sidebar
                button.Clicked += OnSidebarClicked;                                 // Add to sidebar event handler

                if (i == 1)
                {
                    button.Image = "editedinst.png";
                }
                if (i == 2)
                {
                    button.Image = "editedpiano.png";
                }
                if (i == 3)
                {
                    button.Image = "editedbass.png";
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
                Text = "->",
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
                highlight.IsVisible = false;
            }
            else
            {
                player.BeginPlaying(240);
                ((Button)sender).Text = "■";
                
                highlight.IsVisible = true;
                Grid.SetColumn(highlight, 0);
            }
        }


        /// <summary>
        /// Event handler for the more options button. Sends user to more options page.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnMoreOptionsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MoreOptionsPage());
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

            //if (sideBarColor == Red)
            //{
            //    sidebarBass.BeginPlaying(240);
            //}
            //TODO: Have sidebar buttons make designated sound when clicked/tapped

            if (button == selectedInstrButton) //User has selected the instrument that is aleady selected

            {
                return;
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
