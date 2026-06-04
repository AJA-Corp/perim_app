using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using perimapp.Models;
using perimapp.PopUp;
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

            var confirmPopUp = new BoolPopUp("Confirmation", "Voulez-vous supprimer définitivement tous les produits de la corbeille ?", "Oui", "Non");

            if (confirmPopUp.Result)
            {
                await _localProductService.EmptyTrashLocallyAsync();

                Products.Clear();
            }
        }

        [RelayCommand]
        private async Task RestoreAsync(ProductInfos product)
        {
            if (product == null) return;

            var confirmPopUp = new BoolPopUp("Confirmation", $"Voulez-vous restaurer {product.DisplayName} ?", "Oui", "Non");
            await _page.ShowPopupAsync(confirmPopUp);

            if (confirmPopUp.Result)
            {
                bool localSuccess = await _localProductService.UpdateProductStateAsync(product.ProductUniqueId, "Active");

                if (localSuccess)
                {
                    Products.Remove(product);
                }
                else
                {
                    var popUp = new InfosPopUp("Erreur", "Une erreur est survenue lors de la restauration du produit. Veuillez réessayer.", "OK");
                    await _page.ShowPopupAsync(popUp);
                }
            }
        }
    }
}