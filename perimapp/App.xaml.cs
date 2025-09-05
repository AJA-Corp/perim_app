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
            perimapp.Platforms.Android.NotificationWorkerManager.ScheduleWork();
#endif
            MainPage = new AppShell();
        }
    }
}