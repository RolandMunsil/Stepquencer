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


			BackgroundColor = Color.FromHex("#000000");


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
					var button = new Button { Style = greyButton };
					//stepgrid.Children.Add(new Button { Style = greyButton}, j, i);
					stepgrid.Children.Add(button, j, i);
					button.Clicked += OnButtonClicked;

				}
			};

			//

			//

			//do something with plainButtons


			//var label = new Label { Text = "This should show up with music", TextColor = Color.FromHex("#77fd65"), FontSize = 20 };

			Content = stepgrid;
		}

		//fix this

		void OnButtonClicked(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}