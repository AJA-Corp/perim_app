using perimapp.Models;
using perimapp.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace perimapp.Pages
{
    public partial class DeletedProductPage : ContentPage
    {
        private readonly LocalProductService _localProductService;
        public ObservableCollection<ProductInfos> Products { get; set; }

        public DeletedProductPage()
        {
            InitializeComponent();
            _localProductService = new LocalProductService();
            Products = new ObservableCollection<ProductInfos>();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var products = await _localProductService.LoadProductsAsync();
            Products.Clear();

            foreach (var product in products.Where(p => p.State == "Deleted"))
            {
                Products.Add(product);
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Confirmation",
                "Voulez-vous supprimer tous les produits de la corbeille ?",
                "Oui",
                "Non"
            );

            if (confirm)
            {
                await _localProductService.DeleteAllDeletedProductsAsync();

                Products.Clear();
            }
        }

        private async void OnRestoreClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton imageButton && imageButton.BindingContext is ProductInfos product)
            {
                bool confirm = await DisplayAlert(
                    "Confirmation",
                    $"Voulez-vous restaurer {product.Name} ?",
                    "Oui",
                    "Non"
                );

                if (confirm)
                {
                    bool success = await _localProductService.UpdateProductStateAsync(product.ProductUniqueId, "Active");

                    if (success)
                    {
                        Products.Remove(product);
                    }
                }
            }
        }
    }
}
