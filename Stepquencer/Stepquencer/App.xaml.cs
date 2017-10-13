/* STEPQUENCER
 * Build: 1.1.1
 * 
 * Made By Gabriel Brown, Roland Munsil, Paige Pfeiffer and Mani Diaz
 */ 

﻿﻿using System;
using Xamarin.Forms;

namespace Stepquencer
{
    public partial class App : Application
    {
        public static readonly int TABLET_THRESHOLD = 1020;  // If the width of the screen is higher than this number, we can assume the device is a tablet

        public MainPage mainpage;
        public static int screenWidth;
        public static bool isTablet;    // Based on TABLET_THRESHOLD

        public String songStringToImport;

        public App(String songStringFromUrl = null)
        {
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

            // TODO: Is the following still necessary for android? If not we can just delete it.

    //        if(songStringToImport != null)
    //        {
               

				//Song s = FileUtilities.GetSongFromSongString(songStringToImport);
				//Device.BeginInvokeOnMainThread(delegate
				//{
    //                if (mainpage.loadedSongChanged)
    //                {
    //                    mainpage.LoadWarning(s);
    //                }
    //                else
    //                {
    //                    mainpage.SetSong(s);
    //                }
				//});
				//songStringToImport = null;
            //}

        }
    }
}
