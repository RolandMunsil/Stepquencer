﻿using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class MoreOptionsPage : ContentPage
    {
        const int tempoRows = 1;
        const int tempoColumns = 6;
        const double minBPM = 100;
        const double maxBPM = 480;

        private StackLayout masterLayout;
        private Grid tempoGrid;
        private Grid buttonGrid;
        private Label tempoLabel;
        private Slider tempoSlider;
        private Button saveButton, loadButton, clearAllButton, undoClearButton;
        private MainPage mainpage;
        private Song song;


        public MoreOptionsPage(MainPage passedpage, Song passedSong)
        {
            mainpage = passedpage;
            song = passedSong;

            NavigationPage.SetHasNavigationBar(this, true);
            this.Title = "More Options";                        // Set up basic page details
            this.BackgroundColor = Color.FromHex("#2C2C2C");


            // Initialize tempoGrid to hold tempo and slider

            tempoGrid = new Grid { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            for (int i = 0; i < tempoRows; i++)
            {
                tempoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < tempoColumns; i++)
            {
                tempoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }


            // Initialize tempo label and slider

            tempoLabel = new Label
            {
                Text = "Tempo: 240 BPM",
                TextColor = Color.White,
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.End, 
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.End
            };

            tempoSlider = new Slider(minBPM, maxBPM, 240);
            tempoSlider.HorizontalOptions = LayoutOptions.FillAndExpand;
            tempoSlider.ValueChanged += OnSliderChanged;


            // Add tempo label and slider to tempoGrid

            tempoGrid.Children.Add(tempoLabel, 0, 0);
            tempoGrid.Children.Add(tempoSlider, tempoColumns - 1, 0);
            Grid.SetColumnSpan(tempoLabel, 2);
            Grid.SetColumnSpan(tempoSlider, 4);


            // Initialize buttonGrid to hold buttons

            buttonGrid = new Grid { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand};

            buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            // Initialize buttons and add them to buttonGrid

            saveButton = new Button { Text = "SAVE", TextColor = Color.White, BackgroundColor = Color.Black, Font = Font.SystemFontOfSize(20) };
            loadButton = new Button { Text = "LOAD", TextColor = Color.White, BackgroundColor = Color.Black, Font = Font.SystemFontOfSize(20) };
            clearAllButton = new Button { Text = "CLEAR ALL", TextColor = Color.White, BackgroundColor = Color.Black, Font = Font.SystemFontOfSize(20) };
            undoClearButton = new Button { Text = "UNDO CLEAR", TextColor = Color.White, BackgroundColor = Color.Black, Font = Font.SystemFontOfSize(20) };

            buttonGrid.Children.Add(saveButton, 0, 0);
            buttonGrid.Children.Add(loadButton, 0, 1);
            buttonGrid.Children.Add(clearAllButton, 1, 0);
            buttonGrid.Children.Add(undoClearButton, 1, 1);


            // Add grids to masterLayout

            masterLayout = new StackLayout();
            masterLayout.Children.Add(tempoGrid);           
            masterLayout.Children.Add(buttonGrid);

            Content = masterLayout;

            clearAllButton.Clicked += OnClearAllClicked;
        }

        /// <summary>
        /// Changes the BPM visualization when slider changes
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnSliderChanged(object sender, ValueChangedEventArgs e)
        {
            tempoLabel.Text = "Tempo: " + (int)e.NewValue + " BPM";
        }

        /// <summary>
        /// Clears the stepgrid and audio data when ClearAllButton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClearAllClicked(object sender, EventArgs e)
        {
            mainpage.MakeNewStepGrid();
            song.ClearAllBeats();
            mainpage.scroller.Content = mainpage.stepgrid;

            // Add the scroller (which contains stepgrid) and sidebar to mastergrid
            mainpage.mastergrid.Children.Add(mainpage.scroller, 0, 0); // Add scroller to first column of mastergrid
            mainpage.Content = mainpage.mastergrid;
        }
    }
}
