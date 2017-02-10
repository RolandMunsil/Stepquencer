using System;
using Xamarin.Forms;

namespace Stepquencer
{
	public class StepSquares
	{
		private double SQUARE_WIDTH = 20.0;
		private double SQUARE_LENGTH = 20.0;
		private double x;
		private double y;
		private Color c;

		public StepSquares(double x, double y, Color c)
		{
			this.x = x;
			this.y = y;
			this.c = c;
		}

		public double getSquareWidth() { return SQUARE_WIDTH; }
		public double getSquareLength() { return SQUARE_LENGTH; }
	}
}
