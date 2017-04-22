﻿using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Stepquencer
{
    /// <summary>
    /// A page that allows users to change the set of instruments they're using
    /// </summary>

    public partial class ChangeInstrumentsPage : ContentPage
    {
        private const int instrumentsPerRow = 4;

        private MainPage mainpage;                        // Reference to the mainPage instance in order to access currently selected instruments
        private HashSet<Instrument> selectedInstruments;  // Holds all currently selected instruments  
        private StackLayout instrumentSlotLayout;         //* Layout for various UI elements  
        private Grid allInstruments;                      //* 
        private InstrumentButton selectedSlot;            // Currently selected instrument  

        public ChangeInstrumentsPage(MainPage mainpage)
        {
            this.mainpage = mainpage;
            this.BackgroundColor = Color.Black;                 //* 
            this.Title = "Pick your instruments";               //* Set up basic page attributes
            NavigationPage.SetHasBackButton(this, false);       //*
            selectedInstruments = new HashSet<Instrument>();    //*


            // Initialize masterGrid to hold all UI elements

            Grid masterGrid = new Grid();

            masterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            masterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            masterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4, GridUnitType.Star) });
            masterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });


            // Initialize a StackLayout to hold currently selected instruments

            instrumentSlotLayout = new StackLayout 
            { 
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                Margin = 10,
                Spacing = 10
            };


            // Make sure instrumentSlotLayout is initialized with instruments used on main page

            foreach (InstrumentButton sideButton in mainpage.instrumentButtons)
            {
                InstrumentButton button = new InstrumentButton(sideButton.Instrument);

                button.WidthRequest = 60;           //*
                button.HeightRequest = 60;          //* Style choices

                button.Clicked += OnSlotClicked;    // Add event listener

                instrumentSlotLayout.Children.Add(button);      // Add to layout
                selectedInstruments.Add(button.Instrument);     // Keep track of what colors are selected

                if (sideButton.Equals(mainpage.selectedInstrButton))
                {                                   //
                    selectedSlot = button;          // Makes sure previously selected slot is selected
                    button.Selected = true;         //
                }                                   //
            }

            masterGrid.Children.Add(instrumentSlotLayout, 0, 0);     // Add selectedButtons to masterGrid



            // Initialize a Grid for all available instruments

            allInstruments = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            int numInstruments = Instrument.colorMap.Keys.Count;        // First, calculate how many rows we'll need given a max of 4(?) columns
            int numRows = (int)Math.Ceiling(numInstruments / 4.0);      // NOTE: I know this is inefficient, but this was the simplest way to do it

            for (int i = 0; i < instrumentsPerRow; i++)
            {                                                                                                                   //
                allInstruments.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });    //
            }                                                                                                                   //
            for (int i = 0; i < numRows; i++)                                                                                   // Add in rows and columns
            {                                                                                                                   //
                allInstruments.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });         //
            }                                                                                                                   //



            // Put instruments in the allInstruments Grid

            int columnIndex = 0;        // Used for row/column placement calculations
            int rowIndex = 0;           //

            foreach (KeyValuePair<String, Color> nameAndColor in Instrument.colorMap)   // For each mapping of instrument name and color:
            {
                if (columnIndex > instrumentsPerRow)                    //
                {                                       //
                    columnIndex = 0;                    // If at the end of a row, reset column index and move on to the next row
                    rowIndex++;                         //
                }                                       //

                InstrumentButton button = new InstrumentButton(Instrument.GetByName(nameAndColor.Key));     // Make a new InstrumentButton initialized with this instrument
                button.HeightRequest = 60;                                                                  // Style choice
                button.BorderWidth = 0;
                button.Clicked += OnInstrumentClicked;                                                      // Add event handler to button

                allInstruments.Children.Add(button, columnIndex, rowIndex);         // Add button to Grid 
                columnIndex++;                                                      
            }


            // Make a ScrollView to hold the grid with all the instruments and add it to masterGrid

            ScrollView scroller = new ScrollView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            scroller.Content = allInstruments;
            masterGrid.Children.Add(scroller, 0, 1);


            // Make a StackLayout to hold cancel and done buttons

            StackLayout userButtons = new StackLayout 
            { 
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };


            // Make cancel, done buttons

            Button cancelButton = new Button
            {
                Text = "CANCEL",
                TextColor = Color.White,
                BackgroundColor = Color.FromHex("#252525"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            Button doneButton = new Button
            {
                Text = "DONE",
                TextColor = Color.White,
                BackgroundColor = Color.FromHex("#252525"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };


            cancelButton.Clicked += OnCancelClicked;            // Add event listeners to
            doneButton.Clicked += OnDoneClicked;                // cancel and done buttons

            userButtons.Children.Add(cancelButton);             // Add buttons to layout
            userButtons.Children.Add(doneButton);               //

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

            if (slotClicked.Selected == false)
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

            if (!selectedInstruments.Contains(clickedButton.Instrument))
            {
                selectedInstruments.Remove(selectedSlot.Instrument);        // Remove current instrument from list of instruments selected
                selectedSlot.Instrument = clickedButton.Instrument;         // Set slot to hold the new instrument
                selectedInstruments.Add(selectedSlot.Instrument);           // Update the list of colors selected
            }
        }


        /// <summary>
        /// Cancels the request to change instruments and sends user back to MoreOptions page
        /// </summary>
        async void OnCancelClicked(Object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }




        /// <summary>
        /// Switches the instruments used by main page then sends user back
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void OnDoneClicked(Object sender, EventArgs e)
        {
            // TODO: Warn the user about how things will change

            //DisplayAlert("Not Enough Instruments!", "You need to select at least 4 instruments", "Dismiss");


            // Switch the instruments being used by the main page

            Instrument[] instruments = new Instrument[MainPage.NumInstruments];                       //
            int index = 0;                                                      //
            foreach (InstrumentButton button in instrumentSlotLayout.Children)  //
            {                                                                   // Gather up current selected instruments in an array
                instruments[index] = button.Instrument;                         //
                index++;                                                        //
            }                                                                   //

            // Set up the main page for user's return and go back
            Device.BeginInvokeOnMainThread( delegate 
            {
                mainpage.ReplaceInstruments(instruments);
            });

            returnToMainPage();                             
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
