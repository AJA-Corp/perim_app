using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Models;
using perimapp.Pages;
using perimapp.Services;

namespace perimapp.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly NeonUserService _userService = new();
        private readonly ContentPage _page;

        [ObservableProperty]
        private string _emailText;

        [ObservableProperty]
        private string _passwordText;

        [ObservableProperty]
        private string _confirmPasswordText;

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

            int homeCode = GenerateRandomHomeCode();

            if (
                string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(confirmPassword)
            )
            {
                await _page.DisplayAlert("Erreur", "Tous les champs doivent \u00eatre remplis.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await _page.DisplayAlert("Erreur", "Les mots de passe ne correspondent pas.", "OK");
                return;
            }

            var newUser = new UserProfileDetails
            {
                Email = email,
                Password = password,
                HomeCode = homeCode,
            };

            int userId = await _userService.RegisterUserAsync(newUser);

            if (userId > 0)
            {
                await SecureStorage.SetAsync("user_id", userId.ToString());

                await _page.DisplayAlert("Succ\u00e8s", "Inscription r\u00e9ussie !", "OK");
                await Shell.Current.GoToAsync(nameof(MainPage));
            }
            else if (userId == -2)
            {
                await _page.DisplayAlert("Erreur", "Cet email est d\u00e9j\u00e0 utilis\u00e9.", "OK");
            }
            else
            {
                await _page.DisplayAlert(
                    "Erreur",
                    "Une erreur s'est produite lors de l'inscription.",
                    "OK"
                );
            }
        }

        [RelayCommand]
        private async Task BackSignUpAsync()
        {
            await Shell.Current.GoToAsync(nameof(StartingPage));
        }

        [RelayCommand]
        private async Task GotoHomeCodeAsync()
        {
            await Shell.Current.GoToAsync(nameof(LogInPage));
        }

        private int GenerateRandomHomeCode()
        {
            Random random = new();
            return random.Next(100000, 1000000);
        }
    }
}