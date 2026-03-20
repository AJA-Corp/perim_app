using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Services;
using perimapp.Pages;

namespace perimapp.ViewModels
{
    public partial class LogInViewModel : ObservableObject
    {
        private readonly NeonUserService _userService = new();
        private readonly EmailService _emailService = new();
        private readonly LoginVerificationService _verificationService = new();
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
            string homeCodeText = HomeCodeText?.Trim() ?? "";
            string userEmail = "";
            string loginMethod = "";
            int userId = -1;

            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
            {
                userId = await _userService.AuthenticateUserAsync(email, password);
                userEmail = email;
                loginMethod = "email";
            }
            else if (!string.IsNullOrWhiteSpace(homeCodeText))
            {
                if (int.TryParse(homeCodeText, out int homeCode))
                {
                    userId = await _userService.AuthenticateByHomeCodeAsync(homeCode);
                    if (userId > 0)
                    {
                        userEmail = await _userService.GetUserEmailAsync(userId);
                    }
                    loginMethod = "homecode";
                }
                else
                {
                    await _page.DisplayAlert("Erreur", "Le code foyer est invalide. Veuillez entrer un nombre.", "OK");
                    return;
                }
            }
            else
            {
                await _page.DisplayAlert("Erreur", "Email ou mot de passe incorrect.", "OK");
                return;
            }

            if (userId > 0 && !string.IsNullOrWhiteSpace(userEmail))
            {
                var sessionId = await _verificationService.CreateVerificationSessionAsync(userId, userEmail, loginMethod);
                var session = _verificationService.GetSession(sessionId);
                
                var emailSent = await _emailService.SendLoginConfirmationEmailAsync(userEmail, session.VerificationCode);
                
                if (emailSent)
                {
                    await Shell.Current.GoToAsync($"{nameof(EmailVerificationPage)}?sessionId={sessionId}");
                }
                else
                {
                    await _page.DisplayAlert("Erreur", "Impossible d'envoyer l'email de confirmation. V\u00e9rifiez votre configuration email dans EmailConfig.cs", "OK");
                }
            }
            else
            {
                await _page.DisplayAlert("Erreur", "Identifiants ou code foyer incorrect.", "OK");
            }
        }

        [RelayCommand]
        private async Task BackLogInAsync()
        {
            await Shell.Current.GoToAsync(nameof(StartingPage));
        }
    }
}