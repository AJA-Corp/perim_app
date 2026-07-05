using System;
using Microsoft.Maui.Controls;
using perimapp.ViewModels;
using perimapp.Services;

namespace perimapp.Views
{
    public partial class ProfileView : ContentPage
    {
        private ProfileViewModel _viewModel;

        public ProfileView(ProfileViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadProfileDataAsync();
        }
    }
}