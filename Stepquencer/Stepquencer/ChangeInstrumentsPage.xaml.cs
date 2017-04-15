using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class ChangeInstrumentsPage : ContentPage
    {

        private MainPage mainpage;                      // Reference to the mainPage instance so that currently selected Colors can be used
        private StackLayout instrumentSlotLayout;
        private HashSet<Color> selectedColors;
        private Grid allInstruments;
        InstrumentButton selectedSlot;

        public ChangeInstrumentsPage(MainPage mainpage)
        {
            this.mainpage = mainpage;
            this.BackgroundColor = Color.Black;             //* 
            this.Title = "Pick your instruments";           //* Set up basic page attributes
            NavigationPage.SetHasBackButton(this, false);   //*
            selectedColors = new HashSet<Color>();


            // Initialize masterGrid to hold all UI elements

            Grid masterGrid = new Grid();

            masterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            masterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            masterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4, GridUnitType.Star) });
            masterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });


            // Initialize a StackLayout with placeholder buttons for top row. This will be updated as user selects instruments

            instrumentSlotLayout = new StackLayout 
            { 
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 10
            };

            int j = 0;      // Used to ensure first item is selected
            foreach (InstrumentButton sideButton in mainpage.instrumentButtons)
            {
                InstrumentButton button = new InstrumentButton(sideButton.Instrument);

                button.WidthRequest = 60;           //*
                button.HeightRequest = 40;          //* Style choices

                button.Clicked += OnSlotClicked;

                instrumentSlotLayout.Children.Add(button); // Add to layout
                selectedColors.Add(button.BackgroundColor);     // Keep track of what colors are selected

                if (j == 0)                         //
                {                                   //
                    selectedSlot = button;          // Makes sure first slot is selected
                    button.Selected = true;         //
                }                                   //
                j++;                                //
            }

            masterGrid.Children.Add(instrumentSlotLayout, 0, 0);     // Add selectedButtons to masterGrid



            // Initialize a Grid for all colors user currently has and a scrollview to hold it

            allInstruments = new Grid 
            { 
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            int numInstruments = Instrument.colorMap.Keys.Count;        // First, calculate how many rows we'll need given a max of 4(?) columns
            int numRows = (int)Math.Ceiling(numInstruments / 4.0);      // NOTE: I know this is inefficient, but this was the simplest way to do it

            for (int i = 0; i < 4; i++)
            {                                                                                                                   //
                allInstruments.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });    //
            }                                                                                                                   //
            for (int i = 0; i < numRows; i++)                                                                                   // Add in rows and columns
            {                                                                                                                   //
                allInstruments.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });         //
            }                                                                                                                   //



            // Put all instruments in the allInstruments Grid

            int columnIndex = 0;
            int rowIndex = 0;

            foreach (KeyValuePair<String, Color> nameAndColor in Instrument.colorMap)
            {
                if (columnIndex > 3) 
                { 
                    columnIndex = columnIndex % 4;
                    rowIndex++;
                }

                InstrumentButton button = new InstrumentButton(Instrument.GetByName(nameAndColor.Key));
                button.Clicked += OnInstrumentClicked;

                //if (selectedColors.Contains(button.BackgroundColor)) { button.BorderColor = Color.White; }

                allInstruments.Children.Add(button, columnIndex, rowIndex);
                columnIndex++;
            }


            // Make a ScrollView to hold all of the instruments and add it to the masterGrid

            ScrollView scroller = new ScrollView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            scroller.Content = allInstruments;
            masterGrid.Children.Add(scroller, 0, 1);


            // Make a StackLayout to hold cancel, clear, and save buttons

            StackLayout userButtons = new StackLayout 
            { 
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };

            Button cancelButton = new Button
            {
                Text = "CANCEL",
                TextColor = Color.White,
                BackgroundColor = Color.Black,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            Button saveButton = new Button
            {
                Text = "SAVE",
                TextColor = Color.White,
                BackgroundColor = Color.Black,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            cancelButton.Clicked += OnCancelClicked;            // Add event listeners
            saveButton.Clicked += OnSaveClicked;                //

            userButtons.Children.Add(cancelButton);             // Add buttons to layout
            userButtons.Children.Add(saveButton);               //

            masterGrid.Children.Add(userButtons, 0, 2);

            Content = masterGrid;
        }


        /// <summary>
        /// Event handler for instrument slots up top
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void OnSlotClicked(Object sender, EventArgs e)
        {
            InstrumentButton slotClicked = (InstrumentButton)sender;
            SongPlayer.PlayNote(slotClicked.Instrument.AtPitch(0));

            if (!slotClicked.BorderColor.Equals(Color.White))
            {
                slotClicked.Selected = true;        // Slot we just clicked is highlighted
                selectedSlot.Selected = false;      // Previously highlighted slot is de-highlighted
                selectedSlot = slotClicked;         // Update selectedSlot

            }
        }


        /// <summary>
        /// Event handler for the Instrument Buttons
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void OnInstrumentClicked(Object sender, EventArgs e)
        {
            InstrumentButton clickedButton = (InstrumentButton)sender;
            SongPlayer.PlayNote(clickedButton.Instrument.AtPitch(0));

            if (!selectedColors.Contains(clickedButton.BackgroundColor))
            {
                selectedColors.Remove(selectedSlot.BackgroundColor);

                selectedSlot.Instrument = clickedButton.Instrument;
                selectedSlot.BackgroundColor = clickedButton.BackgroundColor;
                selectedSlot.Image = clickedButton.Image;

                selectedColors.Add(selectedSlot.BackgroundColor);
            }
        }


        /// <summary>
        /// Cancels the request to change instruments and sends user back to MoreOptions page
        /// </summary>
        async void OnCancelClicked(Object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }




        /// <summary>
        /// Switches the instruments used by main page then sends user back
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void OnSaveClicked(Object sender, EventArgs e)
        {
            // TODO: Warn the user about how things will change
            if (selectedColors.Count < 4)
            {
                DisplayAlert("Not Enough Instruments!", "You need to select at least 4 instruments", "Dismiss");
            }
            else
            {

                // Switch the instruments being used by the main page

                Instrument[] instruments = new Instrument[4];
                int index = 0;
                foreach (InstrumentButton button in instrumentSlotLayout.Children)
                {
                    instruments[index] = button.Instrument;
                    index++;
                }

                //Figure out which instruments have changed
                List<Instrument> oldInstruments = new List<Instrument>();
                List<Instrument> newInstruments = new List<Instrument>();
                for(int i = 0; i < instruments.Length; i++)
                {
                    Instrument oldInstr = mainpage.instrumentButtons[i].Instrument;
                    Instrument newInstr = instruments[i];
                    if (oldInstr != newInstr)
                    {
                        oldInstruments.Add(oldInstr);
                        newInstruments.Add(newInstr);
                    }
                }

                Device.BeginInvokeOnMainThread( delegate 
                {
                    this.mainpage.SetSidebarInstruments(instruments);
                    mainpage.song.ReplaceInstruments(oldInstruments, newInstruments);
                    mainpage.SetSong(mainpage.song);
                });

                returnToMainPage();                             // Send user back to main page
            }
        }


        /// <summary>
        /// Returns to main page.
        /// </summary>
        async void returnToMainPage()
        {
            await Navigation.PopToRootAsync();
        }

    }
}
