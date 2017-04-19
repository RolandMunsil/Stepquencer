using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Xamarin.Forms;

namespace Stepquencer
{
    public partial class MainPage : ContentPage
    {
        const int NumRows = 12;
        const int NumColumns = 16;
        const int NumInstruments = 4;
        const double brightnessIncrease = 0.25;						// Amount to increase the red, green, and blue values of each button when it's highlighted

        readonly static Color LightGrey = Color.FromHex("#ebf5f4");
        public readonly static Color Grey = Color.FromHex("#606060");
        readonly static Color Red = Color.FromHex("#ff0000");
        readonly static Color Blue = Color.FromHex("#3333ff");
        readonly static Color Green = Color.FromHex("#33ff33");
        readonly static Color Yellow = Color.FromHex("#ffff00");

        public Grid mastergrid;
        public Grid stepgrid;                                       // Grid for whole screen
        Grid sidebar;						                 // Grid for sidebar

        public ScrollView scroller;                                 // ScrollView that will be used to scroll through stepgrid
        RelativeLayout verticalBarArea;
        RelativeLayout horizontalBarArea;
        BoxView verticalScrollBar;                            // Scroll bar to show position on vertical scroll
        BoxView horizontalScrollBar;                             // Scroll bar to show position on horizontal scroll
        public int currentTempo;

        public Song song;                                    // Array of HashSets of Songplayer notes
        public Dictionary<Color, Instrument> colorMap;       // Dictionary mapping colors to instrument

        public Color sideBarColor = Red;
        Button selectedInstrButton = null;

        BoxView highlight;

        SongPlayer player;


        public MainPage()
        {
            InitializeComponent();
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

            BackgroundColor = Color.FromHex("#000000");     // Make background color black


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
                Button button = new Button { Font = Font.SystemFontOfSize(10), BackgroundColor = colors[i - 1], BorderColor = Color.Black, BorderWidth = 3 };     // Make a new button
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

            // Make the stepgrid and fill it with boxes
            MakeNewStepGrid();

            // Set up scroll view and put grid inside it
            scroller = new ScrollView {
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
                BackgroundColor = MainPage.LightGrey,
                WidthRequest = 1,
                HeightRequest = 1,
            };

            //make vertical scrollbar, size doesn't matter because constraints are used to size it later
            horizontalScrollBar = new BoxView
            {
                BackgroundColor = MainPage.LightGrey,
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
            player.BeatStarted += HighlightColumns;
            
            
        }

        
        /// <summary>
        /// This method redraws the scrollbars at a location that properly represents the location in the scrollview
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void updateScrollBars(Object o, ScrolledEventArgs e)//object sender, EventArgs e)
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


        /// <summary>
        /// Method to make a new, empty stepGrid
        /// </summary>
        public void MakeNewStepGrid()
        {

            //Set up grid of note squares
            Grid tempGrid = new Grid { ColumnSpacing = 4, RowSpacing = 4 };


            //Initialize the number of rows and columns for the tempGrid
            for (int i = 0; i < NumRows; i++)
            {
                tempGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(54, GridUnitType.Absolute) });
            }
            for (int i = 0; i < NumColumns; i++)
            {
                tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(54, GridUnitType.Absolute) });
            }


            //Add grids to the tempGrid, and give each 2 columns, two rows and a BoxView
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    tempGrid.Children.Add(new MiniGrid(this, (NumRows - 1) - i), j, i);
                }
            }

            stepgrid = tempGrid;
        }

        public void SetSong(Song song)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Event handler for the Play/Stop button
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnPlayStopClicked(object sender, EventArgs e)
        {
            if (player.IsPlaying)
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
