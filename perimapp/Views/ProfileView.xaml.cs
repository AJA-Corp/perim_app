using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using perimapp.Models;
using perimapp.PopUp;
using perimapp.Services;
using System.Diagnostics;
using CommunityToolkit.Maui.Extensions;
using perimapp.ViewModels;

namespace perimapp.Views
{
    public partial class ProfileView : ContentPage
    {
        private ProfileViewModel _viewModel;

        public ProfileView()
        {
            InitializeComponent();
            _viewModel = new ProfileViewModel(this);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadProfileDataCommand.ExecuteAsync(null);
        }
    }
}