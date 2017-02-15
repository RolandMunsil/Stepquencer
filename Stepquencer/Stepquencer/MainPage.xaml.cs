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
		const int NumColumns = 8;  

		public MainPage()
		{
			InitializeComponent();

			SongPlayer.Note[,] songArray = new SongPlayer.Note[NumColumns, NumRows];	// Array of StepSquare data for SongPlayer, stored this way because C# is row-major

			BackgroundColor = Color.FromHex("#000000");		// Make background color black


			Style greyButton = new Style(typeof(Button))	// Button style for testing grid


			//Style plainButton = new Style(typeof(Button))

			{
				Setters = 
				{
	 				new Setter { Property = Button.BackgroundColorProperty, Value = Color.FromHex ("#606060") },
	  				new Setter { Property = Button.TextColorProperty, Value = Color.Black },
	  				new Setter { Property = Button.BorderRadiusProperty, Value = 0 },
	 				new Setter { Property = Button.FontSizeProperty, Value = 40 }
	            }
			};


			//Set up grid of StepSquares

			Grid stepgrid = new Grid { ColumnSpacing = 2, RowSpacing = 2};



			//Initialize the number of rows and columns
			for (int i = 0; i < NumRows; i++)
			{
				stepgrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			}

			for (int i = 0; i < NumColumns; i++)
			{
				stepgrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			}

			//Add the buttons to it
			for (int i = 0; i < NumRows; i++)
			{
				for (int j = 0; j < NumColumns; j++)
				{
					var button = new Button { Style = greyButton };		// Make a new button
					stepgrid.Children.Add(button, j, i);				// Add it to the grid
					button.Clicked += OnButtonClicked;					// C# event handling

					songArray[j, i] = SongPlayer.Note.None;		// Add a placeholder to songArray

				}
			};

			//

			//

			//do something with plainButtons


			//var label = new Label { Text = "This should show up with music", TextColor = Color.FromHex("#77fd65"), FontSize = 20 };

			Content = stepgrid;


            SongPlayer player = new SongPlayer();
            SongPlayer.Note[] snareNotes = player.LoadInstrument("Snare");
            SongPlayer.Note[] hihatNotes = player.LoadInstrument("Hi-Hat");
            SongPlayer.Note[] bdrumNotes = player.LoadInstrument("Bass Drum");
            SongPlayer.Note[] atmosNotes = player.LoadInstrument("YRM1x Atmosphere");

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

		//fix this

		void OnButtonClicked(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}