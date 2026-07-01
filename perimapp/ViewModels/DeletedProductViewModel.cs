using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using perimapp.Models;
using perimapp.Services;

namespace perimapp.ViewModels
{
    public partial class DeletedProductViewModel : ObservableObject
    {
        private readonly LocalProductService _localProductService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<ProductInfos> Products { get; } = new();

        public DeletedProductViewModel(LocalProductService localProductService, IDialogService dialogService)
        {
            _localProductService = localProductService;
            _dialogService = dialogService;
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
            bool confirm = await _dialogService.ShowConfirmAsync("Vider la corbeille", "Voulez-vous vraiment vider la corbeille ? Cette action est irréversible.", "Oui", "Non");

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

            bool confirm = await _dialogService.ShowConfirmAsync("Confirmation", $"Voulez-vous restaurer {product.DisplayName} ?", "Oui", "Non");

            if (confirm)
            {
                bool localSuccess = await _localProductService.UpdateProductStateAsync(product.ProductUniqueId, "Active");

                if (localSuccess)
                {
                    Products.Remove(product);
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Erreur", "Une erreur est survenue lors de la restauration du produit. Veuillez réessayer.", "OK");
                }
            }
        }
    }
}