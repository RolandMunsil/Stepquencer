using Foundation;
using System;
using UIKit;

namespace Stepquencer.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
        App stepquencerApp;
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            App.screenWidth = (int)UIScreen.MainScreen.Bounds.Width;    // Get width of screen

            global::Xamarin.Forms.Forms.Init();

            stepquencerApp = new App();
            LoadApplication(stepquencerApp);

            UIApplication.SharedApplication.StatusBarHidden = true;     // Initialize status bar to be hidden

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            string songStringToImport = Uri.UnescapeDataString(url.AbsoluteString.Substring(url.AbsoluteString.IndexOf('=') + 1));
            Song s = FileUtilities.GetSongFromSongString(songStringToImport);

            if (stepquencerApp.mainpage.loadedSongChanged)
            {
                stepquencerApp.mainpage.LoadWarning(s);
            }
            else
            {
                stepquencerApp.mainpage.SetSong(s);
            }

            return true;
        }
    }
}
