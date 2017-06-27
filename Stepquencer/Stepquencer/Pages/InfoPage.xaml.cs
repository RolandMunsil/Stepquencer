using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {

            Label infoLabel = new Label {Text = "Info page!"};

            this.Content = infoLabel;

        }
    }
}
