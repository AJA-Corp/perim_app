using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using perimapp.Services;
using perimapp.Views;
using perimapp.Data;

namespace perimapp.ViewModels
{
    public partial class LogInViewModel : ObservableObject
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
        private string _homeCodeText;

        public LogInViewModel(
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
        private async Task NextLogInAsync()
        {
            string email = EmailText?.Trim() ?? "";
            string password = PasswordText ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await _dialogService.ShowAlertAsync("Erreur", "Veuillez entrer votre email et votre mot de passe.", "OK");
                return;
            }

            bool isLoginSuccessful = await _authService.SignInAsync(email, password);

            if (isLoginSuccessful)
            {
                string pendingCode = _preferences.Get("pending_home_code", "");
                var myProfile = await _apiProfileService.GetOrCreateMyProfileAsync(pendingCode);

                if (myProfile != null)
                {
                    _preferences.Remove("pending_home_code");

                    _preferences.Set("mon_user_id", myProfile.Id);
                    _preferences.Set("mon_home_code", myProfile.HomeCode);

                    await _localUserService.SaveUserAsync(myProfile);
                    await _secureStorage.SetAsync("user_id", myProfile.Id.ToString());

                    AppData.CurrentUser = myProfile;
                    AppData.CurrentUserId = myProfile.Id;

                    if (!myProfile.IsValidated)
                    {
                        await _navigationService.GoToAsync(nameof(EmailVerificationView));
                    }
                    else
                    {
                        await _navigationService.GoToAsync($"///{nameof(MainView)}");
                    }
                }
                else
                {
                    _secureStorage.Remove("auth_token");
                    await _dialogService.ShowAlertAsync("Erreur Serveur", "Impossible de récupérer votre profil.", "OK");
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Email ou mot de passe incorrect.", "OK");
            }
        }

        [RelayCommand]
        private async Task BackLogInAsync()
        {
            await _navigationService.GoToAsync("..");
        }
    }
}