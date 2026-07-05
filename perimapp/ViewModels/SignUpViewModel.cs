using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using perimapp.Models;
using perimapp.Views;
using perimapp.Services;
using perimapp.Data;

namespace perimapp.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly ApiProfileService _apiProfileService;
        private readonly LocalUserService _localUserService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ISecureStorage _secureStorage;
        private readonly IPreferences _preferences;

        [ObservableProperty]
        private string _emailText;

        [ObservableProperty]
        private string _passwordText;

        [ObservableProperty]
        private string _confirmPasswordText;

        [ObservableProperty]
        private string _homeCodeText;

        public SignUpViewModel(
            AuthService authService, 
            ApiProfileService apiProfileService, 
            LocalUserService localUserService,
            INavigationService navigationService,
            IDialogService dialogService,
            ISecureStorage secureStorage,
            IPreferences preferences)
        {
            _authService = authService;
            _apiProfileService = apiProfileService;
            _localUserService = localUserService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _secureStorage = secureStorage;
            _preferences = preferences;
        }

        [RelayCommand]
        private async Task NextSignUpAsync()
        {
            string email = EmailText?.Trim() ?? "";
            string password = PasswordText ?? "";
            string confirmPassword = ConfirmPasswordText ?? "";
            string codeATester = HomeCodeText?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                await _dialogService.ShowAlertAsync("Erreur", "Tous les champs doivent être remplis.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await _dialogService.ShowAlertAsync("Erreur", "Les mots de passe ne correspondent pas.", "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(codeATester))
            {
                bool codeExists = await _apiProfileService.CheckHomeCodeExistsAsync(codeATester);

                if (!codeExists)
                {
                    await _dialogService.ShowAlertAsync("Erreur", "Ce code foyer est introuvable. Vérifiez-le et réessayez.", "OK");
                    return;
                }
            }

            bool isRegistered = await _authService.SignUpAsync(email, password, "Nouvel", "Utilisateur");

            if (isRegistered)
            {
                _preferences.Set("pending_home_code", codeATester);
                var myProfile = await _apiProfileService.GetOrCreateMyProfileAsync(codeATester);

                if (myProfile != null)
                {
                    _preferences.Set("mon_user_id", myProfile.Id);
                    _preferences.Set("mon_home_code", myProfile.HomeCode);

                    await _localUserService.SaveUserAsync(myProfile);
                    await _secureStorage.SetAsync("user_id", myProfile.Id.ToString());

                    AppData.CurrentUser = myProfile;
                    AppData.CurrentUserId = myProfile.Id;

                    if (!myProfile.IsValidated)
                    {
                        await _dialogService.ShowAlertAsync("Validation en attente", "Votre compte est en attente de validation par le propriétaire du foyer. Vous serez redirigé vers la page de vérification par email.", "OK");
                        await _navigationService.GoToAsync(nameof(EmailVerificationView));
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Succès", "Votre compte a été créé et validé avec succès ! Vous allez être redirigé vers la page d'accueil.", "OK");
                        await _navigationService.GoToAsync($"///{nameof(MainView)}");
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Erreur", "Problème lors de la synchronisation du profil. Veuillez réessayer plus tard.", "OK");
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "L'inscription a échoué. Cet email est peut-être déjà utilisé.", "OK");
            }
        }

        [RelayCommand]
        private async Task BackSignUpAsync()
        {
            await _navigationService.GoToAsync("..");
        }
    }
}