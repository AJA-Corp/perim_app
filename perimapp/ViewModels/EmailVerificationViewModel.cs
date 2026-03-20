using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Views;
using perimapp.Services;

namespace perimapp.ViewModels
{
    public partial class EmailVerificationViewModel : ObservableObject
    {
        private readonly EmailService _emailService = new();
        private readonly LoginVerificationService _verificationService = new();
        private readonly NeonUserService _userService = new();
        private readonly ContentPage _page;
        private System.Timers.Timer _timer;
        private int _remainingSeconds = 300;

        [ObservableProperty]
        private string _sessionId;

        [ObservableProperty]
        private string _infoText;

        [ObservableProperty]
        private string _timerText = "Code expire dans : 5:00";

        [ObservableProperty]
        private Color _timerTextColor = Colors.White;

        [ObservableProperty]
        private string _verificationCode;

        public EmailVerificationViewModel(ContentPage page)
        {
            _page = page;
        }

        partial void OnSessionIdChanged(string value)
        {
            OnSessionIdSet();
        }

        private void OnSessionIdSet()
        {
            if (!string.IsNullOrEmpty(SessionId))
            {
                var session = _verificationService.GetSession(SessionId);
                if (session != null)
                {
                    InfoText = $"Un code de v\u00e9rification a \u00e9t\u00e9 envoy\u00e9 \u00e0 {MaskEmail(session.Email)}.";
                }
                
                StartTimer();
            }
        }

        private void StartTimer()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += UpdateTimer;
            _timer.Start();
        }

        private void UpdateTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            _remainingSeconds--;
            
            _page.Dispatcher.Dispatch(() =>
            {
                if (_remainingSeconds <= 0)
                {
                    _timer?.Stop();
                    TimerText = "Code expir\u00e9";
                    TimerTextColor = Colors.Red;
                }
                else
                {
                    int minutes = _remainingSeconds / 60;
                    int seconds = _remainingSeconds % 60;
                    TimerText = $"Code expire dans : {minutes}:{seconds:D2}";
                }
            });
        }

        [RelayCommand]
        private async Task VerifyAsync()
        {
            var enteredCode = VerificationCode?.Trim();
            
            if (string.IsNullOrWhiteSpace(enteredCode))
            {
                await _page.DisplayAlert("Erreur", "Veuillez entrer le code de v\u00e9rification.", "OK");
                return;
            }

            if (enteredCode.Length != 6)
            {
                await _page.DisplayAlert("Erreur", "Le code doit contenir 6 chiffres.", "OK");
                return;
            }

            var isValid = _verificationService.VerifyCode(SessionId, enteredCode);
            
            if (isValid)
            {
                var session = _verificationService.GetSession(SessionId);
                if (session != null)
                {
                    await SecureStorage.SetAsync("user_id", session.UserId.ToString());
                    _verificationService.CompleteVerification(SessionId);
                    
                    _timer?.Stop();
                    
                    await Shell.Current.GoToAsync(nameof(MainView));
                }
                else
                {
                    await _page.DisplayAlert("Erreur", "Session expir\u00e9e. Veuillez vous reconnecter.", "OK");
                    await Shell.Current.GoToAsync(nameof(LogInView));
                }
            }
            else
            {
                await _page.DisplayAlert("Erreur", "Code de v\u00e9rification incorrect ou expir\u00e9.", "OK");
            }
        }

        [RelayCommand]
        private async Task ResendCodeAsync()
        {
            var session = _verificationService.GetSession(SessionId);
            if (session == null)
            {
                await _page.DisplayAlert("Erreur", "Session expir\u00e9e. Veuillez vous reconnecter.", "OK");
                await Shell.Current.GoToAsync(nameof(LogInView));
                return;
            }

            var userProfile = await _userService.GetUserProfileAsync(session.UserId);
            if (userProfile == null)
            {
                await _page.DisplayAlert("Erreur", "Utilisateur introuvable.", "OK");
                return;
            }

            var newSessionId = await _verificationService.CreateVerificationSessionAsync(
                session.UserId, 
                session.Email, 
                session.LoginMethod);

            var newSession = _verificationService.GetSession(newSessionId);
            
            var emailSent = await _emailService.SendLoginConfirmationEmailAsync(session.Email, newSession.VerificationCode);
            
            if (emailSent)
            {
                SessionId = newSessionId;
                _remainingSeconds = 300;
                TimerText = "Code expire dans : 5:00";
                TimerTextColor = Colors.White;
                await _page.DisplayAlert("Succ\u00e8s", "Un nouveau code a \u00e9t\u00e9 envoy\u00e9 \u00e0 votre email.", "OK");
            }
            else
            {
                await _page.DisplayAlert("Erreur", "Impossible d'envoyer l'email. V\u00e9rifiez votre configuration email.", "OK");
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            _timer?.Stop();
            await Shell.Current.GoToAsync(nameof(LogInView));
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;

            var parts = email.Split('@');
            var localPart = parts[0];
            var domain = parts[1];

            if (localPart.Length <= 2)
                return email;

            var maskedLocal = localPart[0] + new string('*', localPart.Length - 2) + localPart[^1];
            return $"{maskedLocal}@{domain}";
        }

        public void StopTimer()
        {
            _timer?.Stop();
        }
    }
}