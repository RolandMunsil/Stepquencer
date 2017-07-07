﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            this.BackgroundColor = Color.Black;
            this.Title = "Credits";
            ScrollView scroller = new ScrollView { Orientation = ScrollOrientation.Vertical };
            StackLayout pageLayout = new StackLayout { Orientation = StackOrientation.Vertical };

            using (StreamReader stream = new StreamReader(FileUtilities.LoadEmbeddedResource("credits.txt")))
            {
                while (stream.Peek() != -1)
                {
                    String line = stream.ReadLine();
                    if (line == "")
                    {
                        pageLayout.Children.Add(new Label());
                    }
                    else
                    {
                        String[] parts = line.Split('|');
                        pageLayout.Children.Add(new LinkLabel(parts[0], parts[1]));
                    }
                }
            }

            scroller.Content = pageLayout;
            this.Content = scroller;
        }
    }
}
