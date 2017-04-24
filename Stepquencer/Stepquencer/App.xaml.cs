using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Stepquencer
{
    public partial class App : Application
    {
        private MainPage mainpage;

        public App ()
        {
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
            if (mainpage.firstTime) { mainpage.displayInstructions(); }
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
