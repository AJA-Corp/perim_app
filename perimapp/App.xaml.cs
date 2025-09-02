using Microsoft.Maui.Storage;
using perimapp.Data;
using perimapp.Pages;

namespace perimapp
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            Services = serviceProvider;
            MainPage = new AppShell();
        }
    }
}
