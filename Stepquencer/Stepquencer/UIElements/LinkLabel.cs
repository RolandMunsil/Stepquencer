﻿using System;
using Xamarin.Forms;
namespace Stepquencer
{
    /// <summary>
    /// Essentially just a Xamarin forms label, but one that also keeps track of a url and opens up a page when tapped
    /// </summary>
    public class LinkLabel : Label
    {
        private TapGestureRecognizer gestureRecognizer;

        /// <summary>
        /// Constructs a generic LinkLabel, where values must be set afterwards
        /// </summary>
        public LinkLabel(String text, String link) : base()
        {
            this.Text = text;
            this.TextColor = Color.Blue;    // Default to traditional link color

            this.gestureRecognizer = new TapGestureRecognizer();
            gestureRecognizer.Tapped += (sender, e) =>
            {
                Device.OpenUri(new Uri(link));
            };

            this.GestureRecognizers.Add(gestureRecognizer);
        }
    }
}
