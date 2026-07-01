using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IDispatcherService _dispatcherService;

        [ObservableProperty]
        private string _productUniqueId;

        [ObservableProperty]
        private ProductInfos? _productDetail;

        public DetailsViewModel(
            LocalProductService localService, 
            LocalUserService localUserService,
            INavigationService navigationService,
            IDialogService dialogService,
            IDispatcherService dispatcherService)
        {
            _localService = localService;
            _localUserService = localUserService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _dispatcherService = dispatcherService;
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
                await _dialogService.ShowAlertAsync("Produit non actif", $"Le produit {ProductDetail.Name} est actuellement dans un état '{ProductDetail.State}' et ne peut pas être consulté.", "OK");
                await _navigationService.GoToAsync("..");
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
                    await _dialogService.ShowAlertAsync("Produit introuvable", "Aucun ID de produit fourni. Veuillez revenir en arrière et sélectionner un produit valide.", "OK");
                    await _navigationService.GoToAsync("..");
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
                await _navigationService.GoToAsync(route);
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur de navigation", "Impossible de modifier le produit car l'ID est manquant. Veuillez revenir en arrière et sélectionner un produit valide.", "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteProductAsync()
        {
            if (ProductDetail == null) return;

            bool confirm = await _dialogService.ShowConfirmAsync(
                "Supprimer le produit",
                $"Êtes-vous sûr de vouloir jeter {ProductDetail.Name}? Il sera archivé temporairement.",
                "Oui",
                "Non"
            );

            if (!confirm) return;

            bool localSuccess = await _localService.UpdateProductStateAsync(ProductDetail.ProductUniqueId, "Deleted");

            if (localSuccess)
            {
                bool isExpired = ProductDetail.Dlc.HasValue && ProductDetail.Dlc.Value < DateOnly.FromDateTime(DateTime.Today);

                if (isExpired)
                {
                    await _localUserService.IncrementLostProductCountAsync();
                    Debug.WriteLine("DetailsView: Compteur de produits perdus incrémenté localement.");
                }

                _dispatcherService.BeginInvokeOnMainThread(async () =>
                {
                    var productInList = AppData.CurrentProducts.FirstOrDefault(p => p.ProductUniqueId == ProductDetail.ProductUniqueId);
                    if (productInList != null)
                    {
                        productInList.State = "Deleted";
                        productInList.DeletedAt = DateTime.UtcNow;
                    }

                    await _navigationService.GoToAsync("..");
                });
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur de suppression", "Impossible de supprimer le produit.", "OK");
            }
        }
    }
}