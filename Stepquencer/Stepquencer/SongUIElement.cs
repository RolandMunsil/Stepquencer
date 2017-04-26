﻿using System;
using Xamarin.Forms;
using System.IO;
namespace Stepquencer
{

    /// <summary>
    /// Encapsulation of a single song that a user can load or delete
    /// </summary>
    public class SongUIElement : StackLayout
    {
        public readonly String filePath;                // Path to the song file this UI element references

        public event Action<SongUIElement> DeleteClicked;   // Allows other classes to decide what happens
        public event Action<SongUIElement> Tap;             // when either this is tapped or delete button is tapped

        public SongUIElement(String filePath) : base()
        {
            this.filePath = filePath;

            this.Orientation = StackOrientation.Horizontal; //
            this.BackgroundColor = Color.Black;             // Stylistic choices
            this.HeightRequest = 50;                        //


            // Make a new label initialized with the song name

            Label songLabel = new Label             
            {
                Text = FileUtilities.SongNameFromFilePath(filePath),// Make sure it displays the name of song
                FontSize = 20,                                      //*
                BackgroundColor = Color.Black,                      //*
                TextColor = Color.White,                            //* Stylistic choices
                HorizontalOptions = LayoutOptions.CenterAndExpand,  //*
                VerticalOptions = LayoutOptions.CenterAndExpand,    //*
                InputTransparent = true                             // Ensures the user can't tap on label instead of main body of this object
            };


            // Make a button that lets you delete this object's song

            Button deleteButton = new Button
            {
                Text = "DELETE",
                TextColor = Color.Red,
                Margin = 7
            };

            this.Children.Add(songLabel);       //
            this.Children.Add(deleteButton);    // Add visual elements to this object 


            // Ensure that gestures on the delete button and rest of object are handled by public events

            deleteButton.Clicked += (s, e) => { DeleteClicked.Invoke(this); };


            TapGestureRecognizer tgr = new TapGestureRecognizer()
            {
                Command = new Command(()=> { Tap.Invoke(this);})
            };
            this.GestureRecognizers.Add(tgr);
        }
    }
}
