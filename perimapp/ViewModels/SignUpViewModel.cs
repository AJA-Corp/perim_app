using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Models;
using perimapp.Views;
using perimapp.Services;
using perimapp.Data;
using perimapp.PopUp;
using CommunityToolkit.Maui.Extensions;

namespace perimapp.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly ApiProfileService _apiProfileService;
        private readonly LocalUserService _localUserService;


        [ObservableProperty]
        private string _emailText;

        [ObservableProperty]
        private string _passwordText;

        [ObservableProperty]
        private string _confirmPasswordText;

        [ObservableProperty]
        private string _homeCodeText;

        public SignUpViewModel(AuthService authService, ApiProfileService apiProfileService, LocalUserService localUserService)
        {
            _authService = authService;
            _apiProfileService = apiProfileService;
            _localUserService = localUserService;
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
                var errorPopup = new InfosPopUp("Erreur", "Tous les champs doivent être remplis.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                return;
            }

            if (password != confirmPassword)
            {
                var errorPopup = new InfosPopUp("Erreur", "Les mots de passe ne correspondent pas.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                return;
            }

            if (!string.IsNullOrWhiteSpace(codeATester))
            {
                bool codeExists = await _apiProfileService.CheckHomeCodeExistsAsync(codeATester);

                if (!codeExists)
                {
                    var errorPopup = new InfosPopUp("Erreur", "Ce code foyer est introuvable. Vérifiez-le et réessayez.", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                    return;
                }
            }

            bool isRegistered = await _authService.SignUpAsync(email, password, "Nouvel", "Utilisateur");

            if (isRegistered)
            {
                Preferences.Set("pending_home_code", codeATester);
                var myProfile = await _apiProfileService.GetOrCreateMyProfileAsync(codeATester);

                if (myProfile != null)
                {
                    Preferences.Set("mon_user_id", myProfile.Id);
                    Preferences.Set("mon_home_code", myProfile.HomeCode);

                    await _localUserService.SaveUserAsync(myProfile);
                    await SecureStorage.SetAsync("user_id", myProfile.Id.ToString());

                    AppData.CurrentUser = myProfile;
                    AppData.CurrentUserId = myProfile.Id;

                    if (!myProfile.IsValidated)
                    {
                        var verificationPopup = new InfosPopUp("Validation en attente", "Votre compte est en attente de validation par le propriétaire du foyer. Vous serez redirigé vers la page de vérification par email.", "OK");
                        await Shell.Current.CurrentPage.ShowPopupAsync(verificationPopup);
                        await Shell.Current.GoToAsync(nameof(EmailVerificationView));
                    }
                    else
                    {
                        var successPopup = new InfosPopUp("Succès", "Votre compte a été créé et validé avec succès ! Vous allez être redirigé vers la page d'accueil.", "OK");
                        await Shell.Current.CurrentPage.ShowPopupAsync(successPopup);
                        await Shell.Current.GoToAsync($"///{nameof(MainView)}");
                    }
                }
                else
                {
                    var errorPopup = new InfosPopUp("Erreur", "Problème lors de la synchronisation du profil. Veuillez réessayer plus tard.", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                }
            }
            else
            {
                var errorPopup = new InfosPopUp("Erreur", "L'inscription a échoué. Cet email est peut-être déjà utilisé.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
            }
        }

        [RelayCommand]
        private async Task BackSignUpAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}