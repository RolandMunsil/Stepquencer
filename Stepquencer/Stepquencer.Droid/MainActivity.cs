using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Media;
using System.Collections.Generic;
using System.Timers;
using System.Threading;

namespace Stepquencer.Droid
{
    [Activity (Label = "Stepquencer", Icon = "@drawable/NewIcon4", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            App.screenWidth = (int)(Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density);   // Get device screen width

            base.OnCreate(bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
               
            LoadApplication(new Stepquencer.App());
        }
    }
}

