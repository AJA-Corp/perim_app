using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using perimapp.Models;
using perimapp.Services;

namespace perimapp.ViewModels
{
    public partial class DeletedProductViewModel : ObservableObject
    {
        private readonly LocalProductService _localProductService;
        private readonly ContentPage _page;

        public ObservableCollection<ProductInfos> Products { get; } = new();

        public DeletedProductViewModel(ContentPage page, LocalProductService localProductService)
        {
            _page = page;
            _localProductService = localProductService;
        }

        [RelayCommand]
        public async Task LoadDeletedProductsAsync()
        {
            var products = await _localProductService.LoadProductsAsync();
            Products.Clear();

            foreach (var product in products.Where(p => p.State == "Deleted"))
            {
                Products.Add(product);
            }
        }

        [RelayCommand]
        private async Task DeleteAllAsync()
        {
            if (!Products.Any()) return;

            bool confirm = await _page.DisplayAlert(
                "Confirmation",
                "Voulez-vous supprimer définitivement tous les produits de la corbeille ?",
                "Oui",
                "Non"
            );

            if (confirm)
            {
                await _localProductService.EmptyTrashLocallyAsync();

                Products.Clear();
            }
        }

        [RelayCommand]
        private async Task RestoreAsync(ProductInfos product)
        {
            if (product == null) return;

            bool confirm = await _page.DisplayAlert(
                "Confirmation",
                $"Voulez-vous restaurer {product.DisplayName} ?",
                "Oui",
                "Non"
            );

            if (confirm)
            {
                bool localSuccess = await _localProductService.UpdateProductStateAsync(product.ProductUniqueId, "Active");

                if (localSuccess)
                {
                    Products.Remove(product);
                }
                else
                {
                    await _page.DisplayAlert("Erreur", "Impossible de restaurer le produit.", "OK");
                }
            }
        }
    }
}