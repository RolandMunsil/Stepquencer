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
        const int NumRows = 21;
        const int NumColumns = 16;
        const int NumInstruments = 4;


		readonly static Color Grey = Color.FromHex("#606060");
		readonly static Color Red = Color.FromHex("#ff0000");
		readonly static Color Blue = Color.FromHex("#3333ff");
		readonly static Color Green = Color.FromHex("#33ff33");
		readonly static Color Yellow = Color.FromHex("#ffff00");


        static Grid mastergrid;
        static Grid stepgrid;                                       // Grid for whole screen
        static Grid sidebar;						// Grid for sidebar
        static ScrollView scroller;                                 // ScrollView that will be used to scroll through stepgrid
        static SongPlayer.Note[,] noteArray;                        // Array of StepSquare data for SongPlayer, stored this way because C# is row-major
        static Dictionary<Color, SongPlayer.Note[]> colorMap;       // Dictionary mapping colors to instruments
        static Color SideBarColor = Red;
        static Color SideBorderColor = Color.Black;


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
                     new Setter { Property = Button.BackgroundColorProperty, Value = Grey },
                      new Setter { Property = Button.BorderRadiusProperty, Value = 0 },
                }
            };

            //Set up a master grid with 2 columns to eventually place stepgrid and sidebar in.

            mastergrid = new Grid { ColumnSpacing = 2};
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });
            mastergrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

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
            {
                for (int i = 0; i < NumRows; i++)
                    for (int j = 0; j < NumColumns; j++)
                    {

                        Button button = new Button { Style = greyButton };  // Make a new button
                        stepgrid.Children.Add(button, j, i);                // Add it to the grid
                        button.Clicked += OnButtonClicked;                  // Add it to stepsquare event handler
                        noteArray[j, i] = SongPlayer.Note.None;             // Add a placeholder to songArray

                    }
            };



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
                Orientation = ScrollOrientation.Both  //Both vertical and horizontal orientation
            };
           
            scroller.Content = stepgrid;

            // Add the scroller (which contains stepgrid) and sidebar to mastergrid

            mastergrid.Children.Add(scroller, 0, 0); // Add scroller to first column of mastergrid
            mastergrid.Children.Add(sidebar, 1, 0);  // Add sidebar to final column of mastergrid
                                                     //Grid.SetRowSpan(sidebar, NumRows);                  // Make sure that it spans the whole column

            Content = mastergrid;



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

		/// <summary>
		/// Event handler for normal buttons in the grid
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void OnButtonClicked(object sender, EventArgs e)
		{

			Button button = (Button)sender;
			if (button.BackgroundColor.Equals(Grey))
			{
				button.BackgroundColor = SideBarColor;
				noteArray[Grid.GetColumn(button), Grid.GetRow(button)] = colorMap[SideBarColor][(NumRows - 1) - Grid.GetRow(button)]; // Puts the instrument/pitch combo for this button into noteArray

			}
			else
			{
				button.BackgroundColor = Grey;
				noteArray[Grid.GetColumn(button), Grid.GetRow(button)] = SongPlayer.Note.None;
			}


		}



		//TODO: Add to method
		//If a button on sidebar is already highlighted and another sidebar button is clicked....
		//Unhighlight the 'old' button


		/// <summary>
		/// Event handler for buttons in the sidebar
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void OnSidebarClicked(object sender, EventArgs e)
		{
			Button button = (Button)sender;

			SideBarColor = button.BackgroundColor;
			SideBorderColor = button.BorderColor;

			if (button.BorderColor.Equals(Color.Black))
			{
				button.BorderColor = Color.FromHex("#ffff00"); //Change border highlight to yellow
			}

			else
			{
				button.BorderColor = Color.Black;
			}

		}


		void HighlightColumns(int beat)
		{
			// Each time the songplayer starts playing a beat, trigger this for loop:

			for (int i = 0; i < NumRows; i++)
			{
				//stepgrid.Children.
			}
		}
	}
}