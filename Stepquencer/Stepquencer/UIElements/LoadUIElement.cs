using System;
using Xamarin.Forms;
namespace Stepquencer
{

    /// <summary>
    /// Encapsulation of a single song that a user can load or delete
    /// </summary>
    public class LoadUIElement : StackLayout
    {
        public readonly String filePath;                // Path to the song file this UI element references

        public event Action<LoadUIElement> DeleteClicked;   // Allows other classes to decide what happens
        public event Action<LoadUIElement> ShareClicked;    //
        public event Action<LoadUIElement> Tap;             // when this is tapped, share is tapped, or delete is tapped

        public LoadUIElement(String filePath) : base()
        {
            this.filePath = filePath;

            this.Orientation = StackOrientation.Horizontal; //
            this.BackgroundColor = Color.Black;             // Stylistic choices
            this.HeightRequest = 50;                        //


            // Make a new label initialized with the song name

            Label songLabel = new Label
            {
                Text = FileUtilities.SongNameFromFilePath(filePath),// Make sure it displays the name of song
                FontSize = 20,                                      //*
                BackgroundColor = Color.Black,                      //*
                TextColor = Color.White,                            //* Stylistic choices
                HorizontalTextAlignment = TextAlignment.Start,      //*  
                HorizontalOptions = LayoutOptions.StartAndExpand,   //*
                VerticalOptions = LayoutOptions.CenterAndExpand,    //*
                InputTransparent = true,                            // Ensures the user can't tap on label instead of main body of this object
                Margin = new Thickness(17.0, 0.0)

            };


			// Make a button that lets user delete this object's song
			Button deleteButton = new Button
			{
				Text = "DELETE",
				TextColor = Color.Red,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.End,
				Margin = 7
			};

			// Make a button that prompts user to share this song
			Button shareButton = new Button
			{
				Text = "SHARE",
				TextColor = Color.Blue,
				VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.End,
				Margin = 22
			};

			this.Children.Add(songLabel);       // 
            this.Children.Add(shareButton);     // Add visual elements to this object 
			this.Children.Add(deleteButton);    //


			// Ensure that gestures on the delete button, share button and rest of object are handled by public events

			deleteButton.Clicked += (s, e) => { DeleteClicked.Invoke(this); };
            shareButton.Clicked += (s, e) => { ShareClicked.Invoke(this); };


            TapGestureRecognizer tgr = new TapGestureRecognizer()
            {
                Command = new Command(()=> { Tap.Invoke(this);})
            };
            this.GestureRecognizers.Add(tgr);
        }
    }
}
