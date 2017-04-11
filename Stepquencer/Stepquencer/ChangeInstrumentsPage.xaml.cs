using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class ChangeInstrumentsPage : ContentPage
    {

        private MainPage mainpage;                      // Reference to the mainPage instance so that currently selected Colors can be used
        private HashSet<Color> currentColors;           // Colors user has currently selected

        public ChangeInstrumentsPage(MainPage mainpage)
        {
            this.mainpage = mainpage;
            this.BackgroundColor = Color.Black;             //*
            this.Title = "Pick your instruments";           //* Set up basic page attributes
            NavigationPage.SetHasBackButton(this, false);   //*


            // Put the currently selected colors at the top of the screen
            foreach (Color color in mainpage.colorMap.Keys)
            {
                // TODO:

                // 1) Get the .wav file associated with that key (just by referencing colorMap again)
                // 2) Get the image associated with that color (or perhaps the .wav file, either way need to make a map in MainPage)
                // 3) Add to currentColors
                // 4) Create four View objects to show each instrument at top
            }

            // TODO:

            // 1) For each instrument/color/.wav file combo:
            //      a) Get the other materials needed, depending on how things are associated
            //      b) Combine color and image in a Button object, add it below the currently selected instruments
            //      c) If currentColors.Contains the color of current instrument, make sure it's highlighted
            //

        }

        // TODO:

        // Write an OnButtonClicked method that deals with highlighting and selection logic
    }
}
