using Microsoft.Maui.Storage;
using perimapp.Data;
using perimapp.Models;
using perimapp.Pages;
using Plugin.LocalNotification;
using perimapp.Services;


namespace perimapp
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        // On garde un service local pour les données utilisateur
        private readonly LocalUserService _localUserService;

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
            _localUserService = new LocalUserService();

            // On initialise la page principale
            MainPage = new AppShell();
        }


        // Appelée automatiquement au démarrage de l'application.
        // on charge l'utilisateur en local si présent.
        protected override async void OnStart()
        {
            base.OnStart();

            // Test TOTP functionality (for debugging)
#if DEBUG
            try
            {
                perimapp.Testing.TotpTest.RunBasicTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] TOTP Test failed: {ex.Message}");
            }
#endif

            // Charger l'utilisateur depuis le stockage local
            AppData.CurrentUser = await _localUserService.LoadUserAsync();

            if (AppData.CurrentUser != null)
            {
                Console.WriteLine($"[DEBUG] Utilisateur local chargé : {AppData.CurrentUser.FirstName} {AppData.CurrentUser.LastName}");
            }
            else
            {
                Console.WriteLine("[DEBUG] Aucun utilisateur trouvé localement.");
            }
        }

        //Appelée quand l'application revient en avant-plan.
        // On recharge l'utilisateur au cas où ses données locales ont changé.
        protected override async void OnResume()
        {
            base.OnResume();
            AppData.CurrentUser = await _localUserService.LoadUserAsync();
        }
    }
}