using System;
using UIKit;
[assembly: Xamarin.Forms.Dependency(typeof(Stepquencer.StatusBarImplementation))]
namespace Stepquencer
{
    public class StatusBarImplementation : IStatusBar
    {


        #region IStatusBar implementation

        public void HideStatusBar()
        {
            UIApplication.SharedApplication.StatusBarHidden = true;
        }

        public void ShowStatusBar()
        {
            UIApplication.SharedApplication.StatusBarHidden = false;
        }

        #endregion
    }
}
