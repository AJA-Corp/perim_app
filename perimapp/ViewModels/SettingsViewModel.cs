using System;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.PopUp;
using perimapp.Services;
using perimapp.Views;

namespace perimapp.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly LocalUserService _localUserService;
        private readonly AuthService _authService;
        private readonly SyncService _syncService;

        [ObservableProperty]
        private string _userName = "Utilisateur";

        [ObservableProperty]
        private string _userEmail = "";

        [ObservableProperty]
        private string _familyCode = "";

        [ObservableProperty]
        private string _appVersion = AppInfo.Current.VersionString;

        [ObservableProperty]
        private bool _isSyncing;

        public SettingsViewModel(LocalUserService localUserService, AuthService authService, SyncService syncService)
        {
            _localUserService = localUserService;
            _authService = authService;
            _syncService = syncService;
        }

        [RelayCommand]
        public async Task LoadSettingsAsync()
        {
            try
            {
                var user = await _localUserService.LoadUserAsync();
                if (user != null)
                {
                    UserName = $"{user.FirstName} {user.LastName}".Trim();
                    UserEmail = user.Email ?? "";
                    FamilyCode = user.HomeCode ?? "Aucun";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettingsViewModel] Erreur chargement utilisateur : {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task EditProfileAsync()
        {
            try
            {
                var user = await _localUserService.LoadUserAsync();
                var popup = new EditProfilePopUp(user?.FirstName ?? "", user?.LastName ?? "");
                await Shell.Current.CurrentPage.ShowPopupAsync(popup);

                if (popup.Result != null && user != null)
                {
                    user.FirstName = popup.Result.FirstName?.Trim();
                    user.LastName = popup.Result.LastName?.Trim();
                    await _localUserService.SaveUserAsync(user);
                    await LoadSettingsAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettingsViewModel] Erreur modification profil : {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ConfigureNotificationsAsync()
        {
            try
            {
                var popup = new NotificationPopUp();
                await Shell.Current.CurrentPage.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettingsViewModel] Erreur popup notification : {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SyncDataAsync()
        {
            if (IsSyncing) return;

            try
            {
                IsSyncing = true;
                await _syncService.ProcessSyncAsync();
                var popup = new InfosPopUp("Synchronisation", "Les données ont été synchronisées avec succès.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                var popup = new InfosPopUp("Erreur", $"Échec de la synchronisation : {ex.Message}", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(popup);
            }
            finally
            {
                IsSyncing = false;
            }
        }

        [RelayCommand]
        private async Task ViewDeletedProductsAsync()
        {
            await Shell.Current.GoToAsync(nameof(DeletedProductView));
        }

        [RelayCommand]
        private async Task CopyFamilyCodeAsync()
        {
            if (!string.IsNullOrEmpty(FamilyCode) && FamilyCode != "Aucun")
            {
                await Clipboard.Default.SetTextAsync(FamilyCode);
                var popup = new InfosPopUp("Copié !", "Le code de votre foyer a été copié dans le presse-papiers.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(popup);
            }
        }

        [RelayCommand]
        private async Task AboutAppAsync()
        {
            var popup = new InfosPopUp(
                "À propos de Perim'App",
                $"Perim'App v{AppInfo.Current.VersionString}\nApplication de gestion anti-gaspillage alimentaire et suivi des dates limites de consommation (DLC).\n\nDéveloppé par AJA Corp.",
                "Fermer"
            );
            await Shell.Current.CurrentPage.ShowPopupAsync(popup);
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            var confirmPopup = new BoolPopUp("Déconnexion", "Êtes-vous sûr de vouloir vous déconnecter ?", "Oui", "Non");
            await Shell.Current.CurrentPage.ShowPopupAsync(confirmPopup);

            if (!confirmPopup.Result) return;

            try
            {
                _authService.SignOut();
                _localUserService.ClearUser();
                AppData.Clear();
                await Shell.Current.GoToAsync($"///{nameof(StartingView)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SettingsViewModel] Erreur déconnexion : {ex.Message}");
            }
        }
    }
}
