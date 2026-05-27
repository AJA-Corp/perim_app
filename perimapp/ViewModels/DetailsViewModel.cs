using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using perimapp.Views;

namespace perimapp.ViewModels
{
    public partial class DetailsViewModel : ObservableObject
    {
        private readonly LocalProductService _localService;
        private readonly LocalUserService _localUserService;
        private readonly ContentPage _page;

        [ObservableProperty]
        private string _productUniqueId;

        [ObservableProperty]
        private ProductInfos? _productDetail;

        public DetailsViewModel(ContentPage page, LocalProductService localService, LocalUserService localUserService)
        {
            _page = page;
            _localService = localService;
            _localUserService = localUserService;
        }

        partial void OnProductUniqueIdChanged(string value)
        {
            LoadProductDetailAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task LoadProductDetailAsync()
        {
            ProductDetail = AppData.CurrentProducts.FirstOrDefault(p => p.ProductUniqueId == ProductUniqueId);

            if (ProductDetail != null && ProductDetail.State != "Active")
            {
                Debug.WriteLine($"DetailsView: Tentative d'accès à un produit non actif ({ProductDetail.State}). Redirection.");
                await _page.DisplayAlert("Erreur", "Ce produit n'est plus actif.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (ProductDetail != null)
            {
                Debug.WriteLine($"DetailsView: Produit chargé : {ProductDetail.Name}");
            }
            else
            {
                Debug.WriteLine("DetailsView: Aucun ProductUniqueId fourni ou produit non trouvé.");
                if (string.IsNullOrEmpty(ProductUniqueId))
                {
                    await _page.DisplayAlert("Erreur", "Aucun ID de produit fourni.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
        }

        [RelayCommand]
        private async Task ModifyProductAsync()
        {
            if (ProductDetail != null)
            {
                string route = $"///{nameof(ModifyProductView)}?ProductUniqueId={ProductDetail.ProductUniqueId}";
                Debug.WriteLine($"DetailsView: Navigating to {route}");
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                await _page.DisplayAlert("Erreur", "Impossible de modifier le produit. ID manquant.", "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteProductAsync()
        {
            if (ProductDetail == null) return;

            bool confirmed = await _page.DisplayAlert(
                "Supprimer le produit",
                $"Êtes-vous sûr de vouloir jeter {ProductDetail.Name}? Il sera archivé temporairement.",
                "Oui",
                "Non"
            );

            if (!confirmed) return;

            bool localSuccess = await _localService.UpdateProductStateAsync(ProductDetail.ProductUniqueId, "Deleted");

            if (localSuccess)
            {
                bool isExpired = ProductDetail.Dlc.Date < DateTime.Today;

                if (isExpired)
                {
                    await _localUserService.IncrementLostProductCountAsync();
                    Debug.WriteLine("DetailsView: Compteur de produits perdus incrémenté localement.");
                }

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var productInList = AppData.CurrentProducts.FirstOrDefault(p => p.ProductUniqueId == ProductDetail.ProductUniqueId);
                    if (productInList != null)
                    {
                        productInList.State = "Deleted";
                        productInList.DeletedAt = DateTime.UtcNow;
                    }

                    await Shell.Current.GoToAsync("..");
                });
            }
            else
            {
                await _page.DisplayAlert("Erreur", "Impossible de supprimer le produit.", "OK");
            }
        }
    }
}