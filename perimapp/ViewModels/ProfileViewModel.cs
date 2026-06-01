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
        private ContentPage _page;

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
        private bool _isEditPopupVisible;

        [ObservableProperty]
        private string _editFirstName;

        [ObservableProperty]
        private string _editLastName;

        public ProfileViewModel(ContentPage page, LocalUserService localUserService, LocalProductService localProductService, ApiProfileService apiProfileService, AuthService authService)
        {
            _page = page;
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
                    UpdateUI(_currentUser);
                }

                var localProducts = await _localProductService.LoadProductsAsync();
                RegisteredProductsCount = localProducts.Count;

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
                        UpdateUI(_currentUser);
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
                await _page.DisplayAlert("Erreur", "Impossible de charger votre profil.", "OK");
                return;
            }

            EditFirstName = _currentUser.FirstName;
            EditLastName = _currentUser.LastName;
            IsEditPopupVisible = true;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditPopupVisible = false;
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
                    await _page.DisplayAlert("Attention", "Vos modifications ont été sauvegardées localement mais n'ont pas pu être envoyées au serveur (Pas de réseau ?)", "OK");
                }

                await _localUserService.SaveUserAsync(_currentUser);

                UpdateUI(_currentUser);
                IsEditPopupVisible = false;

                if (isUpdatedOnServer)
                    await _page.DisplayAlert("Succès", "Votre profil a été mis à jour.", "OK");
            }
            catch (Exception ex)
            {
                await _page.DisplayAlert("Erreur", $"Impossible de sauvegarder : {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteAccountAsync()
        {
            IsMenuVisible = false;

            bool confirm = await _page.DisplayAlert("Supprimer le compte",
                "⚠️ ATTENTION ⚠️\n\nCette action est irréversible. Vos produits, votre foyer et vos identifiants de connexion seront définitivement détruits. Voulez-vous vraiment continuer ?",
                "Supprimer définitivement", "Annuler");

            if (!confirm) return;

            try
            {
                bool isDeletedOnServer = await _apiProfileService.DeleteMyAccountAsync();

                if (!isDeletedOnServer)
                {
                    await _page.DisplayAlert("Erreur réseau", "Impossible de contacter le serveur Render pour supprimer vos données. Réessayez plus tard.", "OK");
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

                await _page.DisplayAlert("Compte supprimé", "Votre compte et l'intégralité de vos données ont été définitivement supprimés.", "OK");

                await Shell.Current.GoToAsync($"///{nameof(StartingView)}");
            }
            catch (Exception ex)
            {
                await _page.DisplayAlert("Erreur", $"Une erreur est survenue : {ex.Message}", "OK");
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
        private void ActivateNotifications()
        {
            var popup = new NotificationPopUp();
            _page.ShowPopup(popup);
        }
    }
}