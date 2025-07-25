using System;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;

namespace perimapp.Pages
{
    public partial class SignUpPage : ContentPage
    {
        private readonly NeonUserService _userService = new();

        public SignUpPage()
        {
            InitializeComponent();
        }

        private async void OnNextSignUpClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text?.Trim() ?? "";
            string password = PasswordEntry.Text ?? "";
            string confirmPassword = ConfirmPasswordEntry.Text ?? "";
            string homeCode = CodeFoyerEntry.Text?.Trim() ?? "";

            if (
                string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(confirmPassword)
                || string.IsNullOrWhiteSpace(homeCode)
            )
            {
                await DisplayAlert("Erreur", "Tous les champs doivent être remplis.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Erreur", "Les mots de passe ne correspondent pas.", "OK");
                return;
            }

            var newUser = new UserProfile
            {
                Email = email,
                Password = password,
                HomeCode = Convert.ToInt32(homeCode),
            };

            int userId = await _userService.RegisterUserAsync(newUser);

            if (userId > 0)
            {
                AppData.CurrentUserId = userId;
                await DisplayAlert("Succès", "Inscription réussie !", "OK");
                await Shell.Current.GoToAsync(nameof(MainPage));
            }
            else if (userId == -2)
            {
                await DisplayAlert("Erreur", "Cet email est déjà utilisé.", "OK");
            }
            else
            {
                await DisplayAlert(
                    "Erreur",
                    "Une erreur s'est produite lors de l'inscription.",
                    "OK"
                );
            }
        }

        private async void OnBackSignUpClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(StartingPage));
        }
    }
}
