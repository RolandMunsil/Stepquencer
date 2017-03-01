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

        static Grid mastergrid;
        static Grid stepgrid;                                       // Grid for whole screen
        static Grid sidebar;						                // Grid for sidebar
        static ScrollView scroller;                                 // ScrollView that will be used to scroll through stepgrid
        static SongPlayer.Note[,] noteArray;                        // Array of StepSquare data for SongPlayer
		static Button[,] buttonArray;								// Array of the buttons to make it easy to light them up 
        static Dictionary<Color, SongPlayer.Instrument> colorMap;   // Dictionary mapping colors to instruments	

        static Color sideBarColor = Red;
        static Color sideBorderColor = Color.Black;
		static List<Button> buttonInUse = new List<Button>();

		static Color highLightedGrey;           //Could static be a problem??
		static Color highLightedRed;
		static Color highLightedBlue;
		static Color highLightedGreen;
		static Color highLightedYellow;

		SongPlayer player;

        public MainPage()
        {
            InitializeComponent();

            // Initializing the song player and noteArray
            noteArray = new SongPlayer.Note[NumColumns, NumRows];	//stored this way because C# is row-major and we want to access a column at a time
            player = new SongPlayer(noteArray);

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
                    noteArray[j, i] = SongPlayer.Note.None;             // Add a placeholder to songArray
					buttonArray[j, i] = button;							// Add button to buttonArray
                }
            }


            // Make the sidebar
            sidebar = new Grid { ColumnSpacing = 1, RowSpacing = 1 };

            for (int i = 0; i < NumInstruments; i++)
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
			}


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
            player.BeginPlaying(240);
		}

		/// <summary>
		/// Event handler for normal buttons in the grid
		/// </summary>
		void OnButtonClicked(object sender, EventArgs e)
		{
            //TODO: set it up so that it starts a new thread to add note?
			Button button = (Button)sender;
			if (button.BackgroundColor.Equals(Grey) && buttonInUse.Count > 0)						// If the button is unhighlighted
			{
				button.BackgroundColor = sideBarColor;
				SongPlayer.Note toAdd = colorMap[sideBarColor].AtPitch((NumRows - 1) - Grid.GetRow(button));
				lock (noteArray)
				{
					noteArray[Grid.GetColumn(button), Grid.GetRow(button)] = toAdd; // Puts the instrument/pitch combo for this button into noteArray
				}
			}
			else if (button.BackgroundColor.Equals(highLightedGrey))		// If the button IS highlighted
			{
				button.BackgroundColor = HighLightedVersion(sideBarColor);
				SongPlayer.Note toAdd = colorMap[sideBarColor].AtPitch((NumRows - 1) - Grid.GetRow(button));
				lock (noteArray)
				{
					noteArray[Grid.GetColumn(button), Grid.GetRow(button)] = toAdd; // Puts the instrument/pitch combo for this button into noteArray
				}
			}
			else
			{
				button.BackgroundColor = Grey;
				lock (noteArray)
                {
                    noteArray[Grid.GetColumn(button), Grid.GetRow(button)] = SongPlayer.Note.None;
                }
			}
		}



		//TODO: Add to method
		//If a button on sidebar is already highlighted and another sidebar button is clicked....
		//Unhighlight the 'old' button

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

			//else   //If button clicked has yellow border //Conflicting if-else statements??
			//{
			//	button.BorderColor = Color.Black;
			//	buttonInUse.Clear();
			//}

			//else if (button.BorderColor.Equals(Color.FromHex("#ffff00"))) 
			//{
			//	button.BorderColor = Color.Black;
			//	buttonInUse.Clear();
			//}

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

				Device.BeginInvokeOnMainThread(delegate         // Ensures that the main thread does work on UI
				{
					for (int i = 0; i < NumRows; i++)
					{
						Color previousColor = buttonArray[previousBeat, i].BackgroundColor;

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
						else
						{
							previousColor = Grey;
						}

						buttonArray[previousBeat, i].BackgroundColor = previousColor;  // Set the new background color of the button
						}
				});
			}

			// Highlight the next column of buttons

			Device.BeginInvokeOnMainThread(delegate     // Ensures that the main thread does work on UI
			{

				for (int i = 0; i < NumRows; i++)
				{
					Color nextColor = buttonArray[currentBeat, i].BackgroundColor;

					buttonArray[currentBeat, i].BackgroundColor = HighLightedVersion(nextColor); //Set the new background color of the button	
				}
			});

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