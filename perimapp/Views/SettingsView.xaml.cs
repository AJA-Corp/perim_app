using Microsoft.Maui.Controls;
using perimapp.Services;
using perimapp.ViewModels;

namespace perimapp.Views
{
    public partial class SettingsView : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsView(LocalUserService localUserService, AuthService authService, SyncService syncService)
        {
            InitializeComponent();
            _viewModel = new SettingsViewModel(localUserService, authService, syncService);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadSettingsAsync();
        }
    }
}
