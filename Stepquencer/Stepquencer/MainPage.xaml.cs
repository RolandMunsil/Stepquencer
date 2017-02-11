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
		const int NumRows = 7;
		const int NumColumns = 16;

		public MainPage()
		{
			InitializeComponent();


			BackgroundColor = Color.FromHex("#404040");

			Style plainButton = new Style(typeof(Button))
			{
				Setters = 
				{
	  				new Setter { Property = Button.BackgroundColorProperty, Value = Color.FromHex ("#eee") },
	  				new Setter { Property = Button.TextColorProperty, Value = Color.Black },
	  				new Setter { Property = Button.BorderRadiusProperty, Value = 0 },
	  				new Setter { Property = Button.FontSizeProperty, Value = 40 }
				}
			};

			//Set up grid of StepSquares

			Grid stepgrid = new Grid();


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
					stepgrid.Children.Add(new Button { Style = plainButton}, i, j);
				}
			}

			//var label = new Label { Text = "This should show up with music", TextColor = Color.FromHex("#77fd65"), FontSize = 20 };

			Content = stepgrid;
		}
	}
}