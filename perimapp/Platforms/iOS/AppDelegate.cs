using Foundation;
using UIKit;

namespace perimapp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    
    public static UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
    {
        return UIInterfaceOrientationMask.Portrait; // 🔥 Bloque en mode portrait
    }
}