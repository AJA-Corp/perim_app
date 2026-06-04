using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using perimapp.Views;
using perimapp.PopUp;
using CommunityToolkit.Maui.Extensions;

namespace perimapp.ViewModels
{
    public partial class StartingViewModel : ObservableObject
    {
        public StartingViewModel()
        {
        }

        [RelayCommand]
        private async Task LogInAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(LogInView));
            }
            catch (Exception error)
            {
                Console.WriteLine("[DEBUG] " + error);
                var errorPopup = new InfosPopUp("Erreur", "Une erreur est survenue lors de la navigation. Veuillez réessayer.", "OK");
                await Shell.Current.ShowPopupAsync(errorPopup);
            }
        }

        [RelayCommand]
        private async Task SignUpAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(SignUpView));
            }
            catch (Exception error)
            {
                Console.WriteLine("[DEBUG] " + error);
                var errorPopup = new InfosPopUp("Erreur", "Une erreur est survenue lors de la navigation. Veuillez réessayer.", "OK");
                await Shell.Current.ShowPopupAsync(errorPopup);
            }
        }
    }
}