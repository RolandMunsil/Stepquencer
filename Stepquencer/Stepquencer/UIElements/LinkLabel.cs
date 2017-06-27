using System;
using Xamarin.Forms;
namespace Stepquencer
{
    /// <summary>
    /// Essentially just a Xamarin forms label, but one that also keeps track of a url and opens up a page when tapped
    /// </summary>
    public class LinkLabel : Label
    {
        private string linkText;
        private TapGestureRecognizer gestureRecognizer;

        public string LinkText
        {
            get { return linkText;}
            set { linkText = value;}
        }

        public LinkLabel() : base()
        {
            this.linkText = "";
            this.TextColor = Color.Blue;    // Default to traditional link color

            this.gestureRecognizer = new TapGestureRecognizer();
            gestureRecognizer.Tapped += (sender, e) =>
            {
                try
                {
                    Device.OpenUri(new Uri(((LinkLabel)sender).LinkText));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex + "\n\nMake sure you set the link text correctly!");
                }
            };

            this.GestureRecognizers.Add(gestureRecognizer);
        }
    }
}
