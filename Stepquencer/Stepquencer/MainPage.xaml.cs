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
		const double brightnessIncrease = 0.5;						// Amount to increase the red, green, and blue values of each button when it's highlighted

		readonly static Color Grey = Color.FromHex("#606060");
		readonly static Color Red = Color.FromHex("#ff0000");
		readonly static Color Blue = Color.FromHex("#3333ff");
		readonly static Color Green = Color.FromHex("#33ff33");
		readonly static Color Yellow = Color.FromHex("#ffff00");

        Grid mastergrid;
        Grid stepgrid;                                       // Grid for whole screen
        Grid sidebar;						                 // Grid for sidebar
        ScrollView scroller;                                 // ScrollView that will be used to scroll through stepgrid
		HashSet<SongPlayer.Note>[] noteList;                 // Array of HashSets of Songplayer notes
		Button[,] buttonArray;								 // Array of the buttons to make it easy to light them up 
        Dictionary<Color, SongPlayer.Instrument> colorMap;   // Dictionary mapping colors to instrument

        Color sideBarColor = Red;
        Color sideBorderColor = Color.Black;
		List<Button> buttonInUse = new List<Button>();

		Color highLightedGrey;           //Could static be a problem??
		Color highLightedRed;
		Color highLightedBlue;
		Color highLightedGreen;
		Color highLightedYellow;



		//Create variable and method for double-tapping
		//When a user double-taps a colored button on the grid,
		//an alert will pop up that shows which sounds are being used



		SongPlayer player;
        object highlightingSyncObject = new object();

        public MainPage()
        {
            InitializeComponent();

            // Initializing the song player and noteArray
			noteList = new HashSet<SongPlayer.Note>[NumColumns];    //stored this way because C# is row-major and we want to access a column at a time
			for (int i = 0; i < NumColumns; i++)
			{
				noteList[i] = new HashSet<SongPlayer.Note>();
			}

			player = new SongPlayer(noteList);


            // Initializing the colorMap
            colorMap = new Dictionary<Color, SongPlayer.Instrument>();

            colorMap[Red] = player.LoadInstrument("Snare");
            colorMap[Blue] = player.LoadInstrument("YRM1x Atmosphere");
            colorMap[Green] = player.LoadInstrument("Bass Drum");
            colorMap[Yellow] = player.LoadInstrument("Hi-Hat");

			// Initialize color of highlighted buttons
			highLightedGrey = HighLightedVersion(Grey);
			highLightedRed = HighLightedVersion(Red);
			highLightedBlue = HighLightedVersion(Blue);
			highLightedGreen = HighLightedVersion(Green);
			highLightedYellow = HighLightedVersion(Yellow);

			// Initaialize the array of buttons
			buttonArray = new Button[NumColumns, NumRows];			//stored this way because C# is row-major and we want to access a column at a time

            BackgroundColor = Color.FromHex("#000000");     // Make background color black

            //Set up a master grid with 2 columns to eventually place stepgrid and sidebar in.
            mastergrid = new Grid { ColumnSpacing = 2};
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            //Set up grid of note squares
            stepgrid = new Grid { ColumnSpacing = 2, RowSpacing = 2 };

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
					Grid colorGrid = new Grid { ColumnSpacing = 1, RowSpacing = 1, BackgroundColor = Color.Black };		// Make a grid
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
					buttonInUse.Add(button);                         //Button now in use
				}
			}

            Button playStopButton = new Button
            {
                BackgroundColor = Color.Black,
                Font = Font.SystemFontOfSize(40),
                BorderRadius = 0,
            };
            playStopButton.Text = "▶";
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
			//player.BeatStarted += HighlightColumns;
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
                ((Button)sender).Text = "▶";
				/*
                System.Threading.Monitor.Enter(highlightingSyncObject);
                foreach (Button button in buttonArray)
                {
                    DehighlightButton(button);
                }
                System.Threading.Monitor.Exit(highlightingSyncObject);
                */
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
		void OnColorGridTapped(object boxview)
		{
			Grid grid = (Grid) (((BoxView) boxview).Parent);
			HashSet<Color> colors = ChangeColor(grid);      // Changes UI represenation and returns colors that new button has

			if (!colors.Contains(sideBarColor))     // If sidebar color isn't part of button's new set of colors, remove it
			{
				SongPlayer.Note toRemove = colorMap[sideBarColor].AtPitch((NumRows - 1) - Grid.GetRow(grid));
				lock (noteList)
				{
					noteList[Grid.GetColumn(grid)].Remove(toRemove);
				}
			}
			else
			{
				SongPlayer.Note toAdd = colorMap[sideBarColor].AtPitch((NumRows - 1) - Grid.GetRow(grid));
				lock (noteList)
				{
					noteList[Grid.GetColumn(grid)].Add(toAdd); // Puts the instrument/pitch combo for this button into noteArray
				}
			}

		}

		/// <summary>
		/// Correctly changes UI representation of a note when clicked and returns list of colors in new note
		/// </summary>
		/// <param name="grid">Grid.</param>
		HashSet<Color> ChangeColor(Grid grid)
		{
			HashSet<Color> colorList = new HashSet<Color>();		// Array to store the colors to be added to the new button

			BoxView topLeft = (BoxView)grid.Children.ElementAt(0);	// Gets the top left box
			BoxView topRight = (BoxView)grid.Children.ElementAt(1);     // Might be 2, gets top right box
			BoxView bottomRight = (BoxView)grid.Children.ElementAt(3);	// gets bottom right box
			BoxView bottomLeft = (BoxView)grid.Children.ElementAt(2);   // Might be 1, gets bottom left box

			foreach (BoxView box in grid.Children)
			{
				if (!(box.BackgroundColor.Equals(Grey)))		// If the box isn't grey
				{
					colorList.Add(box.BackgroundColor);	// Add the box's color to the array

					box.BackgroundColor = Grey;             // Default the box's color to grey
					Grid.SetRowSpan(box, 1);				// Default the box to span one row
					Grid.SetColumnSpan(box, 1);				// Default the box to span one column
				}
			}


			if (colorList.Contains(sideBarColor))			// If button already has the sidebar color, remove it from colorList
			{
				colorList.Remove(sideBarColor);
			}
			else    										// If button doesn't already have the sidebar color, add it to colorList
			{
				colorList.Add(sideBarColor);
			}


			if (colorList.Count == 0)		// If the box is reverting to grey
			{
				Grid.SetRowSpan(topLeft, 2);
				Grid.SetColumnSpan(topLeft, 2);     // Make one box take up the whole space
				 
				topRight.IsVisible = true;
				bottomRight.IsVisible = true;		// Make all boxes visible
				bottomLeft.IsVisible = true;
			}

			else if (colorList.Count == 1)		// If box will have one color
			{
				//TODO: FIX THIS MOFO. It's not always the sideways color
				if (colorList.Contains(sideBarColor))
				{
					topLeft.BackgroundColor = sideBarColor;     // If we're not removing a color
				}
				else
				{
					topLeft.BackgroundColor = colorList.First();	// If we're removing a color, use the only other one
				}
				Grid.SetRowSpan(topLeft, 2);
				Grid.SetColumnSpan(topLeft, 2);     // Make one button take up the whole space

				topRight.IsVisible = false;
				bottomRight.IsVisible = false;		// Make other boxes invisible
				bottomLeft.IsVisible = false;
			}

			else if (colorList.Count == 2)	// If box will have two colors
			{

				topRight.IsVisible = true;
				bottomRight.IsVisible = false;		// Make topRight visible and bottom invisible
				bottomLeft.IsVisible = false;

				foreach (Color color in colorList)		// Assign the correct colors
				{
					if (topLeft.BackgroundColor.Equals(Grey))		// If top left is grey
					{
						topLeft.BackgroundColor = color;
					}
					else
					{
						topRight.BackgroundColor = color;
					}
				}

				Grid.SetRowSpan(topLeft, 2);		// Make topLeft take up half of grid
				Grid.SetRowSpan(topRight, 2);		// Make topRight take up half of grid
			}

			else if (colorList.Count == 3)	// If box will have three colors
			{
				topRight.IsVisible = true;
				bottomRight.IsVisible = true;	//Make topright and bottomright visible, make bottom left invisible
				bottomLeft.IsVisible = false;

				foreach (Color color in colorList)      // Assign the correct colors
				{
					if (topLeft.BackgroundColor.Equals(Grey))    // If top left is grey
					{
						topLeft.BackgroundColor = color;
					}
					else if (topRight.BackgroundColor.Equals(Grey))    // If top right is grey
					{
						topRight.BackgroundColor = color;
					}
					else
					{
						bottomRight.BackgroundColor = color;
					}
				}

				Grid.SetRowSpan(topLeft, 2);	// Make topLeft up take up half the grid; other two split the remaining space
			}

			else                    // If box will have four colors
			{
				topRight.IsVisible = true;
				topLeft.IsVisible = true;
				bottomRight.IsVisible = true;		// Make all boxes visible
				bottomLeft.IsVisible = true;

				// Assign the correct colors
				foreach (Color color in colorList)      
				{
					if (topLeft.BackgroundColor.Equals(Grey))    // If top left is grey
					{
						topLeft.BackgroundColor = color;
					}
					else if (topRight.BackgroundColor.Equals(Grey))    // If top right is grey
					{
						topRight.BackgroundColor = color;
					}
					else if (bottomLeft.BackgroundColor.Equals(Grey))	// If bottom left is grey
					{
						bottomLeft.BackgroundColor = color;
					}
					else
					{
						bottomRight.BackgroundColor = color;
					}
				}

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
			sideBorderColor = button.BorderColor;

			if (button.BorderColor.Equals(Color.FromHex("#ffff00")))
			{
				return;													//do nothing
			}

			if (button.BorderColor.Equals(Color.Black) && buttonInUse.Count == 0) //if button clicked has black border and no button in use yet
			{
				button.BorderColor = Color.FromHex("#ffff00"); //Change border highlight to yellow
				buttonInUse.Add(button);
			}

			else if (button.BorderColor.Equals(Color.Black) && buttonInUse.Count > 0)
			{
				button.BorderColor = Color.FromHex("#ffff00");   //Change border highlight to yellow
				buttonInUse[0].BorderColor = Color.Black;  //The button that WAS in use will now have a black border
				buttonInUse.Clear();							 //Clear the USED button from the lost
				buttonInUse.Add(button);						 //Add this CURRENTLY USED button to list

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
                int previousBeat;

                if (currentBeat == 0)
                {
                    previousBeat = NumColumns - 1;
                }
                else
                {
                    previousBeat = currentBeat - 1;
                }

                if (!firstBeat)
                {
                    // De-highlight the previous column of buttons
                    for (int i = 0; i < NumRows; i++)
                    {
                        DehighlightButton(buttonArray[previousBeat, i]);
                    }
                }

                // Highlight the next column of buttons
                for (int i = 0; i < NumRows; i++)
                {
                    Color nextColor = buttonArray[currentBeat, i].BackgroundColor;

                    buttonArray[currentBeat, i].BackgroundColor = HighLightedVersion(nextColor); //Set the new background color of the button	
                }

                System.Threading.Monitor.Exit(highlightingSyncObject);
            });

		}

        void DehighlightButton(Button button)
        {
            Color previousColor = button.BackgroundColor;

            if (previousColor.Equals(highLightedRed))
            {
                previousColor = Red;
            }
            else if (previousColor.Equals(highLightedBlue))
            {
                previousColor = Blue;
            }
            else if (previousColor.Equals(highLightedGreen))
            {
                previousColor = Green;
            }
            else if (previousColor.Equals(highLightedYellow))
            {
                previousColor = Yellow;
            }
            else if (previousColor.Equals(highLightedGrey))
            {
                previousColor = Grey;
            }
            else
            {
                //Button is not highlighted, return;
                return;
            }

            button.BackgroundColor = previousColor;  // Set the new background color of the button
        }


        /// <summary>
        /// Returns the highlighted version of a color
        /// </summary>
        /// <returns>The lighted version.</returns>
        /// <param name="c">C.</param>
        Color HighLightedVersion(Color c)
		{
			double red = c.R;
			double green = c.G;
			double blue = c.B;

			red += brightnessIncrease;
			green += brightnessIncrease;
			blue += brightnessIncrease;

			return Color.FromRgb(red, green, blue);
		}
	}
}
