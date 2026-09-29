using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.LocalNotification;

namespace perimapp.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        CheckAndRequestNotificationPermission();
    }

    private async void CheckAndRequestNotificationPermission()
    {
        await LocalNotificationCenter.Current.RequestNotificationPermission();
    }
}