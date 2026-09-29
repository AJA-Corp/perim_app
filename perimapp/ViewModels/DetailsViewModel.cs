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
using perimapp.PopUp;
using CommunityToolkit.Maui.Extensions;

namespace perimapp.ViewModels
{
    public partial class DetailsViewModel : ObservableObject
    {
        private readonly LocalProductService _localService;
        private readonly LocalUserService _localUserService;

        [ObservableProperty]
        private string _productUniqueId;

        [ObservableProperty]
        private ProductInfos? _productDetail;

        public DetailsViewModel(LocalProductService localService, LocalUserService localUserService)
        {
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
                var errorPopup = new InfosPopUp("Produit non actif", $"Le produit {ProductDetail.Name} est actuellement dans un état '{ProductDetail.State}' et ne peut pas être consulté.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
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
                    var errorPopup = new InfosPopUp("Produit introuvable", "Aucun ID de produit fourni. Veuillez revenir en arrière et sélectionner un produit valide.", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                    await Shell.Current.GoToAsync("..");
                }
            }
        }

        [RelayCommand]
        private async Task ModifyProductAsync()
        {
            if (ProductDetail != null)
            {
                string route = $"{nameof(ModifyProductView)}?ProductUniqueId={ProductDetail.ProductUniqueId}";
                Debug.WriteLine($"DetailsView: Navigating to {route}");
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                var errorPopup = new InfosPopUp("Erreur de navigation", "Impossible de modifier le produit car l'ID est manquant. Veuillez revenir en arrière et sélectionner un produit valide.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
            }
        }

        [RelayCommand]
        private async Task DeleteProductAsync()
        {
            if (ProductDetail == null) return;

            var confirmPopup = new BoolPopUp(
                "Supprimer le produit",
                $"Êtes-vous sûr de vouloir jeter {ProductDetail.Name}? Il sera archivé temporairement.",
                "Oui",
                "Non"
            );

            await Shell.Current.CurrentPage.ShowPopupAsync(confirmPopup);

            if (!confirmPopup.Result) return;

            bool localSuccess = await _localService.UpdateProductStateAsync(ProductDetail.ProductUniqueId, "Deleted");

            if (localSuccess)
            {
                bool isExpired = ProductDetail.Dlc.HasValue && ProductDetail.Dlc.Value < DateOnly.FromDateTime(DateTime.Today);

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
                var errorPopup = new InfosPopUp("Erreur de suppression", "Impossible de supprimer le produit.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
            }
        }
    }
}