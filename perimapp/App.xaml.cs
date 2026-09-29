using Microsoft.Maui.Storage;
using perimapp.Data;
using perimapp.Models;
using perimapp.Views;
using perimapp.Services;

namespace perimapp
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        private readonly LocalUserService _localUserService;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            Services = serviceProvider;

            _localUserService = Services.GetService<LocalUserService>();

            var syncService = Services.GetService<SyncService>();
            if (syncService != null)
            {
                _ = Task.Run(async () => await syncService.ProcessSyncAsync());
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                return new Window(new AppShell());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\n[CRASH FATAL MAUI] : {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[CAUSE EXACTE] : {ex.InnerException.Message}\n\n");
                }
                throw;
            }
        }

        protected override async void OnStart()
        {
            base.OnStart();

            AppData.CurrentUser = await _localUserService.LoadUserAsync();

            if (AppData.CurrentUser != null)
                Console.WriteLine($"[DEBUG] Utilisateur local chargé : {AppData.CurrentUser.FirstName} {AppData.CurrentUser.LastName}");
            else
                Console.WriteLine("[DEBUG] Aucun utilisateur trouvé localement.");
        }

        protected override async void OnResume()
        {
            base.OnResume();
            AppData.CurrentUser = await _localUserService.LoadUserAsync();
        }
    }
}