using Foundation;

namespace OWCE.Platforms.iOS
{
    // iOS 27 terminates apps that haven't adopted the UIScene lifecycle, so the
    // app declares a scene manifest in Info.plist that points at this delegate.
    [Register("SceneDelegate")]
    public class SceneDelegate : MauiUISceneDelegate
    {
    }
}
