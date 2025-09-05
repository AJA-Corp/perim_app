using Microsoft.Maui.Storage;
using perimapp.Data;
using perimapp.Pages;
using Plugin.LocalNotification;

namespace perimapp
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            Services = serviceProvider;

#if ANDROID
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var intent = new Android.Content.Intent(Android.App.Application.Context, typeof(perimapp.Platforms.Android.NotificationForegroundService));
                Android.App.Application.Context.StartForegroundService(intent);
            }
#endif
            MainPage = new AppShell();
        }
    }
}