using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using perimapp.Services;
using perimapp.Views;

namespace perimapp.ViewModels
{
    public partial class LoadingViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly LocalUserService _localUserService;
        private readonly ISecureStorage _secureStorage;

        public LoadingViewModel(
            INavigationService navigationService,
            LocalUserService localUserService,
            ISecureStorage secureStorage)
        {
            _navigationService = navigationService;
            _localUserService = localUserService;
            _secureStorage = secureStorage;
        }

        [RelayCommand]
        private async Task LoadAppAsync()
        {
            await Task.Delay(1500); 

            string? token = null;
            try 
            { 
                token = await _secureStorage.GetAsync("auth_token"); 
            } 
            catch { }

            var user = await _localUserService.LoadUserAsync();

            if (!string.IsNullOrEmpty(token) && user != null)
            {
                await _navigationService.GoToAsync($"///{nameof(MainView)}");
            }
            else
            {
                try 
                { 
                    _secureStorage.Remove("auth_token"); 
                } 
                catch { }
                _localUserService.ClearUser();
                await _navigationService.GoToAsync($"///{nameof(StartingView)}");
            }
        }
    }
}