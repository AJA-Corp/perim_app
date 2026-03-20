using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using perimapp.Models;
using perimapp.Services;

namespace perimapp.ViewModels
{
    public partial class DeletedProductViewModel : ObservableObject
    {
        private readonly LocalProductService _localProductService;
        private readonly NeonProductService _neonProductService;
        private readonly NeonUserService _userService;
        private readonly LocalUserService _localUserService;
        private readonly ContentPage _page;

        public ObservableCollection<ProductInfos> Products { get; } = new();

        public DeletedProductViewModel(
            ContentPage page,
            NeonProductService neonProductService,
            LocalProductService localProductService,
            NeonUserService userService,
            LocalUserService localUserService)
        {
            _page = page;
            _neonProductService = neonProductService;
            _localProductService = localProductService;
            _userService = userService;
            _localUserService = localUserService;
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
            bool confirm = await _page.DisplayAlert(
                "Confirmation",
                "Voulez-vous supprimer d\u00e9finitivement tous les produits de la corbeille ?",
                "Oui",
                "Non"
            );

            if (confirm)
            {
                await _localProductService.DeleteAllDeletedProductsAsync();

                bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                if (hasInternet)
                {
                    string userIdString = await SecureStorage.GetAsync("user_id");
                    if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                    {
                        await _neonProductService.EmptyTrashOnlineAsync(userId);
                    }
                }

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
                System.Diagnostics.Debug.WriteLine($"DeletedProductView: Tentative de restauration du produit {product.DisplayName}");

                bool localSuccess = await _localProductService.UpdateProductStateAsync(product.ProductUniqueId, "Active");

                if (localSuccess)
                {
                    bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

                    if (product.DeletedAt.HasValue)
                    {
                        bool wasExpiredAtDeletion = product.Dlc.Date < product.DeletedAt.Value.Date;

                        if (wasExpiredAtDeletion)
                        {
                            await _localUserService.DecrementLostProductCountAsync();

                            if (hasInternet)
                            {
                                string userIdStr = await SecureStorage.GetAsync("user_id");
                                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                                {
                                    await _userService.DecrementLostProductCountAsync(userId);
                                }
                            }
                        }
                    }

                    if (hasInternet)
                    {
                        await _neonProductService.UpdateProductStateAsync(product.Id.ToString(), "Active");
                    }

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