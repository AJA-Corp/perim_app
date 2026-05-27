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
        [RelayCommand]
        private async Task LoadAppAsync()
        {
            await Task.Delay(1500); 

            var token = await SecureStorage.GetAsync("auth_token");

            if (!string.IsNullOrEmpty(token))
            {
                await Shell.Current.GoToAsync($"///{nameof(MainView)}");
            }
            else
            {
                await Shell.Current.GoToAsync($"///{nameof(StartingView)}");
            }
        }
    }
}