
using Xamarin.Forms;
using System.ComponentModel;
using Stepquencer;
#if __IOS__
using Xamarin.Forms.Platform.iOS;
#endif

#if __ANDROID__
using Xamarin.Forms.Platform.Android;
#endif

[assembly: ExportRenderer(typeof(Xamarin.Forms.ScrollView), typeof(CustomScrollViewRenderer))]
namespace Stepquencer
{
    public class CustomScrollViewRenderer : ScrollViewRenderer
    {
        /// <summary>
        /// When the visual element is changed, update it to hold the custom method that removes the xamarin scrollbars from scrollview
        /// </summary>
        /// <param name="e"></param>
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
                return;

            if (e.OldElement != null)
                e.OldElement.PropertyChanged -= OnElementPropertyChanged;  //remove event listener method from old visual element if it exists

            e.NewElement.PropertyChanged += OnElementPropertyChanged;  //add event listener that removes scrollbars to new visual element.

        }

        /// <summary>
        /// Disables the automatically displayed vertical and horizontal scrollbars (we implemented our own)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
#if __ANDROID__

            if (ChildCount > 0)
            {
                GetChildAt(0).HorizontalScrollBarEnabled = false;
                VerticalScrollBarEnabled = false;
            }
#endif

#if __IOS__
            ShowsHorizontalScrollIndicator = false;
            ShowsVerticalScrollIndicator = false;
#endif
        }
    }
}