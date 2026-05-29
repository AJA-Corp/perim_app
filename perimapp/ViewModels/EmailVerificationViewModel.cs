using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Views;
using perimapp.Services;
using System.Net.Http;
using System.Net.Http.Headers;

namespace perimapp.ViewModels
{
    public partial class EmailVerificationViewModel : ObservableObject
    {
        private readonly ApiProfileService _apiProfileService = new();
        private readonly ContentPage _page;
        private System.Timers.Timer _timer;
        private int _remainingSeconds = 300;

        [ObservableProperty]
        private string _infoText = "Veuillez entrer le code de validation reçu par le propriétaire du foyer.";

        [ObservableProperty]
        private string _timerText = "Le code expire bientôt";

        [ObservableProperty]
        private Color _timerTextColor = Colors.White;

        [ObservableProperty]
        private string _verificationCode;

        public EmailVerificationViewModel(ContentPage page)
        {
            _page = page;
            StartTimer();
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
                    TimerText = "Code possiblement expiré";
                    TimerTextColor = Colors.Red;
                }
                else
                {
                    int minutes = _remainingSeconds / 60;
                    int seconds = _remainingSeconds % 60;
                    TimerText = $"Temps estimé : {minutes}:{seconds:D2}";
                }
            });
        }

        [RelayCommand]
        private async Task VerifyAsync()
        {
            var enteredCode = VerificationCode?.Trim();

            if (string.IsNullOrWhiteSpace(enteredCode) || enteredCode.Length != 6)
            {
                await _page.DisplayAlert("Erreur", "Veuillez entrer un code valide à 6 chiffres.", "OK");
                return;
            }

            bool success = await _apiProfileService.ValidateHomeJoinCodeAsync(enteredCode);

            if (success)
            {
                _timer?.Stop();
                await _page.DisplayAlert("Succès", "Vous avez rejoint le foyer !", "OK");
                await Shell.Current.GoToAsync($"///{nameof(MainView)}");
            }
            else
            {
                await _page.DisplayAlert("Erreur", "Code incorrect ou demande introuvable.", "OK");
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            _timer?.Stop();
            await Shell.Current.GoToAsync("..");
        }

        public void StopTimer()
        {
            _timer?.Stop();
        }
    }
}