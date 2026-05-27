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

namespace perimapp.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly AuthService _authService = new();
        private readonly ApiProfileService _apiProfileService = new();
        private readonly LocalUserService _localUserService = new();

        private readonly ContentPage _page;

        [ObservableProperty]
        private string _emailText;

        [ObservableProperty]
        private string _passwordText;

        [ObservableProperty]
        private string _confirmPasswordText;

        [ObservableProperty]
        private string _homeCodeText;

        public SignUpViewModel(ContentPage page)
        {
            _page = page;
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
                await _page.DisplayAlertAsync("Erreur", "Tous les champs doivent être remplis.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await _page.DisplayAlertAsync("Erreur", "Les mots de passe ne correspondent pas.", "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(codeATester))
            {
                bool codeExists = await _apiProfileService.CheckHomeCodeExistsAsync(codeATester);

                if (!codeExists)
                {
                    await _page.DisplayAlertAsync("Erreur", "Ce code foyer est introuvable. Vérifiez-le et réessayez.", "OK");
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
                        await _page.DisplayAlertAsync("Validation", "Un code a été envoyé au propriétaire du foyer.", "OK");
                        await Shell.Current.GoToAsync(nameof(EmailVerificationView));
                    }
                    else
                    {
                        await _page.DisplayAlertAsync("Succès", "Foyer créé avec succès !", "OK");
                        await Shell.Current.GoToAsync($"///{nameof(MainView)}");
                    }
                }
                else
                {
                    await _page.DisplayAlertAsync("Erreur", "Problème lors de la synchronisation du profil.", "OK");
                }
            }
            else
            {
                await _page.DisplayAlertAsync("Erreur", "L'inscription a échoué. Cet email est peut-être déjà utilisé.", "OK");
            }
        }

        [RelayCommand]
        private async Task BackSignUpAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}