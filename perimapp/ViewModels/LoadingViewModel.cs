using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Views;

namespace perimapp.ViewModels
{
    public partial class LoadingViewModel : ObservableObject
    {
        public LoadingViewModel()
        {
        }

        [RelayCommand]
        public async Task LoadAppAsync()
        {
            string savedUserIdString = await SecureStorage.GetAsync("user_id");

            Console.WriteLine($"[DEBUG] ID utilisateur récupéré depuis SecureStorage : {savedUserIdString}");

            await Task.Delay(5000); 

            if (!string.IsNullOrEmpty(savedUserIdString))
            {
                await Shell.Current.GoToAsync(nameof(MainView));
            }
            else
            {
                await Shell.Current.GoToAsync(nameof(StartingView));
            }
        }
    }
}