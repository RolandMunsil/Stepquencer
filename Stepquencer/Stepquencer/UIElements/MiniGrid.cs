using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Stepquencer
{
    /// <summary>
    /// A grid that holds 4 boxviews, which can represent up to 4 instruments at a single pitch and time. 
    /// </summary>
    class MiniGrid : Grid
    {
        //The color of the grid when it has no notes
        private readonly static Color NoNotesColor = Color.FromHex("#606060");

        //The BoxViews for each of the four possible notes on the grid
        private BoxView topLeft;
        private BoxView topRight;
        private BoxView bottomLeft;
        private BoxView bottomRight;

        //The semitone shift of notes on this button
        public int semitoneShift;

        //The event that is triggered when this grid is tapped on.
        public delegate void TapDelegate(MiniGrid tappedGrid);
        public event TapDelegate Tap;

        public MiniGrid(int semitoneShift) : base()
        {
            this.semitoneShift = semitoneShift;

            //No grid spacing
            this.ColumnSpacing = 0;
            this.RowSpacing = 0;
            this.BackgroundColor = Color.Black;
            //Using GridUnitType.Star means the squares will split the grid in half horizontally and vertically
            this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Add in column definitions
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });  // Add in row definitions

            // Add in boxviews
            topLeft = new BoxView { BackgroundColor = NoNotesColor };
            topRight = new BoxView { BackgroundColor = NoNotesColor };
            bottomLeft = new BoxView { BackgroundColor = NoNotesColor };
            bottomRight = new BoxView { BackgroundColor = NoNotesColor };

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
                Command = new Command(delegate() 
                {
                    Tap.Invoke(this);
                })
            };
            //Add recognizer to the grid itself and each of the boxes
            this.GestureRecognizers.Add(tgr);
            topLeft.GestureRecognizers.Add(tgr);
            topRight.GestureRecognizers.Add(tgr);
            bottomLeft.GestureRecognizers.Add(tgr);
            bottomRight.GestureRecognizers.Add(tgr);
        }

        /// <summary>
        /// Adds or removes <code>sidebarColor</code> from this grid.
        /// </summary>
        /// <returns>true if sidebarColor was added, false if not</returns>
        public bool ToggleColor(Color sidebarColor)
        {
            List<Color> colorList = new List<Color>();		// List to store the colors to be added to the new button

            //Figure out what colors are already on this grid
            foreach (BoxView box in this.Children)
            {
                if (box.BackgroundColor != NoNotesColor)
                {
                    colorList.Add(box.BackgroundColor);	// Add the box's color to the array       
                }
            }

            //Remove/add sidebarColor depending on if it's on this grid already
            bool removeColor = colorList.Contains(sidebarColor);            
            if (removeColor)
            {
                colorList.Remove(sidebarColor);
            }
            else
            {
                colorList.Add(sidebarColor);
            }
            //Update color layout
            SetColors(colorList);

            return !removeColor;
        }

        /// <summary>
        /// Sets this grid's display to show the colors in <code>colors</code>
        /// </summary>
        public void SetColors(List<Color> colors)
        {
            //Reset all of the boxes
            foreach (BoxView box in this.Children)
            {
                if (box.BackgroundColor != NoNotesColor)
                {
                    box.BackgroundColor = NoNotesColor;             // Default the box's color to grey   
                    Grid.SetRowSpan(box, 1);                // Default the box to span one row
                    Grid.SetColumnSpan(box, 1);             // Default the box to span one column               
                }
            }

            //Sort colors so the same set of colors is always displayed the same
            colors.Sort((c1, c2) => Math.Sign(c1.Hue - c2.Hue));

            if (colors.Count == 0)		// If the box is reverting to grey
            {
                Grid.SetRowSpan(topLeft, 2);
                Grid.SetColumnSpan(topLeft, 2);     // Make one box take up the whole space

                SetVisibility(true, false, false, false);
            }
            else if (colors.Count == 1)		// If box will have one color
            {
                topLeft.BackgroundColor = colors[0];

                Grid.SetRowSpan(topLeft, 2);
                Grid.SetColumnSpan(topLeft, 2);     // Make one button take up the whole space

                SetVisibility(true, false, false, false);
            }
            else if (colors.Count == 2)	// If box will have two colors
            {
                topLeft.BackgroundColor = colors[0];
                topRight.BackgroundColor = colors[1];

                Grid.SetRowSpan(topLeft, 2);		// Make topLeft take up half of grid
                Grid.SetRowSpan(topRight, 2);		// Make topRight take up half of grid

                SetVisibility(true, true, false, false);
            }
            else if (colors.Count == 3)	// If box will have three colors
            {
                topLeft.BackgroundColor = colors[0];
                topRight.BackgroundColor = colors[1];
                bottomRight.BackgroundColor = colors[2];

                Grid.SetRowSpan(topLeft, 2);	// Make topLeft up take up half the grid; other two split the remaining space

                SetVisibility(true, true, true, false);
            }
            else                    // If box will have four colors
            {
                // Assign the correct colors
                topLeft.BackgroundColor = colors[0];
                topRight.BackgroundColor = colors[1];
                bottomLeft.BackgroundColor = colors[2];
                bottomRight.BackgroundColor = colors[3];

                // Make all boxes visible
                SetVisibility(true, true, true, true);

                // No resizing needed
            }
        }

        /// <summary>
        /// Set the visibility of all boxes in the Minigrid
        /// </summary>
        /// <param name="topLeft">If set to <c>true</c> top left.</param>
        /// <param name="topRight">If set to <c>true</c> top right.</param>
        /// <param name="bottomLeft">If set to <c>true</c> bottom left.</param>
        /// <param name="bottomRight">If set to <c>true</c> bottom right.</param>
        private void SetVisibility(bool topLeft, bool topRight, bool bottomRight, bool bottomLeft)
        {
            this.topLeft.IsVisible = topLeft;
            this.topRight.IsVisible = topRight;
            this.bottomRight.IsVisible = bottomRight;
            this.bottomLeft.IsVisible = bottomLeft;
        }
    }
}
