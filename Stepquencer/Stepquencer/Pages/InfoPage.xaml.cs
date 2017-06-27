using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {

            LinkLabel infoLabel = new LinkLabel
            {
                Text = "Link to Freepik's website",
                LinkText = "http://www.freepik.com/",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            this.Content = infoLabel;
        }
    }
}
