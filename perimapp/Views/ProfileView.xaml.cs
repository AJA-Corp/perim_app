using System;
using Microsoft.Maui.Controls;
using perimapp.ViewModels;
using perimapp.Services;

namespace perimapp.Views
{
    public partial class ProfileView : ContentPage
    {
        private ProfileViewModel _viewModel;

        public ProfileView(LocalUserService localUserService, LocalProductService localProductService, ApiProfileService apiProfileService, AuthService authService)
        {
            InitializeComponent();
            _viewModel = new ProfileViewModel(this, localUserService, localProductService, apiProfileService, authService);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadProfileDataCommand.ExecuteAsync(null);
        }
    }
}