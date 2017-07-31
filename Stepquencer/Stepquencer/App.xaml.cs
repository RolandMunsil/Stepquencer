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

        public App ()
        {
            isTablet = screenWidth > TABLET_THRESHOLD;
            mainpage = new MainPage();
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
        }
    }
}
