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
    [IntentFilter(new[] {
        Android.Content.Intent.ActionView }, 
        Categories = new[] {
            Android.Content.Intent.CategoryDefault,
            Android.Content.Intent.CategoryBrowsable },
        DataScheme = "stepquencer")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            App.screenWidth = (int)(Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density);   // Get device screen width

            base.OnCreate(bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);

            String data = Intent?.Data?.EncodedSchemeSpecificPart;
            if (data != null)
            {
                String songStr = Uri.UnescapeDataString(data.Substring(data.IndexOf("=") + 1));
                LoadApplication(new Stepquencer.App(songStr));
            }
            else
            {
                LoadApplication(new Stepquencer.App());
            }
        }
    }
}

