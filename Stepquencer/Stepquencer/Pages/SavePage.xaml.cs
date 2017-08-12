using System;
using System.IO;
using Xamarin.Forms;

namespace Stepquencer
{
    public partial class SavePage : ContentPage
    {
        private MainPage mainpage;                                          // The MainPage this screen came from
        private Entry songTitleEntry;                                       // Input Box for name of user's song
        private Button saveButton, cancelButton;    // Buttons to let user save a new song, overwrite their current one, or go back

        private int fontSize = App.isTablet ? 25 : 15;                  // Sets default font size based on whether device is tablet or phone
        public SavePage(MainPage mainpage)
        {
            this.mainpage = mainpage;                           // Need to pass in main page in order to change it on this page
            this.BackgroundColor = Color.FromHex("#2C2C2C");    // Set the background color of the page
            NavigationPage.SetHasNavigationBar(this, false);    // Make sure navigation bar doesn't show up on this one

            // Initialize scrollView
            ScrollView scroller = new ScrollView();

            // Initialize masterLayout
            StackLayout masterLayout = new StackLayout { Spacing = 30 };

            // Initialize saveLabel
            Label saveLabel = new Label
            {
                Text = "Song Title:",
                TextColor = Color.White,
                BackgroundColor = Color.FromHex("#2C2C2C"),
                FontSize = fontSize * 2,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };

            // Initialize text entry box
            songTitleEntry = new Entry
            {
                Placeholder = "Enter song title",
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = 300
            };
            songTitleEntry.TextChanged += (sender, e) => OnTextChanged();

            // Initialize buttonLayout
            StackLayout buttonLayout = new StackLayout();
            buttonLayout.Orientation = StackOrientation.Horizontal;
            buttonLayout.VerticalOptions = LayoutOptions.CenterAndExpand;

            // Initialize save and cancel buttons
            saveButton = new Button
            {
                Text = "SAVE",
                TextColor = Color.Gray,
                FontSize = fontSize,
                BackgroundColor = Color.Black,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            cancelButton = new Button
            {
                Text = "CANCEL",
                TextColor = Color.White,
                FontSize = fontSize,
                BackgroundColor = Color.Black,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            // Initialize event handlers for buttons
            saveButton.Clicked += OnSaveButtonClicked;
            cancelButton.Clicked += OnCancelButtonClicked;

            // Add buttons to buttonLayout
            buttonLayout.Children.Add(cancelButton);
            buttonLayout.Children.Add(saveButton);

            // Add label, entry, buttonLayout to masterLayout
            masterLayout.Children.Add(saveLabel);
            masterLayout.Children.Add(songTitleEntry);
            masterLayout.Children.Add(buttonLayout);

            scroller.Content = masterLayout;
            Content = scroller;
        }

        /// <summary>
        /// Event handler for save button
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnSaveButtonClicked(Object sender, EventArgs e)
        {
            if (songTitleEntry.Text == null || songTitleEntry.Text == "")
            {
                //User hasn't entered a name and the button is gray so do nothing
                return;
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
			if (songTitleEntry.Text.IndexOfAny(invalidChars) >= 0)        // Invalid song name
			{
				await DisplayAlert("Invalid filename!",
					$"Filename cannot contain any of the following characters: {String.Join("", invalidChars)}",
					 "OK");
                return;
			}
			else if (File.Exists(FileUtilities.PathToSongFile(songTitleEntry.Text)))    // Trying to save to a song that already exists
			{
				//DisplayAlert returns boolean value
				bool answer = await DisplayAlert("Overwrite Warning", "A song with this name already exists. Do you want to overwrite it?", "Overwrite", "Cancel");
				//If user presses "OK"
				if (answer)
				{
					//Delete old song first
					File.Delete(FileUtilities.PathToSongFile(songTitleEntry.Text));
				}
                else
                {
                    return;
                }
			}

            //Add new song
            FileUtilities.SaveSongToFile(mainpage.song, songTitleEntry.Text);

            //Make sure main page knows current state is saved
            mainpage.loadedSongChanged = false;
            mainpage.lastLoadedSongName = songTitleEntry.Text;

            //Go back to main grid
            await Navigation.PopToRootAsync();
        }

        /// <summary>
        /// Event handler for cancel button
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnCancelButtonClicked(Object sender, EventArgs e)
        {
			await Navigation.PopToRootAsync();     // Send user back to mainpage.
		}


        /// <summary>
        /// Ensures that the cancel button is grayed out if no song name has been entered
        /// </summary>
        private void OnTextChanged()
        {
            if (songTitleEntry.Text.Equals(""))
            {
                saveButton.TextColor = Color.Gray;
            }
            else
            {
                saveButton.TextColor = Color.White;
            }
        }
    }
}
