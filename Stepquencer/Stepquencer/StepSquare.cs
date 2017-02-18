using System;
using Xamarin.Forms;
namespace Stepquencer
{
	public class StepSquare : Button
	{
		int row;
		int column;

		public StepSquare(int row, int column)
		{
			this.row = row;
			this.column = column;

		}
	}
}
