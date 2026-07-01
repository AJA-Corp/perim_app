using Microsoft.Maui.Controls;
using perimapp.Services;
using System.ComponentModel;
using perimapp.ViewModels;
using System.Threading.Tasks;

namespace perimapp.Views
{
    [QueryProperty(nameof(ProductUniqueId), "ProductUniqueId")]
    public partial class DetailsView : ContentPage
    {
        private DetailsViewModel _viewModel;

        public string ProductUniqueId
        {
            get => _viewModel?.ProductUniqueId ?? string.Empty;
            set
            {
                if (_viewModel != null)
                {
                    _viewModel.ProductUniqueId = value;
                }
            }
        }

        public DetailsView(DetailsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}