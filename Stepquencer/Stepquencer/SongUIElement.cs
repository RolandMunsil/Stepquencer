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
        private LoadPage loadpage;
        private String songName;
        private Song song;
        private Label songLabel;
        private Button deleteButton;
        private Image folder;

        public SongUIElement(String filePath, LoadPage loadpage) : base()
        {
            this.filePath = filePath;
            this.songName = GetSongNameFromFilePath(filePath);
            this.loadpage = loadpage;

            this.Orientation = StackOrientation.Horizontal;
            this.BackgroundColor = Color.Black;
            this.HeightRequest = 50;


            this.songLabel = new Label             // Make a new label initialized with the song name
            {
                Text = songName,
                FontSize = 20,
                BackgroundColor = Color.Black,
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };


            this.deleteButton = new Button      // Make a button that lets you delete a file
            {
                Text = "DELETE",
                TextColor = Color.Red,       //TODO: replace with trashcan image
                Margin = 7
            };

            deleteButton.Clicked += (sender, e) =>      // Event handler for delete buttons
            {
                File.Delete(this.filePath);             // Delete song file
                this.loadpage.loadSongUIElements();     // Refresh the page
            };


            this.Children.Add(songLabel);
            this.Children.Add(deleteButton);
        }

        /// <summary>
        /// Given a file path, returns the name of the actual song
        /// </summary>
        /// <returns>The song name from file path.</returns>
        /// <param name="path">Path.</param>
        private String GetSongNameFromFilePath(String path)
        {
            int start = path.IndexOf("stepsongs") + "stepsongs".Length + 1;
            int end = path.LastIndexOf('.');
            return path.Substring(start, end - start);
        }

        /// <summary>
        /// Adds a gesture recognizer to the label and layout
        /// </summary>
        /// <param name="tgr">Tgr.</param>
        public void AddGestureRecognizers(TapGestureRecognizer tgr)
        {
            this.songLabel.GestureRecognizers.Add(tgr);
            this.GestureRecognizers.Add(tgr);
        }
    }
}
