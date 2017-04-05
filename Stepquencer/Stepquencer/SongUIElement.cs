using System;
using Xamarin.Forms;
using System.IO;
namespace Stepquencer
{
    public class SongUIElement : StackLayout
    {
        private String documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        private String filePath;
        private String songName;
        private Song song;
        private StackLayout songUILayout;
        private Label songLabel;
        private Button deleteButton;
        private Image folder;

        public SongUIElement(String filePath) : base()
        {
            this.filePath = filePath;
            this.songName = GetSongNameFromFilePath(filePath);

            this.Orientation = StackOrientation.Horizontal;
            this.BackgroundColor = Color.Black;



            this.songLabel = new Label             // Make a new label initialized with the song name
            {
                Text = songName,
                BackgroundColor = Color.Black,
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };


            this.deleteButton = new Button      // Make a button that lets you delete a file
            {
                Text = "DELETE",
                TextColor = Color.Red       //TODO: replace with trashcan image

            };

            deleteButton.Clicked += (sender, e) =>      // Add event handler to button that will cause it to delete the current file and remove this object from LoadPage
            {
                File.Delete(this.filePath);
                //TODO: Make a way to remove this object from LoadPage
            };


            this.Children.Add(songLabel);
            this.Children.Add(deleteButton);


            // Generate songObject

            this.song = LoadPage.LoadSongFromFile(filePath);

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
        /// Getter for this object's Song
        /// </summary>
        /// <returns>The song.</returns>
        public Song GetSong()
        {
            return this.song;
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
