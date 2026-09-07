using Foundation;
using UIKit;
using WatchConnectivity;

namespace OWCE.Platforms.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            var result = base.FinishedLaunching(application, launchOptions);

            // Apple Watch session manager
            WCSessionManager.SharedManager.StartSession();

            return result;
        }
    }
}
