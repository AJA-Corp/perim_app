using perimapp.Services;
using System;
using System.Threading.Tasks;

namespace perimapp.Pages
{
    [QueryProperty(nameof(SessionId), "sessionId")]
    public partial class EmailVerificationPage : ContentPage
    {
        private readonly EmailService _emailService = new();
        private readonly LoginVerificationService _verificationService = new();
        private readonly NeonUserService _userService = new();
        private string _sessionId;
        private System.Timers.Timer _timer;
        private int _remainingSeconds = 300; // 5 minutes

        public string SessionId
        {
            get => _sessionId;
            set
            {
                _sessionId = value;
                OnSessionIdSet();
            }
        }

        public EmailVerificationPage()
        {
            InitializeComponent();
        }

        private void OnSessionIdSet()
        {
            if (!string.IsNullOrEmpty(_sessionId))
            {
                var session = _verificationService.GetSession(_sessionId);
                if (session != null)
                {
                    InfoLabel.Text = $"Un code de vérification a été envoyé à {MaskEmail(session.Email)}.";
                }
                
                StartTimer();
            }
        }

        private void StartTimer()
        {
            _timer = new System.Timers.Timer(1000); // Update every second
            _timer.Elapsed += UpdateTimer;
            _timer.Start();
        }

        private void UpdateTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            _remainingSeconds--;
            
            Dispatcher.Dispatch(() =>
            {
                if (_remainingSeconds <= 0)
                {
                    _timer?.Stop();
                    TimerLabel.Text = "Code expiré";
                    TimerLabel.TextColor = Colors.Red;
                }
                else
                {
                    int minutes = _remainingSeconds / 60;
                    int seconds = _remainingSeconds % 60;
                    TimerLabel.Text = $"Code expire dans : {minutes}:{seconds:D2}";
                }
            });
        }

        private async void OnVerifyClicked(object sender, EventArgs e)
        {
            var enteredCode = VerificationCodeEntry.Text?.Trim();
            
            if (string.IsNullOrWhiteSpace(enteredCode))
            {
                await DisplayAlert("Erreur", "Veuillez entrer le code de vérification.", "OK");
                return;
            }

            if (enteredCode.Length != 6)
            {
                await DisplayAlert("Erreur", "Le code doit contenir 6 chiffres.", "OK");
                return;
            }

            var isValid = _verificationService.VerifyCode(_sessionId, enteredCode);
            
            if (isValid)
            {
                var session = _verificationService.GetSession(_sessionId);
                if (session != null)
                {
                    // Store user ID for the session
                    await SecureStorage.SetAsync("user_id", session.UserId.ToString());
                    
                    // Complete verification
                    _verificationService.CompleteVerification(_sessionId);
                    
                    // Stop timer
                    _timer?.Stop();
                    
                    // Navigate to main page
                    await Shell.Current.GoToAsync(nameof(MainPage));
                }
                else
                {
                    await DisplayAlert("Erreur", "Session expirée. Veuillez vous reconnecter.", "OK");
                    await Shell.Current.GoToAsync(nameof(LogInPage));
                }
            }
            else
            {
                await DisplayAlert("Erreur", "Code de vérification incorrect ou expiré.", "OK");
            }
        }

        private async void OnResendCodeClicked(object sender, EventArgs e)
        {
            var session = _verificationService.GetSession(_sessionId);
            if (session == null)
            {
                await DisplayAlert("Erreur", "Session expirée. Veuillez vous reconnecter.", "OK");
                await Shell.Current.GoToAsync(nameof(LogInPage));
                return;
            }

            // Get user email from database
            var userProfile = await _userService.GetUserProfileAsync(session.UserId);
            if (userProfile == null)
            {
                await DisplayAlert("Erreur", "Utilisateur introuvable.", "OK");
                return;
            }

            // Create new verification session
            var newSessionId = await _verificationService.CreateVerificationSessionAsync(
                session.UserId, 
                session.Email, 
                session.LoginMethod);

            var newSession = _verificationService.GetSession(newSessionId);
            
            // Send new email
            var emailSent = await _emailService.SendLoginConfirmationEmailAsync(session.Email, newSession.VerificationCode);
            
            if (emailSent)
            {
                _sessionId = newSessionId;
                _remainingSeconds = 300; // Reset timer
                await DisplayAlert("Succès", "Un nouveau code a été envoyé à votre email.", "OK");
            }
            else
            {
                await DisplayAlert("Erreur", "Impossible d'envoyer l'email. Vérifiez votre configuration email.", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            _timer?.Stop();
            await Shell.Current.GoToAsync(nameof(LogInPage));
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _timer?.Stop();
        }
    }
}