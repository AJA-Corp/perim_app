using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Models;
using perimapp.PopUp;
using perimapp.Services;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using perimapp.Pages;

namespace perimapp.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly NeonUserService _userService;
        private readonly NeonProductService _productService;
        private readonly LocalUserService _localUserService;
        private readonly LocalProductService _localProductService;

        private UserProfileDetails? _currentUser;
        private ContentPage _page;

        private bool _isMenuVisible = false;
        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set => SetProperty(ref _isMenuVisible, value);
        }

        private string _userName = "Chargement...";
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private int _registeredProductsCount = 0;
        public int RegisteredProductsCount
        {
            get => _registeredProductsCount;
            set => SetProperty(ref _registeredProductsCount, value);
        }

        private int _lostProductsCount = 0;
        public int LostProductsCount
        {
            get => _lostProductsCount;
            set => SetProperty(ref _lostProductsCount, value);
        }

        private string _familyCode = "Chargement...";
        public string FamilyCode
        {
            get => _familyCode;
            set => SetProperty(ref _familyCode, value);
        }

        private bool _isEditPopupVisible = false;
        public bool IsEditPopupVisible
        {
            get => _isEditPopupVisible;
            set => SetProperty(ref _isEditPopupVisible, value);
        }

        private string _editFirstName;
        public string EditFirstName
        {
            get => _editFirstName;
            set => SetProperty(ref _editFirstName, value);
        }

        private string _editLastName;
        public string EditLastName
        {
            get => _editLastName;
            set => SetProperty(ref _editLastName, value);
        }

        public IAsyncRelayCommand LoadProfileDataCommand { get; }
        public IRelayCommand ToggleSettingsCommand { get; }
        public IAsyncRelayCommand EditProfileCommand { get; }
        public IRelayCommand CancelEditCommand { get; }
        public IAsyncRelayCommand SaveEditCommand { get; }
        public IAsyncRelayCommand DeleteAccountCommand { get; }
        public IAsyncRelayCommand LogoutCommand { get; }
        public IRelayCommand ActivateNotificationsCommand { get; }

        public ProfileViewModel(ContentPage page)
        {
            _page = page;
            _userService = new NeonUserService();
            _productService = new NeonProductService();
            _localUserService = new LocalUserService();
            _localProductService = new LocalProductService();

            LoadProfileDataCommand = new AsyncRelayCommand(LoadProfileDataAsync);
            ToggleSettingsCommand = new RelayCommand(ToggleSettings);
            EditProfileCommand = new AsyncRelayCommand(EditProfileAsync);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SaveEditCommand = new AsyncRelayCommand(SaveEditAsync);
            DeleteAccountCommand = new AsyncRelayCommand(DeleteAccountAsync);
            LogoutCommand = new AsyncRelayCommand(LogoutAsync);
            ActivateNotificationsCommand = new RelayCommand(ActivateNotifications);
        }

        public async Task LoadProfileDataAsync()
        {
            try
            {
                // 1. Charger les infos utilisateur depuis le local d'abord
                var localUser = await _localUserService.LoadUserAsync();
                if (localUser != null)
                {
                    UpdateUI(localUser);
                    Debug.WriteLine("Profil chargé depuis le stockage local \u2705");
                }

                // 2. Charger les produits enregistrés depuis le local
                var localProducts = await _localProductService.LoadProductsAsync();
                RegisteredProductsCount = localProducts.Count;

                // 3. Essayer de rafraîchir depuis Neon si on a une connexion
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    Debug.WriteLine("ProfilePage [ERREUR] : ID utilisateur non trouvé.");
                    return;
                }

                try
                {
                    // On récupère le profil complet depuis NeonDB
                    var userProfile = await _userService.GetUserProfileAsync(userId);
                    int productCount = await _userService.GetRegisteredProductsCountAsync(userId);

                    if (userProfile != null)
                    {
                        // On met à jour le profil
                        userProfile.RegisteredProductsCount = productCount;
                        await _localUserService.SaveUserAsync(userProfile);

                        await SyncLostProductCountAsync(userId, userProfile);

                        UpdateUI(userProfile);
                        Debug.WriteLine("Profil mis à jour depuis Neon");

                        // On synchronise les produits si le serveur en a
                        if (productCount > localProducts.Count)
                        {
                            var serverProducts = await _productService.GetUserProductsAsync(userId);
                            await _localProductService.SaveProductsAsync(serverProducts);
                            RegisteredProductsCount = serverProducts.Count;
                        }

                        Debug.WriteLine(" Produits mis à jour depuis NeonDB \u2705");
                    }
                }
                catch
                {
                    Debug.WriteLine("Mode hors-ligne activé \u2192 utilisation des données locales");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ProfilePage [ERREUR] : {ex.Message}");
            }
        }

        private void UpdateUI(UserProfileDetails user)
        {
            UserName = $"{user.FirstName} {user.LastName}";
            FamilyCode = user.HomeCode.ToString();
            RegisteredProductsCount = user.RegisteredProductsCount;
            LostProductsCount = user.LostProductCount;
        }

        private async Task SyncLostProductCountAsync(int userId, UserProfileDetails serverProfile)
        {
            try
            {
                var localUser = await _localUserService.LoadUserAsync();
                if (localUser == null) return;

                int localCount = localUser.LostProductCount;
                int serverCount = serverProfile.LostProductCount;

                if (localCount != serverCount)
                {
                    Debug.WriteLine($"ProfilePage: Différence détectée - Local: {localCount}, Serveur: {serverCount}");
                    Debug.WriteLine("ProfilePage: Synchronisation du compteur avec le serveur (serveur fait autorité).");

                    localUser.LostProductCount = serverCount;
                    await _localUserService.SaveUserAsync(localUser);
                    LostProductsCount = serverCount;

                    Debug.WriteLine($"ProfilePage: Compteur local mis à jour: {serverCount}");
                }
                else
                {
                    Debug.WriteLine("ProfilePage: Compteurs local et serveur déjà synchronisés.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ProfilePage [SyncLostProductCount] Erreur: {ex.Message}");
            }
        }

        private void ToggleSettings()
        {
            IsMenuVisible = !IsMenuVisible;
            Debug.WriteLine($"Menu de paramètres visible : {IsMenuVisible}");
        }

        private async Task EditProfileAsync()
        {
            IsMenuVisible = false;

            // Charger l'utilisateur depuis le local storage
            _currentUser = await _localUserService.LoadUserAsync();

            if (_currentUser == null)
            {
                await _page.DisplayAlert("Erreur", "Impossible de charger votre profil.", "OK");
                return;
            }

            // Pré-remplir les champs
            EditFirstName = _currentUser.FirstName;
            EditLastName = _currentUser.LastName;

            // Afficher la popup
            IsEditPopupVisible = true;
        }

        private void CancelEdit()
        {
            IsEditPopupVisible = false;
        }

        private async Task SaveEditAsync()
        {
            try
            {
                // Récupérer l'ID utilisateur
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    await _page.DisplayAlert("Erreur", "Impossible de retrouver votre profil.", "OK");
                    return;
                }

                // Mettre à jour les infos en local
                _currentUser.FirstName = EditFirstName?.Trim();
                _currentUser.LastName = EditLastName?.Trim();

                // Sauvegarder sur Neon
                var success = await _userService.UpdateUserProfileAsync(userId, _currentUser);

                if (!success)
                {
                    await _page.DisplayAlert("Erreur", "La mise à jour du profil a échoué.", "OK");
                    return;
                }

                // Sauvegarder aussi en local
                await _localUserService.SaveUserAsync(_currentUser);
                
                // Rafraîchir l'affichage
                UserName = $"{_currentUser.FirstName} {_currentUser.LastName}";

                // Fermer la popup
                IsEditPopupVisible = false;

                await _page.DisplayAlert("Succès", "Votre profil a été mis à jour.", "OK");
            }
            catch (Exception ex)
            {
                await _page.DisplayAlert("Erreur", $"Impossible de sauvegarder : {ex.Message}", "OK");
            }
        }

        private async Task DeleteAccountAsync()
        {
            IsMenuVisible = false;

            bool confirm = await _page.DisplayAlert(
                "Supprimer le compte",
                "⚠️ ATTENTION ⚠️\n\nCette action est irréversible et supprimera définitivement :\n\n" +
                "• Votre profil utilisateur\n" +
                "• Tous vos produits enregistrés\n" +
                "• Votre code foyer et ses données associées\n" +
                "• Toutes vos données locales\n\n" +
                "Voulez-vous vraiment continuer ?",
                "Supprimer définitivement",
                "Annuler"
            );

            if (!confirm) return;

            bool finalConfirm = await _page.DisplayAlert(
                "Dernière confirmation",
                "Êtes-vous absolument certain(e) de vouloir supprimer votre compte ?",
                "Oui, supprimer",
                "Non, annuler"
            );

            if (!finalConfirm) return;

            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    await _page.DisplayAlert("Erreur", "Impossible de retrouver votre profil.", "OK");
                    return;
                }

                Debug.WriteLine($"[ProfilePage] Début de la suppression du compte {userId}...");

                bool deletedFromServer = await _userService.DeleteUserAccountAsync(userId);
                if (!deletedFromServer)
                {
                    await _page.DisplayAlert("Erreur", "Une erreur est survenue lors de la suppression du compte sur le serveur.", "OK");
                    return;
                }

                Debug.WriteLine("[ProfilePage] Données serveur supprimées \u2705");

                _localUserService.ClearUser();
                _localProductService.ClearAllProducts();
                SecureStorage.Remove("user_id");

                Debug.WriteLine("[ProfilePage] Données locales supprimées \u2705");

                await _page.DisplayAlert("Compte supprimé", "Votre compte a été définitivement supprimé.", "OK");

                await Shell.Current.GoToAsync(nameof(StartingPage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePage] Erreur lors de la suppression du compte : {ex.Message}");
                await _page.DisplayAlert("Erreur", $"Une erreur est survenue : {ex.Message}", "OK");
            }
        }

        private async Task LogoutAsync()
        {
            IsMenuVisible = false;

            SecureStorage.Remove("user_id");
            _localUserService.ClearUser();
            Debug.WriteLine("Déconnexion de l'utilisateur. Suppression de l'ID utilisateur.");

            await Shell.Current.GoToAsync(nameof(StartingPage));
        }

        private void ActivateNotifications()
        {
            var popup = new NotificationPopUp();
            _page.ShowPopup(popup);
        }
    }
}