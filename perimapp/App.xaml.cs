using Microsoft.Maui.Storage;
using perimapp.Data;
using perimapp.Pages;

namespace perimapp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}
