using Foundation;
using UIKit;
using Plugin.LocalNotification;

namespace perimapp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    
    public static UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
    {
        return UIInterfaceOrientationMask.Portrait;
    }

    [Export("application:didRegisterUserNotificationSettings:")]
    public void DidRegisterUserNotificationSettings(UIApplication application, UIUserNotificationSettings notificationSettings)
    {
        application.RegisterForRemoteNotifications();
    }
}