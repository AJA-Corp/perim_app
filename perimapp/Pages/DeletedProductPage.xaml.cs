using perimapp.Models;
using perimapp.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using Microsoft.Maui.Networking;
using perimapp.ViewModels;

namespace perimapp.Pages
{
    public partial class DeletedProductPage : ContentPage
    {
        private DeletedProductViewModel _viewModel;

        public DeletedProductPage(NeonProductService neonProductService, LocalProductService localProductService, NeonUserService userService, LocalUserService localUserService)
        {
            InitializeComponent();
            _viewModel = new DeletedProductViewModel(this, neonProductService, localProductService, userService, localUserService);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDeletedProductsCommand.ExecuteAsync(null);
        }
    }
}
