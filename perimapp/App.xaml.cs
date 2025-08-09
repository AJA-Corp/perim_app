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
            MainPage = new AppShell(); // On démarre avec AppShell
        }

        protected override void OnStart()
        {
            base.OnStart();

            int savedUserId = Preferences.Default.Get("UserId", -1);
            Console.WriteLine(
                $"[DEBUG] ID utilisateur récupéré depuis Preferences : {savedUserId}"
            );

            if (savedUserId > 0)
            {
                // Utilisateur déjà connecté
                AppData.CurrentUserId = savedUserId;
                // Rediriger vers la page principale
                Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                // Rediriger vers la page de démarrage
                Shell.Current.GoToAsync($"//{nameof(StartingPage)}");
            }
        }
    }
}
