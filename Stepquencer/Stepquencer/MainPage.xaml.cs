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

			//Set up grid of StepSquares

			var stepgrid = new Grid();


			//Initialize the number of rows and columns
			for (int i = 0; i < NumRows; i++)
			{
				stepgrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			}

			for (int i = 0; i < NumColumns; i++)
			{
				stepgrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			}

			//Add the buttons to it
			for (int i = 0; i < NumRows * NumColumns; i++)
			{
				stepgrid.Children.Add(new Button { Text = "WOOP" });
			}
		}
	}
}
