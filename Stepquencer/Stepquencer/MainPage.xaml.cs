using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

//Grid is made up of buttons
//Find a way so that when the user presses a grid, the grid square changes color

namespace Stepquencer
{
	public partial class MainPage : ContentPage
	{
		const int NumRows = 7;
		const int NumColumns = 9;
		const int NumInstruments = 4;

		readonly static Color Grey = Color.FromHex("#606060");
		readonly static Color Red = Color.FromHex("#ff0000");
		readonly static Color Blue = Color.FromHex("#3333ff");
		readonly static Color Green = Color.FromHex("#33ff33");
		readonly static Color Yellow = Color.FromHex("#ffff00");

		static Grid stepgrid;										// Grid for whole screen
		static Grid sidebar;										// Grid for sidebar
		static SongPlayer.Note[,] noteArray;        				// Array of StepSquare data for SongPlayer, stored this way because C# is row-major
		static Dictionary<Color, SongPlayer.Note[]> colorMap;       // Dictionary mapping colors to instruments
		static Color SideBarColor = Red;

		public MainPage()
		{
			InitializeComponent();

			noteArray = new SongPlayer.Note[NumColumns, NumRows];    // Initializing noteArray

			// Initializing the song player and notes

			SongPlayer player = new SongPlayer();
			SongPlayer.Note[] snareNotes = player.LoadInstrument("Snare");
			SongPlayer.Note[] hihatNotes = player.LoadInstrument("Hi-Hat");
			SongPlayer.Note[] bdrumNotes = player.LoadInstrument("Bass Drum");
			SongPlayer.Note[] atmosNotes = player.LoadInstrument("YRM1x Atmosphere");

			// Initializing the colorMap

			colorMap = new Dictionary<Color, SongPlayer.Note[]>();

			colorMap[Red] = snareNotes;
			colorMap[Blue] = atmosNotes;
			colorMap[Green] = bdrumNotes;
			colorMap[Yellow] = hihatNotes;


			BackgroundColor = Color.FromHex("#000000");     // Make background color black

			Style greyButton = new Style(typeof(Button))    // Button style for testing grid
			{
				Setters =
				{
	 				new Setter { Property = Button.BackgroundColorProperty, Value = Color.FromHex (Grey) },
	  				new Setter { Property = Button.TextColorProperty, Value = Color.Black },
	  				new Setter { Property = Button.BorderRadiusProperty, Value = 0 },
	 				new Setter { Property = Button.FontSizeProperty, Value = 40 }
				}
			};


			//Set up grid of StepSquares

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

			//Add the stepsquare buttons to it
			for (int i = 0; i < NumRows; i++)
			{
				for (int j = 0; j < NumColumns - 1; j++)
				{

					StepSquare button = new StepSquare(j, i);      // Make a new button
					stepgrid.Children.Add(button, j, i);                // Add it to the grid
					button.Clicked += OnButtonClicked;                  // C# event handling

					noteArray[j, i] = SongPlayer.Note.None;     // Add a placeholder to songArray

				}
			};


			// Make the sidebar

			sidebar = new Grid { ColumnSpacing = 1, RowSpacing = 1 };

			for (int i = 0; i < NumInstruments; i++)
			{
				sidebar.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			}

			sidebar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


			// Fill it with buttons
			//TODO: Make a fancier for loop that does this

			Button RedButton = new Button { BackgroundColor = Color.FromHex(Red) }; // Make a new button
			sidebar.Children.Add(RedButton, 0, 0);                     				// Add it to the sidebar
			RedButton.Clicked += OnSidebarClicked;                     				// C# event handling 

			Button BlueButton = new Button { BackgroundColor = Color.FromHex(Blue) }; // Make a new button
			sidebar.Children.Add(BlueButton, 0, 1);                                  // Add it to the sidebar
			BlueButton.Clicked += OnSidebarClicked;                                  // C# event handling

			Button GreenButton = new Button { BackgroundColor = Color.FromHex(Green) }; // Make a new button
			sidebar.Children.Add(GreenButton, 0, 2);                                  // Add it to the sidebar
			GreenButton.Clicked += OnSidebarClicked;                                  // C# event handling

			Button YellowButton = new Button { BackgroundColor = Color.FromHex(Yellow) }; // Make a new button
			sidebar.Children.Add(YellowButton, 0, 3);                                  // Add it to the sidebar
			YellowButton.Clicked += OnSidebarClicked;                                  // C# event handling



			// Add the sidebar to stepgrid

			stepgrid.Children.Add(sidebar, NumColumns - 1, 0);  // Add it it to the final row of stepgrid
			Grid.SetRowSpan(sidebar, NumRows);                  // Make sure that it spans the whole column



			Content = stepgrid;



			//Below is an example of how to use the data returned by LoadInstrument to construct a simple song.
			List<SongPlayer.Note>[] noteLists = new List<SongPlayer.Note>[16];

			//Add drum beat
			for (int b = 0; b < 16; b++)
			{
				List<SongPlayer.Note> notesThisTimestep = new List<SongPlayer.Note>();
				notesThisTimestep.Add(hihatNotes[0]);
				if (b % 2 == 0)
					notesThisTimestep.Add(bdrumNotes[0]);
				if (b % 4 == 2)
					notesThisTimestep.Add(snareNotes[0]);

				noteLists[b] = notesThisTimestep;
			}

			//Add 2 C scale climbs
			noteLists[0].Add(atmosNotes[0]);
			noteLists[1].Add(atmosNotes[2]);
			noteLists[2].Add(atmosNotes[4]);
			noteLists[3].Add(atmosNotes[5]);
			noteLists[4].Add(atmosNotes[7]);
			noteLists[5].Add(atmosNotes[9]);
			noteLists[6].Add(atmosNotes[11]);
			noteLists[7].Add(atmosNotes[12]);

			noteLists[8].Add(atmosNotes[0]);
			noteLists[9].Add(atmosNotes[2]);
			noteLists[10].Add(atmosNotes[4]);
			noteLists[11].Add(atmosNotes[5]);
			noteLists[12].Add(atmosNotes[7]);
			noteLists[13].Add(atmosNotes[9]);
			noteLists[14].Add(atmosNotes[11]);
			noteLists[15].Add(atmosNotes[12]);

			//Convert to array of arrays
			SongPlayer.Note[][] song = new SongPlayer.Note[16][];
			for (int i = 0; i < 16; i++)
			{
				song[i] = noteLists[i].ToArray();
			}

			//Play
			player.PlaySong(song, 240);
		}


		void OnButtonClicked(object sender, EventArgs e)
		{

			StepSquare button = (StepSquare)sender;
			if (button.BackgroundColor.Equals(Grey))
			{
				button.BackgroundColor = SideBarColor;
				noteArray[button.column, button.row] = colorMap[SideBarColor][button.column];		// Puts the instrument/pitch combo for this button into noteArray

			}
			else
			{
				button.BackgroundColor = Grey;
				noteArray[button.column, button.row] = SongPlayer.Note.None;
			}


		}

		void OnSidebarClicked(object sender, EventArgs e)
		{
			Button button = (Button)sender;

			SideBarColor = button.BackgroundColor;
		}
	}
}