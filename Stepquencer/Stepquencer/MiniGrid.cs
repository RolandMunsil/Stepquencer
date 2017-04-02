using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace Stepquencer
{
    class MiniGrid : Grid
    {
        private BoxView topLeft;
        private BoxView topRight;
        private BoxView bottomLeft;
        private BoxView bottomRight;

        private int semitoneShift;

        //TODO: This is fairly hacky - figure out a better way to do it?
        private MainPage mainPage;

        public MiniGrid(MainPage mainPage, int semitoneShift) : base()
        {
            this.mainPage = mainPage;
            this.semitoneShift = semitoneShift;

            //No grid spacing
            this.ColumnSpacing = 0;
            this.RowSpacing = 0;
            this.BackgroundColor = Color.Black;

            this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Add in column definitions
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });  // Add in row definitions

            // Add in boxviews
            topLeft = new BoxView { BackgroundColor = MainPage.Grey };
            topRight = new BoxView { BackgroundColor = MainPage.Grey, IsVisible = false };
            bottomLeft = new BoxView { BackgroundColor = MainPage.Grey, IsVisible = false };
            bottomRight = new BoxView { BackgroundColor = MainPage.Grey, IsVisible = false };

            this.Children.Add(topLeft, 0, 0);
            this.Children.Add(topRight, 1, 0);
            this.Children.Add(bottomLeft, 0, 1);
            this.Children.Add(bottomRight, 1, 1);

            //When created the top left boxview will cover the whole grid
            Grid.SetColumnSpan(topLeft, 2);
            Grid.SetRowSpan(topLeft, 2);


            // Give the grid a tapGesture recognizer
            TapGestureRecognizer tgr = new TapGestureRecognizer()
            {
                Command = new Command(OnTap)
            };
            this.GestureRecognizers.Add(tgr);
            topLeft.GestureRecognizers.Add(tgr);
            topRight.GestureRecognizers.Add(tgr);
            bottomLeft.GestureRecognizers.Add(tgr);
            bottomRight.GestureRecognizers.Add(tgr);
        }

        void OnTap()
        {
            //Grab the stuff we need from mainPage
            Color sidebarColor = mainPage.sideBarColor;
            Song song = mainPage.song;
            Dictionary<Color, Instrument> colorMap = mainPage.colorMap;

            // Changes UI represenation and returns new set of colors on this grid
            List<Color> colors = ChangeColor(sidebarColor);      

            // If sidebar color isn't part of button's new set of colors, remove it
            if (!colors.Contains(sidebarColor))
            {
                Instrument.Note toRemove = colorMap[sidebarColor].AtPitch(semitoneShift);
                song.RemoveNote(toRemove, Grid.GetColumn(this));
            }
            else
            {
                Instrument.Note toAdd = colorMap[sidebarColor].AtPitch(semitoneShift);
                song.AddNote(toAdd, Grid.GetColumn(this));
            }
        }

        /// <summary>
        /// Adds or removes <code>sidebarColor</code> from this grid.
        /// </summary>
        List<Color> ChangeColor(Color sidebarColor)
        {
            List<Color> colorList = new List<Color>();		// List to store the colors to be added to the new button

            //Figure out what colors are already on this grid
            foreach (BoxView box in this.Children)
            {
                if (box.BackgroundColor != MainPage.Grey)
                {
                    colorList.Add(box.BackgroundColor);	// Add the box's color to the array       
                }
            }

            //Remove/add sidebarColor depending on if it's on this grid already
            if (colorList.Contains(sidebarColor))			// If button already has the sidebar color, remove it from colorList
            {
                colorList.Remove(sidebarColor);
            }
            else    										// If button doesn't already have the sidebar color, add it to colorList
            {
                colorList.Add(sidebarColor);
            }

            this.SetColors(colorList);

            return colorList;
        }

        /// <summary>
        /// Sets this grid's display to show the colors in <code>colors</code>
        /// </summary>
        /// <param name="colors"></param>
        public void SetColors(List<Color> colors)
        {
            //Reset all of the boxes
            foreach (BoxView box in this.Children)
            {
                if (box.BackgroundColor != MainPage.Grey)
                {
                    box.BackgroundColor = MainPage.Grey;             // Default the box's color to grey   
                    Grid.SetRowSpan(box, 1);                // Default the box to span one row
                    Grid.SetColumnSpan(box, 1);             // Default the box to span one column               
                }

                box.IsVisible = false;
            }

            //Sort colors so the same set of colors always looks the same
            colors.Sort((c1, c2) => Math.Sign(c1.Hue - c2.Hue));

            if (colors.Count == 0)		// If the box is reverting to grey
            {
                topLeft.IsVisible = true;

                Grid.SetRowSpan(topLeft, 2);
                Grid.SetColumnSpan(topLeft, 2);     // Make one box take up the whole space
            }
            else if (colors.Count == 1)		// If box will have one color
            {
                topLeft.IsVisible = true;

                topLeft.BackgroundColor = colors[0];

                Grid.SetRowSpan(topLeft, 2);
                Grid.SetColumnSpan(topLeft, 2);     // Make one button take up the whole space
            }
            else if (colors.Count == 2)	// If box will have two colors
            {
                topLeft.IsVisible = true;
                topRight.IsVisible = true;

                topLeft.BackgroundColor = colors[0];
                topRight.BackgroundColor = colors[1];

                Grid.SetRowSpan(topLeft, 2);		// Make topLeft take up half of grid
                Grid.SetRowSpan(topRight, 2);		// Make topRight take up half of grid
            }
            else if (colors.Count == 3)	// If box will have three colors
            {
                topLeft.IsVisible = true;
                topRight.IsVisible = true;
                bottomRight.IsVisible = true;	//Make topright and bottomright visible, make bottom left invisible

                topLeft.BackgroundColor = colors[0];
                topRight.BackgroundColor = colors[1];
                bottomRight.BackgroundColor = colors[2];

                Grid.SetRowSpan(topLeft, 2);	// Make topLeft up take up half the grid; other two split the remaining space
            }
            else                    // If box will have four colors
            {
                topRight.IsVisible = true;
                topLeft.IsVisible = true;
                bottomRight.IsVisible = true;		// Make all boxes visible
                bottomLeft.IsVisible = true;

                // Assign the correct colors
                topLeft.BackgroundColor = colors[0];
                topRight.BackgroundColor = colors[1];
                bottomLeft.BackgroundColor = colors[2];
                bottomRight.BackgroundColor = colors[3];

                // No resizing needed
            }
        }
    }
}
