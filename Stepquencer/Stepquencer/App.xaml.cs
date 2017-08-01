using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class App : Application
    {
        public static readonly int TABLET_THRESHOLD = 1000;  // If the width of the screen is higher than this number, we can assume the device is a tablet

        private MainPage mainpage;
        public static int screenWidth;
        public static bool isTablet;    // Based on TABLET_THRESHOLD

        public String songStringToImport;

        public App(String songStringFromUrl = null)
        {
            //Uncomment to test out url loading code. This is the startup song
            //songStringFromUrl = "C3MNCvb091Mw5OUyNFMoyS9JzFFISk0sKa4JzkkscEosLvbJL69xSszJSUwJzkssSq1xBorXeGR6JJbwcjkBVSoY1AA1I6mwModKGIIkwCrhQkYgIZAJcBFjTEUmmEKmNUbIQlgsM6sxxq0CxUZzolVa4PKZJaYLDQ0wvGaIJQAMjbCIGRP0naEJ0Y42NCVGqZGJAS8XAA==";
            isTablet = screenWidth > TABLET_THRESHOLD;
            mainpage = new MainPage(songStringFromUrl);
            MainPage = new NavigationPage(mainpage) 
            {
                BarBackgroundColor = Color.Black,
                BarTextColor = Color.White
            };
        }

        protected override void OnStart ()
        {
            // Handle when app starts
            if (mainpage.firstTime) { mainpage.DisplayInstructions(); }
        }

        protected override void OnSleep ()
        {
            // Handle when app sleeps
            mainpage.StopPlayingSong();
        }

        protected override void OnResume ()
        {
            // Handle when your app resumes
            if(songStringToImport != null)
            {
                Device.BeginInvokeOnMainThread(delegate
                {
                    mainpage.SetSong(FileUtilities.GetSongFromSongString(songStringToImport));
                });
                songStringToImport = null;
            }
        }
    }
}
