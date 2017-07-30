using System;
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
            ScrollView scroller = new ScrollView 
            { 
                Orientation = ScrollOrientation.Vertical
            };
            StackLayout musicCreditsLayout = new StackLayout { Orientation = StackOrientation.Vertical};

            using (StreamReader stream = new StreamReader(FileUtilities.LoadEmbeddedResource("credits.txt")))
            {
                while (stream.Peek() != -1)
                {
                    String line = stream.ReadLine();
                    if (line == "")
                    {
                        musicCreditsLayout.Children.Add(new Label());
                    }
                    else
                    {
                        String[] parts = line.Split('|');

                        LinkLabel label = new LinkLabel(parts[0], parts[1]);
                        //label.HorizontalOptions = LayoutOptions.Center;
                        musicCreditsLayout.Children.Add(label);
                    }
                }
            }

			Label ourCredit = new Label
            {
                Text = "\nMade by Gabriel Brown, Roland Munsil, Paige Pfeiffer, and Mani Diaz\n© 2017 Gabriel Brown",
                TextColor = Color.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.End,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End
            };

            musicCreditsLayout.Children.Add(ourCredit);

            scroller.Content = musicCreditsLayout;
            this.Content = scroller;
        }
    }
}
