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
            get => _viewModel?.ProductUniqueId;
            set
            {
                if (_viewModel != null)
                {
                    _viewModel.ProductUniqueId = value;
                }
            }
        }
        
        public DetailsView(NeonProductService neonService, LocalProductService localService, NeonUserService userService, LocalUserService localUserService)
        {
            InitializeComponent();
            _viewModel = new DetailsViewModel(this, neonService, localService, userService, localUserService);
            BindingContext = _viewModel;
        }
    }
}