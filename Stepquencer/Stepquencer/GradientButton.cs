using Xamarin.Forms;

//This code is from an open source project:
//https://github.com/XLabs/Xamarin-Forms-Labs/blob/master/src/Forms/XLabs.Forms/Controls/GradientContentView.cs


namespace Stepquencer
{
	public enum GradientOrientation
	{

		/// <summary>
		/// The vertical
		/// </summary>
		Vertical,
		/// <summary>
		/// The horizontal
		/// </summary>
		Horizontal
	}


	/// <summary>
	/// ContentView that allows you to have a gradient background for
	/// buttons.
	/// </summary>

	public class GradientContentView : Button
	{
		/// <summary>
		/// Start color of the gradient
		/// Defaults to White
		/// </summary>
		public GradientOrientation Orientation
		{
			get { return (GradientOrientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		/// <summary>
		/// The orientation property
		/// </summary>
		public static readonly BindableProperty OrientationProperty =
			BindableProperty.Create<GradientContentView, GradientOrientation>(x => x.Orientation, GradientOrientation.Vertical, BindingMode.OneWay);


		/// <summary>
		/// Start color of the gradient
		/// Defaults to White
		/// </summary>
		public Color StartColor
		{
			get { return (Color)GetValue(StartColorProperty); }
			set { SetValue(StartColorProperty, value); }
		}


        /// <summary>
        /// Using a BindableProperty as the backing store for StartColor. This enables animation, styling, binding, etc.
        /// </summary>
        public static readonly BindableProperty StartColorProperty =
			BindableProperty.Create<GradientContentView, Color>(x => x.StartColor, Color.White, BindingMode.OneWay);

		/// <summary>
		/// End color of the gradient
		/// Defaults to Black
		/// </summary>
		public Color EndColor
		{
			get { return (Color)GetValue(EndColorProperty); }
			set { SetValue(EndColorProperty, value); }
		}

		/// <summary>
		/// Using a BindableProperty as the backing store for EndColor.  This enables animation, styling, binding, etc...
		/// </summary>
		public static readonly BindableProperty EndColorProperty =
			BindableProperty.Create<GradientContentView, Color>(x => x.EndColor, Color.Black, BindingMode.OneWay);
	}
}





