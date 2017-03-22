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
		HashSet<SongPlayer.Note>[] noteList;                 // Array of HashSets of Songplayer notes
		Button[,] buttonArray;								 // Array of the buttons to make it easy to light them up 
        Dictionary<Color, SongPlayer.Instrument> colorMap;   // Dictionary mapping colors to instrument

        Color sideBarColor = Red;
        Color sideBorderColor = Color.Black;
		List<Button> buttonInUse = new List<Button>();

        BoxView highlight;

		//Create variable and method for double-tapping
		//When a user double-taps a colored button on the grid,
		//an alert will pop up that shows which sounds are being used



		SongPlayer player;
        object highlightingSyncObject = new object();

        public MainPage()
        {
            InitializeComponent();

            highlight = new BoxView() { Color = Color.White, Opacity = brightnessIncrease };
            highlight.InputTransparent = true;

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

			// Initaialize the array of buttons
			buttonArray = new Button[NumColumns, NumRows];			//stored this way because C# is row-major and we want to access a column at a time

            BackgroundColor = Color.FromHex("#000000");     // Make background color black

            Style greyButton = new Style(typeof(Button))    // Button style for testing grid
            {
                Setters =
                {
                    new Setter { Property = Button.BackgroundColorProperty, Value = Grey },
                    new Setter { Property = Button.BorderRadiusProperty, Value = 0 },
                }
            };

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


            //Add the note buttons to the grid
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    Button button = new Button { Style = greyButton };  // Make a new button
                    stepgrid.Children.Add(button, j, i);                // Add it to the grid
                    button.Clicked += OnButtonClicked;                  // Add it to stepsquare event handler
					buttonArray[j, i] = button;     					// Add button to buttonArray
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
		void OnButtonClicked(object sender, EventArgs e)
		{
			Button button = (Button)sender;

			if (button.BackgroundColor.Equals(Grey) && buttonInUse.Count > 0)						// If the button is unhighlighted
			{
				button.BackgroundColor = sideBarColor;
				SongPlayer.Note toAdd = colorMap[sideBarColor].AtPitch((NumRows - 1) - Grid.GetRow(button));
				lock (noteList)
				{
					noteList[Grid.GetColumn(button)].Add(toAdd); // Puts the instrument/pitch combo for this button into noteArray
				}
			}

			//TODO: Handle multiple colors
			//If grid button already has a color BUT the highlighted sidebar color is not the same as selected square
			//Add instrument on same button
			//else if (!(button.BackgroundColor.Equals(highLightedGrey)) & !(button.BackgroundColor.Equals(sideBarColor)))
			//        //Add new instrument--button now has >1 sounds
			//        //Make the button gradient
			//
			//
			//
			//
			// 



			else 
			{
				//TODO: this fails when the user clicks on a highlighted button.
				SongPlayer.Note toRemove = colorMap[button.BackgroundColor].AtPitch((NumRows - 1) - Grid.GetRow(button));
				button.BackgroundColor = Grey;
					
				lock (noteList)
				{
					noteList[Grid.GetColumn(button)].Remove(toRemove);
				}

			}
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
