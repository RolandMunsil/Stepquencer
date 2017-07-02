using System;
using System.Collections.Generic;
using System.IO;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class InfoPage : ContentPage
    {

        private static List<Author> authors;

        public InfoPage()
        {
            this.BackgroundColor = Color.Black;
            StackLayout pageLayout = new StackLayout { Orientation = StackOrientation.Vertical };

            ReadAuthorData();
            foreach (LinkLabel label in ConstructLabels())
            {
                pageLayout.Children.Add(label);
            }

            pageLayout.Children.Add(new LinkLabel()
            {
                Text = "All sounds except clap are from the soundfont \"SGM - V2.01\" by David Shan",
                LinkText = "http://www.geocities.jp/shansoundfont/"
            });

            pageLayout.Children.Add(new LinkLabel()
            {
                Text = "Clap sound is \"Clap-Clean Reverb\" by Freesound user young_daddy",
                LinkText = "http://freesound.org/people/young_daddy/"
            });

            this.Content = pageLayout;
        }

        /// <summary>
        /// Returns a list a linklabels constructed from the Author objects in authors
        /// </summary>
        /// <returns>The labels.</returns>
        private List<LinkLabel> ConstructLabels()
        {
            List<LinkLabel> linkLabels = new List<LinkLabel>();

            foreach (Author author in authors)
            {
                linkLabels.Add(new LinkLabel(author));
            }

            return linkLabels;
        }

        /// <summary>
        /// Reads in data about authors, instantiates Author objects in list of authors
        /// </summary>
        private void ReadAuthorData()
        {
            authors = new List<Author>();
            
            //Read in data
            using (StreamReader stream = new StreamReader(FileUtilities.LoadEmbeddedResource("_iconAuthors.txt")))
            {
                while (stream.Peek() != -1) //While there are no more lines left to read
                {
                    string[] authorInfo = stream.ReadLine().Split('|');
                    if (authorInfo.Length< 4)
                    {
                        authors.Add(new Author(authorInfo[0], authorInfo[1], authorInfo[2]));
                    }
                    else
                    {
                        authors.Add(new Author(authorInfo[0], authorInfo[1], authorInfo[2], authorInfo[3]));
                    }
                }
            }
        }
    }
}
