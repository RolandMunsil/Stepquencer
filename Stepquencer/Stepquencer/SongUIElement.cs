using System;
using Xamarin.Forms;
using System.IO;
namespace Stepquencer
{

    /// <summary>
    /// Encapsulation of a single song that a user can load or delete
    /// </summary>
    public class SongUIElement : StackLayout
    {
        public String filePath;

        public event Action<SongUIElement> DeleteClicked;
        public event Action<SongUIElement> Tap;

        public SongUIElement(String filePath) : base()
        {
            this.filePath = filePath;

            this.Orientation = StackOrientation.Horizontal;
            this.BackgroundColor = Color.Black;
            this.HeightRequest = 50;

            Label songLabel = new Label             // Make a new label initialized with the song name
            {
                Text = SongFileUtilities.SongNameFromFilePath(filePath),
                FontSize = 20,
                BackgroundColor = Color.Black,
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                InputTransparent = true
            };

            Button deleteButton = new Button      // Make a button that lets you delete a file
            {
                Text = "DELETE",
                TextColor = Color.Red,       //TODO: replace with trashcan image
                Margin = 7
            };
            this.Children.Add(songLabel);
            this.Children.Add(deleteButton);


            deleteButton.Clicked += (s, e) => { DeleteClicked.Invoke(this); };

            TapGestureRecognizer tgr = new TapGestureRecognizer()
            {
                Command = new Command(()=> { Tap.Invoke(this);})
            };

            this.GestureRecognizers.Add(tgr);
        }
    }
}
