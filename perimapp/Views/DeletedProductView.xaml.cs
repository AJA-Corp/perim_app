using perimapp.Services;
using Microsoft.Maui.Controls;
using perimapp.ViewModels;

namespace perimapp.Views
{
    public partial class DeletedProductView : ContentPage
    {
        private DeletedProductViewModel _viewModel;

        public DeletedProductView(DeletedProductViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDeletedProductsCommand.ExecuteAsync(null);
        }
    }
}