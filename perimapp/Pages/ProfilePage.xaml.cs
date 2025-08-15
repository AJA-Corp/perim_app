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
        // Propriété pour gérer la visibilité du menu de déconnexion
        private bool _isMenuVisible = false;
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

        private string _userName;
        private int _registeredProductsCount;
        private int _lostProductsCount;
        private string _familyCode;
        
        private readonly NeonUserService _userService;

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
                // ... (Logique pour récupérer l'ID de l'utilisateur) ...
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    Debug.WriteLine("ProfilePage [ERREUR] : ID utilisateur non trouvé. L'utilisateur est-il connecté ?");
                    SetDefaultProfileValues("Utilisateur non connecté");
                    return;
                }

                // 1. Appel pour obtenir les informations de base du profil
                var userProfile = await _userService.GetUserProfileAsync(userId);
        
                // 2. Appel pour obtenir le nombre de produits enregistrés
                int productCount = await _userService.GetRegisteredProductsCountAsync(userId);

                if (userProfile != null)
                {
                    UserName = $"{userProfile.FirstName} {userProfile.LastName}";
                    FamilyCode = userProfile.HomeCode.ToString();
                    RegisteredProductsCount = productCount; // Affectez le résultat du second appel ici
                    LostProductsCount = 0;

                    Debug.WriteLine($"ProfilePage: Profil de l'utilisateur ID {userId} chargé depuis NeonDB.");
                }
                else
                {
                    SetDefaultProfileValues("Profil introuvable");
                    Debug.WriteLine($"ProfilePage: Profil pour l'utilisateur ID {userId} non trouvé dans la base de données.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ProfilePage [ERREUR] : Une erreur inattendue est survenue : {ex.Message}");
                SetDefaultProfileValues("Erreur de chargement");
            }
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

            // 2. Nettoyer l'état de l'utilisateur (supprimer les informations de session)
            SecureStorage.Remove("user_id");
            Debug.WriteLine("Déconnexion de l'utilisateur. Suppression de l'ID utilisateur.");

            // 3. Rediriger l'utilisateur vers la page de connexion
            // Assurez-vous que la route vers la page de connexion est bien définie dans votre AppShell.xaml
            await Shell.Current.GoToAsync(nameof(StartingPage));
        }
    }
}