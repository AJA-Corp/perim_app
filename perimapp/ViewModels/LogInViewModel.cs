using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Services;
using perimapp.Views;
using perimapp.Data;

namespace perimapp.ViewModels
{
    public partial class LogInViewModel : ObservableObject
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
        private string _homeCodeText;

        public LogInViewModel(ContentPage page)
        {
            _page = page;
        }

        [RelayCommand]
        private async Task NextLogInAsync()
        {
            string email = EmailText?.Trim() ?? "";
            string password = PasswordText ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await _page.DisplayAlertAsync("Erreur", "Veuillez entrer votre email et votre mot de passe.", "OK");
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
                    await _page.DisplayAlertAsync("Erreur Serveur", "Impossible de récupérer votre profil.", "OK");
                }
            }
            else
            {
                await _page.DisplayAlertAsync("Erreur", "Email ou mot de passe incorrect.", "OK");
            }
        }

        [RelayCommand]
        private async Task BackLogInAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}