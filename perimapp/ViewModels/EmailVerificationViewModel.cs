using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using perimapp.Views;
using perimapp.Services;

namespace perimapp.ViewModels
{
    public partial class EmailVerificationViewModel : ObservableObject
    {
        private readonly ApiProfileService _apiProfileService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IDispatcherService _dispatcherService;
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

        public EmailVerificationViewModel(
            ApiProfileService apiProfileService,
            INavigationService navigationService,
            IDialogService dialogService,
            IDispatcherService dispatcherService)
        {
            _apiProfileService = apiProfileService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _dispatcherService = dispatcherService;
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

            _dispatcherService.BeginInvokeOnMainThread(() =>
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
                await _dialogService.ShowAlertAsync("Code invalide", "Veuillez entrer un code de validation à 6 chiffres.", "OK");
                return;
            }

            bool success = await _apiProfileService.ValidateHomeJoinCodeAsync(enteredCode);

            if (success)
            {
                _timer?.Stop();
                await _dialogService.ShowAlertAsync("Bienvenue !", "Vous avez rejoint le foyer avec succès. Vous pouvez maintenant accéder à toutes les fonctionnalités de l'application.", "OK");
                await _navigationService.GoToAsync($"///{nameof(MainView)}");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Code incorrect", "Le code de validation que vous avez entré est incorrect. Veuillez vérifier le code reçu par le propriétaire du foyer et réessayer.", "OK");
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            _timer?.Stop();
            await _navigationService.GoToAsync("..");
        }

        public void StopTimer()
        {
            _timer?.Stop();
        }
    }
}