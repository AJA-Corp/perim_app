using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Services;
using perimapp.Views;
using perimapp.Data;
using perimapp.PopUp;
using CommunityToolkit.Maui.Extensions;

namespace perimapp.ViewModels
{
    public partial class LogInViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly ApiProfileService _apiProfileService;
        private readonly LocalUserService _localUserService;

        [ObservableProperty]
        private string _emailText;

        [ObservableProperty]
        private string _passwordText;

        [ObservableProperty]
        private string _homeCodeText;

        public LogInViewModel(AuthService authService, ApiProfileService apiProfileService, LocalUserService localUserService)
        {
            _authService = authService;
            _apiProfileService = apiProfileService;
            _localUserService = localUserService;
        }

        [RelayCommand]
        private async Task NextLogInAsync()
        {
            string email = EmailText?.Trim() ?? "";
            string password = PasswordText ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                var errorPopup = new InfosPopUp("Erreur", "Veuillez entrer votre email et votre mot de passe.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                return;
            }

            bool isLoginSuccessful = await _authService.SignInAsync(email, password);

            if (isLoginSuccessful)
            {
                string pendingCode = Preferences.Get("pending_home_code", "");
                var myProfile = await _apiProfileService.GetOrCreateMyProfileAsync(pendingCode);

                if (myProfile != null)
                {
                    Preferences.Remove("pending_home_code");

                    Preferences.Set("mon_user_id", myProfile.Id);
                    Preferences.Set("mon_home_code", myProfile.HomeCode);

                    await _localUserService.SaveUserAsync(myProfile);
                    await SecureStorage.SetAsync("user_id", myProfile.Id.ToString());

                    AppData.CurrentUser = myProfile;
                    AppData.CurrentUserId = myProfile.Id;

                    if (!myProfile.IsValidated)
                    {
                        await Shell.Current.GoToAsync(nameof(EmailVerificationView));
                    }
                    else
                    {
                        await Shell.Current.GoToAsync($"///{nameof(MainView)}");
                    }
                }
                else
                {
                    SecureStorage.Remove("auth_token");
                    await Shell.Current.CurrentPage.DisplayAlertAsync("Erreur Serveur", "Impossible de récupérer votre profil.", "OK");
                    var errorPopup = new InfosPopUp("Erreur Serveur", "Impossible de récupérer votre profil.", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                }
            }
            else
            {
                var errorPopup = new InfosPopUp("Erreur", "Email ou mot de passe incorrect.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
            }
        }

        [RelayCommand]
        private async Task BackLogInAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}