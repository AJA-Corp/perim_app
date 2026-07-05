using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using perimapp.Services;
using perimapp.Views;

namespace perimapp.ViewModels
{
    public partial class StartingViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public StartingViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private async Task LogInAsync()
        {
            try
            {
                await _navigationService.GoToAsync(nameof(LogInView));
            }
            catch (Exception error)
            {
                Console.WriteLine("[DEBUG] " + error);
                await _dialogService.ShowAlertAsync("Erreur", "Une erreur est survenue lors de la navigation. Veuillez réessayer.", "OK");
            }
        }

        [RelayCommand]
        private async Task SignUpAsync()
        {
            try
            {
                await _navigationService.GoToAsync(nameof(SignUpView));
            }
            catch (Exception error)
            {
                Console.WriteLine("[DEBUG] " + error);
                await _dialogService.ShowAlertAsync("Erreur", "Une erreur est survenue lors de la navigation. Veuillez réessayer.", "OK");
            }
        }
    }
}