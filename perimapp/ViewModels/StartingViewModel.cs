using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using perimapp.Pages;

namespace perimapp.ViewModels
{
    public partial class StartingViewModel : ObservableObject
    {
        private readonly ContentPage _page;

        public StartingViewModel(ContentPage page)
        {
            _page = page;
        }

        [RelayCommand]
        private async Task LogInAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(LogInPage));
            }
            catch (Exception error)
            {
                Console.WriteLine("[DEBUG] " + error);
                await _page.DisplayAlert(
                    "Erreur",
                    "Une erreur est survenue lors de la navigation.",
                    "OK"
                );
            }
        }

        [RelayCommand]
        private async Task SignUpAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(SignUpPage));
            }
            catch (Exception error)
            {
                Console.WriteLine("[DEBUG] " + error);
                await _page.DisplayAlert(
                    "Erreur",
                    "Une erreur est survenue lors de la navigation.",
                    "OK"
                );
            }
        }
    }
}