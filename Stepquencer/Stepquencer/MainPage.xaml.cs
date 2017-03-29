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

        readonly static Color Grey = Color.FromHex("#606060");
        readonly static Color Red = Color.FromHex("#ff0000");
        readonly static Color Blue = Color.FromHex("#3333ff");
        readonly static Color Green = Color.FromHex("#33ff33");
        readonly static Color Yellow = Color.FromHex("#ffff00");

        Grid mastergrid;
        Grid stepgrid;                                       // Grid for whole screen
        Grid sidebar;						                 // Grid for sidebar
        ScrollView scroller;                                 // ScrollView that will be used to scroll through stepgrid
        Song song;                 // Array of HashSets of Songplayer notes
        Dictionary<Color, Instrument> colorMap;   // Dictionary mapping colors to instrument

        Color sideBarColor = Red;
        Button selectedInstrButton = null;

        BoxView highlight;

        SongPlayer player;
        object highlightingSyncObject = new object();

        public MainPage()
        {
            InitializeComponent();

            highlight = new BoxView() { Color = Color.White, Opacity = brightnessIncrease };
            highlight.InputTransparent = true;

            // Initializing the song player and noteArray
            song = new Song(NumColumns);

            player = new SongPlayer(song);

            // Initializing the colorMap
            colorMap = new Dictionary<Color, Instrument>();

            colorMap[Red] = Instrument.LoadByName("Snare");
            colorMap[Blue] = Instrument.LoadByName("YRM1x Atmosphere");
            colorMap[Green] = Instrument.LoadByName("Bass Drum");
            colorMap[Yellow] = Instrument.LoadByName("Hi-Hat");

            BackgroundColor = Color.FromHex("#000000");     // Make background color black

            //Set up a master grid with 2 columns to eventually place stepgrid and sidebar in.
            mastergrid = new Grid { ColumnSpacing = 2};
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            //Set up grid of note squares
            stepgrid = new Grid { ColumnSpacing = 4, RowSpacing = 4 };

            //Initialize the number of rows and columns
            for (int i = 0; i < NumRows; i++)
            {
                stepgrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < NumColumns; i++)
            {
                stepgrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }


            //Add grids to the grid, and give each 2 columns, two rows and a BoxView
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    Grid colorGrid = new Grid { ColumnSpacing = 0, RowSpacing = 0, BackgroundColor = Color.Black };		// Make a grid
                    colorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    colorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Add in column definitions
                    colorGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    colorGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });  // Add in row definitions

                    // Add in boxviews
                    BoxView box1 = new BoxView { BackgroundColor = Grey };
                    BoxView box2 = new BoxView { BackgroundColor = Grey };
                    BoxView box3 = new BoxView { BackgroundColor = Grey };
                    BoxView box4 = new BoxView { BackgroundColor = Grey };

                    colorGrid.Children.Add(box1, 0, 0);
                    colorGrid.Children.Add(box2, 1, 0);
                    colorGrid.Children.Add(box3, 0, 1);
                    colorGrid.Children.Add(box4, 1, 1);

                    Grid.SetColumnSpan(box1, 2);
                    Grid.SetRowSpan(box1, 2);


                    // Give the grid a tapGesture recognizer
                    TapGestureRecognizer tgr = new TapGestureRecognizer();
                    tgr.CommandParameter = colorGrid;
                    tgr.Command = new Command(OnColorGridTapped);
                    colorGrid.GestureRecognizers.Add(tgr);

                    // Give box1 a tapGesture recognizer
                    TapGestureRecognizer tgr1 = new TapGestureRecognizer();
                    tgr1.CommandParameter = box1;
                    tgr1.Command = new Command(OnColorGridTapped);
                    box1.GestureRecognizers.Add(tgr1);

                    // Give box2 a tapGesture recognizer
                    TapGestureRecognizer tgr2 = new TapGestureRecognizer();
                    tgr2.CommandParameter = box2;
                    tgr2.Command = new Command(OnColorGridTapped);
                    box2.GestureRecognizers.Add(tgr2);

                    // Give box3 a tapGesture recognizer

                    TapGestureRecognizer tgr3 = new TapGestureRecognizer();
                    tgr3.CommandParameter = box3;
                    tgr3.Command = new Command(OnColorGridTapped);
                    box3.GestureRecognizers.Add(tgr3);

                    // Give box4 a tapGesture recognizer
                    TapGestureRecognizer tgr4 = new TapGestureRecognizer();
                    tgr4.CommandParameter = box4;
                    tgr4.Command = new Command(OnColorGridTapped);
                    box4.GestureRecognizers.Add(tgr4);

                    stepgrid.Children.Add(colorGrid, j, i);
                }
            }



            // Make the sidebar
            sidebar = new Grid { ColumnSpacing = 1, RowSpacing = 1 };

            for (int i = 0; i < NumInstruments + 1; i++)
            {
                sidebar.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            sidebar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            // Fill sidebar it with buttons
            Color[] colors = new Color[] { Red, Blue, Green, Yellow };      // Array of colors

            for (int i = 0; i < colors.Length; i++)
            {
                Button button = new Button { BackgroundColor = colors[i], BorderColor = Color.Black, BorderWidth = 3 };     // Make a new button
                sidebar.Children.Add(button, 0, i);                                 // Add it to the sidebar
                button.Clicked += OnSidebarClicked;                                 // Add to sidebar event handler

                if (button.BackgroundColor.Equals(Color.Red))
                {
                    button.BorderColor = Color.FromHex("#ffff00");
                    selectedInstrButton = button;                         //Button now in use
                }
            }

            //Play/stop button
            Button playStopButton = new Button
            {
                BackgroundColor = Color.Black,
                Font = Font.SystemFontOfSize(40),
                BorderRadius = 0,
            };
            playStopButton.Text = "\u25BA";
            sidebar.Children.Add(playStopButton, 0, 4);
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
                System.Threading.Monitor.Enter(highlightingSyncObject);
                stepgrid.Children.Remove(highlight);
                System.Threading.Monitor.Exit(highlightingSyncObject);
            }
            else
            {
                player.BeginPlaying(240);
                ((Button)sender).Text = "■";
            }
        }

        /// <summary>
        /// Event handler for normal buttons in the grid
        /// </summary>

        void OnColorGridTapped(object obj)
        {
            Grid grid;

            if (obj.GetType() == typeof(BoxView))       // If user clicks on a box, get the box's parent grid
            {
                grid = (Grid)(((BoxView)obj).Parent);
            }
            else                                        // Otherwise, the user must have clicked on the grid.
            {
                grid = (Grid) obj;
            }


            List<Color> colors = ChangeColor(grid);      // Changes UI represenation and returns colors that new button has


            if (!colors.Contains(sideBarColor))     // If sidebar color isn't part of button's new set of colors, remove it
            {
                Instrument.Note toRemove = colorMap[sideBarColor].AtPitch((NumRows - 1) - Grid.GetRow(grid));
                song.RemoveNote(toRemove, Grid.GetColumn(grid));
            }

            else

            {
                Instrument.Note toAdd = colorMap[sideBarColor].AtPitch((NumRows - 1) - Grid.GetRow(grid));
                song.AddNote(toAdd, Grid.GetColumn(grid));
            }

        }

        /// <summary>
        /// Correctly changes UI representation of a note when clicked and returns list of colors in new note
        /// </summary>
        /// <param name="grid">Grid.</param>
        List<Color> ChangeColor(Grid grid)
        {
            BoxView topLeft = (BoxView)grid.Children.ElementAt(0);	// Gets the top left box
            BoxView topRight = (BoxView)grid.Children.ElementAt(1);     // Might be 2, gets top right box
            BoxView bottomRight = (BoxView)grid.Children.ElementAt(3);	// gets bottom right box
            BoxView bottomLeft = (BoxView)grid.Children.ElementAt(2);   // Might be 1, gets bottom left box

            List<Color> colorList = new List<Color>();		// List to store the colors to be added to the new button

            foreach (BoxView box in grid.Children)
            {
                if (box.BackgroundColor != Grey)
                {
                    colorList.Add(box.BackgroundColor);	// Add the box's color to the array

                    box.BackgroundColor = Grey;             // Default the box's color to grey   
                    Grid.SetRowSpan(box, 1);                // Default the box to span one row
                    Grid.SetColumnSpan(box, 1);             // Default the box to span one column               
                }
                
                box.IsVisible = false;
            }


            if (colorList.Contains(sideBarColor))			// If button already has the sidebar color, remove it from colorList
            {
                colorList.Remove(sideBarColor);
            }
            else    										// If button doesn't already have the sidebar color, add it to colorList
            {
                colorList.Add(sideBarColor);
            }

            colorList.Sort((c1, c2)=>Math.Sign(c1.Hue - c2.Hue));


            if (colorList.Count == 0)		// If the box is reverting to grey
            {
                topLeft.IsVisible = true;

                Grid.SetRowSpan(topLeft, 2);
                Grid.SetColumnSpan(topLeft, 2);     // Make one box take up the whole space
            }
            else if (colorList.Count == 1)		// If box will have one color
            {
                topLeft.IsVisible = true;

                topLeft.BackgroundColor = colorList[0];

                Grid.SetRowSpan(topLeft, 2);
                Grid.SetColumnSpan(topLeft, 2);     // Make one button take up the whole space
            }
            else if (colorList.Count == 2)	// If box will have two colors
            {
                topLeft.IsVisible = true;
                topRight.IsVisible = true;

                topLeft.BackgroundColor = colorList[0];
                topRight.BackgroundColor = colorList[1];

                Grid.SetRowSpan(topLeft, 2);		// Make topLeft take up half of grid
                Grid.SetRowSpan(topRight, 2);		// Make topRight take up half of grid
            }
            else if (colorList.Count == 3)	// If box will have three colors
            {
                topLeft.IsVisible = true;
                topRight.IsVisible = true;
                bottomRight.IsVisible = true;	//Make topright and bottomright visible, make bottom left invisible

                topLeft.BackgroundColor = colorList[0];
                topRight.BackgroundColor = colorList[1];
                bottomRight.BackgroundColor = colorList[2];

                Grid.SetRowSpan(topLeft, 2);	// Make topLeft up take up half the grid; other two split the remaining space
            }
            else                    // If box will have four colors
            {
                topRight.IsVisible = true;
                topLeft.IsVisible = true;
                bottomRight.IsVisible = true;		// Make all boxes visible
                bottomLeft.IsVisible = true;

                // Assign the correct colors
                topLeft.BackgroundColor = colorList[0];
                topRight.BackgroundColor = colorList[1];
                bottomLeft.BackgroundColor = colorList[2];
                bottomRight.BackgroundColor = colorList[3];

                // No resizing needed
            }

            return colorList;
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

                button.BorderColor = Color.Yellow;   //Change border highlight to yellow
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
                if (!System.Threading.Monitor.TryEnter(highlightingSyncObject))
                {
                    //Dehighlighting of entire grid has already started.
                    return;
                }
                if (!player.IsPlaying)
                {
                    //Player has stopped so the grid will already be dehighlighted.
                    return;
                }

                if (firstBeat)
                {
                    stepgrid.Children.Add(highlight, 0, 0);
                    Grid.SetRowSpan(highlight, NumRows);
                }

                Grid.SetColumn(highlight, currentBeat);

                System.Threading.Monitor.Exit(highlightingSyncObject);
            });
        }
    }
}
