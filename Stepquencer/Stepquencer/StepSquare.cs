using System;
using Xamarin.Forms;

namespace Stepquencer
{
	/// <summary>
	/// The type of square that is part of the grid. When it is
	/// tapped, it will play the appropriate sound when the stepquencer
	/// scans over it.
	/// </summary>
	public class StepSquare
	{
		private double SQUARE_WIDTH = 20.0;
		private double SQUARE_LENGTH = 20.0;
		private double x;
		private double y;
		private Color c;

		public double SquareWidth
		{
			get
			{
				return SQUARE_WIDTH;
			}
		}

		public double SquareLength
		{
			get
			{
				return SQUARE_LENGTH;
			}
		}

		public StepSquare(double x, double y, Color c)
		{
			this.x = x;
			this.y = y;
			this.c = c;
		}
			
		}
}

//setter?
//myVar.SquareWidth += 1
