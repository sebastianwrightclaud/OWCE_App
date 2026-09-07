using Android.App;
using Android.Content.PM;
using Android.OS;

namespace OWCE.Droid
{
    [Activity(Label = "OWCE", Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MauiAppCompatActivity
    {
        //public static int BLE_REQUEST_CODE = 230948;
        public static int REQUEST_ENABLE_BT = 3;

        public override void OnBackPressed()
        {
            if (Mopups.Services.MopupService.Instance.PopupStack.Count > 0)
            {
                Mopups.Services.MopupService.Instance.PopAsync();
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}
