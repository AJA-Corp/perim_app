using System;
using Microsoft.Maui.Controls;
using perimapp.Services;
using perimapp.ViewModels;

namespace perimapp.Views
{
    [QueryProperty(nameof(ProductUniqueId), "ProductUniqueId")]
    public partial class ModifyProductView : ContentPage
    {
        private ModifyProductViewModel _viewModel;

        public string? ProductUniqueId
        {
            get => _viewModel?.ProductUniqueId;
            set
            {
                if (_viewModel != null)
                {
                    _viewModel.ProductUniqueId = value;
                }
            }
        }

        public ModifyProductView(LocalProductService localProductService)
        {
            InitializeComponent();
            _viewModel = new ModifyProductViewModel(localProductService);
            BindingContext = _viewModel;
        }

        private void ProductNameEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                _viewModel.CurrentCustomName = entry.Text ?? string.Empty;
            }
        }
    }
}