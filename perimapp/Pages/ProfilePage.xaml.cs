using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using perimapp.Models;
using perimapp.PopUp;
using perimapp.Services;
using System.Diagnostics;

namespace perimapp.Pages
{
    public partial class ProfilePage : ContentPage, INotifyPropertyChanged
    {
        private bool _isMenuVisible = false;
        private string _userName;
        private int _registeredProductsCount;
        private int _lostProductsCount;
        private string _familyCode;
        // Propriété pour gérer la visibilité du menu de déconnexion
        
        //pour la bdd (Neon)
        private readonly NeonUserService _userService;
        private NeonProductService _productService;
        //pour le Local
        private readonly LocalUserService _localUserService;
        //pour les produits enregistrer sur profilepage 
        private readonly LocalProductService _localProductService;
        private UserProfileDetails? _currentUser;


        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set
            {
                if (_isMenuVisible != value)
                {
                    _isMenuVisible = value;
                    OnPropertyChanged();
                }
            }
        }


        public string UserName { get => _userName; set { if (_userName != value) { _userName = value; OnPropertyChanged(); } } }
        public int RegisteredProductsCount { get => _registeredProductsCount; set { if (_registeredProductsCount != value) { _registeredProductsCount = value; OnPropertyChanged(); } } }
        public int LostProductsCount { get => _lostProductsCount; set { if (_lostProductsCount != value) { _lostProductsCount = value; OnPropertyChanged(); } } }
        public string FamilyCode { get => _familyCode; set { if (_familyCode != value) { _familyCode = value; OnPropertyChanged(); } } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProfilePage()
        {
            InitializeComponent();
            _userService = new NeonUserService();
            _productService = new NeonProductService();
            _localUserService = new LocalUserService(); //local
            _localProductService = new LocalProductService();//pour les produits 


            UserName = "Chargement...";
            FamilyCode = "Chargement...";
            RegisteredProductsCount = 0;
            LostProductsCount = 0;

            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadProfileDataAsync();
        }

        private async Task LoadProfileDataAsync()
{
    try
    {
        // 1. Charger les infos utilisateur depuis le local d'abord
        var localUser = await _localUserService.LoadUserAsync();
        if (localUser != null)
        {
            UpdateUI(localUser);
            Debug.WriteLine("Profil chargé depuis le stockage local ✅");
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
                UpdateUI(userProfile);
                Debug.WriteLine("Profil mis à jour depuis Neon");

                // On synchronise les produits si le serveur en a
                if (productCount > localProducts.Count)
                {
                    var serverProducts = await _productService.GetUserProductsAsync(userId);
                    await _localProductService.SaveProductsAsync(serverProducts);
                    RegisteredProductsCount = serverProducts.Count;
                }

                Debug.WriteLine(" Produits mis à jour depuis NeonDB ✅");
            }
        }
        catch
        {
            Debug.WriteLine("Mode hors-ligne activé → utilisation des données locales");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"ProfilePage [ERREUR] : {ex.Message}");
    }
}

        //pour le localUser
        private void UpdateUI(UserProfileDetails user)
        {
            UserName = $"{user.FirstName} {user.LastName}";
            FamilyCode = user.HomeCode.ToString();
            RegisteredProductsCount = user.RegisteredProductsCount;
            LostProductsCount = string.IsNullOrEmpty(user.LostProducts) ? 0 : user.LostProducts.Split(',').Length;
        }

        private void SetDefaultProfileValues(string defaultName)
        {
            UserName = defaultName;
            FamilyCode = "N/A";
            RegisteredProductsCount = 0;
            LostProductsCount = 0;
        }

        private void OnActivateNotificationsClicked(object sender, EventArgs e)
        {
            var popup = new NotificationPopUp();
            this.ShowPopup(popup);
        }

        // Gère l'événement du clic sur le bouton des paramètres pour afficher/masquer le menu
        private void OnSettingsClicked(object sender, EventArgs e)
        {
            IsMenuVisible = !IsMenuVisible;
            Debug.WriteLine($"Menu de paramètres visible : {IsMenuVisible}");
        }

        // Gère l'événement du clic sur le bouton de déconnexion
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // 1. Masquer le menu
            IsMenuVisible = false;

            // 2. Nettoyer l'état de l'utilisateur (supprimer les informations de session) et supprime ID de lutilisateur et le profile en local 
            SecureStorage.Remove("user_id");
            _localUserService.DeleteUser();
            Debug.WriteLine("Déconnexion de l'utilisateur. Suppression de l'ID utilisateur.");

            // 3. Rediriger l'utilisateur vers la page de connexion
            // Assurez-vous que la route vers la page de connexion est bien définie dans votre AppShell.xaml
            await Shell.Current.GoToAsync(nameof(StartingPage));
            
        }

            // Quand on clique sur "Modifier mon profil"
        private async void OnEditProfileClicked(object sender, EventArgs e)
        {
            IsMenuVisible = false;

            // Charger l'utilisateur depuis le local storage
            _currentUser = await _localUserService.LoadUserAsync();

            if (_currentUser == null)
            {
                await DisplayAlert("Erreur", "Impossible de charger votre profil.", "OK");
                return;
            }

            // Pré-remplir les champs
            FirstNameEntry.Text = _currentUser.FirstName;
            LastNameEntry.Text = _currentUser.LastName;

            // Afficher la popup
            EditProfilePopup.IsVisible = true;
        }

        // Si on annule la modification
        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            EditProfilePopup.IsVisible = false;
        }

        // Quand on clique sur "Enregistrer"
        private async void OnSaveEditClicked(object sender, EventArgs e)
        {
            try
            {
                // Récupérer l'ID utilisateur
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    await DisplayAlert("Erreur", "Impossible de retrouver votre profil.", "OK");
                    return;
                }

                // Mettre à jour les infos en local
                _currentUser.FirstName = FirstNameEntry.Text?.Trim();
                _currentUser.LastName = LastNameEntry.Text?.Trim();

                // Sauvegarder sur Neon
              var success =  await _userService.UpdateUserProfileAsync(userId, _currentUser);

                if (!success)
                {
                    await DisplayAlert("Erreur", "La mise à jour du profil a échoué." ,"OK");
                    return;
                }

                // Sauvegarder aussi en local
                    await _localUserService.SaveUserAsync(_currentUser);
                // Rafraîchir l'affichage
                UserName = $"{_currentUser.FirstName} {_currentUser.LastName}";

                // Fermer la popup
                EditProfilePopup.IsVisible = false;

                await DisplayAlert("Succès", "Votre profil a été mis à jour.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de sauvegarder : {ex.Message}", "OK");
            }
        }
        
    }
}