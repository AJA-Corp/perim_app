using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using perimapp.Data;
using perimapp.Models;
using perimapp.PopUp;
using perimapp.Services;
using perimapp.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace perimapp.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly LocalUserService _localUserService;
        private readonly LocalProductService _localProductService;
        private readonly ApiProfileService _apiProfileService;
        private readonly AuthService _authService;

        private UserProfileDetails? _currentUser;

        [ObservableProperty]
        private bool _isMenuVisible;

        [ObservableProperty]
        private string _userName = "Chargement...";

        [ObservableProperty]
        private int _registeredProductsCount;

        [ObservableProperty]
        private int _lostProductsCount;

        [ObservableProperty]
        private string _familyCode = "Chargement...";

        [ObservableProperty]
        private string _editFirstName;

        [ObservableProperty]
        private string _editLastName;

        public ProfileViewModel(LocalUserService localUserService, LocalProductService localProductService, ApiProfileService apiProfileService, AuthService authService)
        {
            _localUserService = localUserService;
            _localProductService = localProductService;
            _apiProfileService = apiProfileService;
            _authService = authService;
        }

        [RelayCommand]
        public async Task LoadProfileDataAsync()
        {
            try
            {
                _currentUser = await _localUserService.LoadUserAsync();
                if (_currentUser != null)
                {
                    MainThread.BeginInvokeOnMainThread(() => UpdateUI(_currentUser));
                }

                var localProducts = await _localProductService.LoadProductsAsync();
                MainThread.BeginInvokeOnMainThread(() => RegisteredProductsCount = localProducts.Count);

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    var serverProfile = await _apiProfileService.GetOrCreateMyProfileAsync();
                    if (serverProfile != null && _currentUser != null)
                    {
                        _currentUser.HomeCode = serverProfile.HomeCode;
                        _currentUser.IsValidated = serverProfile.IsValidated;
                        _currentUser.FirstName = serverProfile.FirstName;
                        _currentUser.LastName = serverProfile.LastName;
                        _currentUser.LostProductCount = serverProfile.LostProductCount;

                        await _localUserService.SaveUserAsync(_currentUser);
                        MainThread.BeginInvokeOnMainThread(() => UpdateUI(_currentUser));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ProfileView [ERREUR] : {ex.Message}");
            }
        }

        private void UpdateUI(UserProfileDetails user)
        {
            string fullName = $"{user.FirstName} {user.LastName}".Trim();
            UserName = string.IsNullOrEmpty(fullName) ? "Mon Profil" : fullName;

            FamilyCode = string.IsNullOrEmpty(user.HomeCode) ? "Aucun" : user.HomeCode;
            LostProductsCount = user.LostProductCount;
        }

        [RelayCommand]
        private void ToggleSettings()
        {
            IsMenuVisible = !IsMenuVisible;
        }

        [RelayCommand]
        private async Task EditProfileAsync()
        {
            IsMenuVisible = false;

            if (_currentUser == null)
            {
                var errorPopup = new InfosPopUp("Erreur", "Impossible de charger votre profil.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                return;
            }

            var popup = new EditProfilePopUp(_currentUser.FirstName, _currentUser.LastName);

            await Shell.Current.CurrentPage.ShowPopupAsync(popup);

            var result = popup.Result;

            if (result != null)
            {
                EditFirstName = result.FirstName;
                EditLastName = result.LastName;

                await SaveEditAsync();
            }
        }

        [RelayCommand]
        private async Task SaveEditAsync()
        {
            try
            {
                _currentUser.FirstName = EditFirstName?.Trim();
                _currentUser.LastName = EditLastName?.Trim();

                bool isUpdatedOnServer = await _apiProfileService.UpdateNameAsync(_currentUser.FirstName, _currentUser.LastName);

                if (!isUpdatedOnServer)
                {
                    var successPopup = new InfosPopUp("Attention", "Vos modifications ont été sauvegardées localement mais n'ont pas pu être envoyées au serveur (Pas de réseau ?)", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(successPopup);
                }

                await _localUserService.SaveUserAsync(_currentUser);

                UpdateUI(_currentUser);

                if (isUpdatedOnServer)
                {
                    var successPopup = new InfosPopUp("Succès", "Votre profil a été mis à jour.", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(successPopup);
                }
            }
            catch (Exception ex)
            {
                var errorPopup = new InfosPopUp("Erreur", $"Impossible de sauvegarder : {ex.Message}", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
            }
        }

        [RelayCommand]
        private async Task DeleteAccountAsync()
        {
            IsMenuVisible = false;

            var confirmPopUp = new BoolPopUp(
                "Supprimer le compte",
                "⚠️ ATTENTION ⚠️\n\nCette action est irréversible. Vos produits, votre foyer et vos identifiants de connexion seront définitivement détruits. Voulez-vous vraiment continuer ?",
                "Supprimer définitivement", "Annuler");
            await Shell.Current.CurrentPage.ShowPopupAsync(confirmPopUp);

            if (!confirmPopUp.Result) return;

            try
            {
                bool isDeletedOnServer = await _apiProfileService.DeleteMyAccountAsync();

                if (!isDeletedOnServer)
                {
                    var errorNetPopup = new InfosPopUp("Erreur réseau", "Impossible de contacter le serveur Render pour supprimer vos données. Réessayez plus tard.", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(errorNetPopup);
                    return;
                }

                bool isDeletedOnNeon = await _authService.DeleteNeonAccountAsync();

                if (!isDeletedOnNeon)
                {
                    System.Diagnostics.Debug.WriteLine("Attention : Le compte Neon n'a pas pu être supprimé automatiquement.");
                }

                _authService.SignOut();
                _localUserService.ClearUser();
                _localProductService.ClearAllProducts();

                var successPopup = new InfosPopUp("Compte supprimé", "Votre compte et l'intégralité de vos données ont été définitivement supprimés.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(successPopup);

                await Shell.Current.GoToAsync($"///{nameof(StartingView)}");
            }
            catch (Exception ex)
            {
                var errorPopup = new InfosPopUp("Erreur", $"Une erreur est survenue : {ex.Message}", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            IsMenuVisible = false;

            _authService.SignOut();
            _localUserService.ClearUser();
            _localProductService.ClearAllProducts();

            AppData.Clear();

            await Shell.Current.GoToAsync($"///{nameof(StartingView)}");
        }

        [RelayCommand]
        private async Task ActivateNotificationsAsync()
        {
            var popup = new NotificationPopUp();
            await Shell.Current.CurrentPage.ShowPopupAsync(popup);
        }
    }
}