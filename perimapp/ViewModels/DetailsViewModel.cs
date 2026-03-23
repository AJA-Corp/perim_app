using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;

namespace perimapp.ViewModels
{
    public partial class DetailsViewModel : ObservableObject
    {
        private readonly NeonProductService _neonService;
        private readonly LocalProductService _localService;
        private readonly NeonUserService _userService;
        private readonly LocalUserService _localUserService;
        private readonly ContentPage _page;

        [ObservableProperty]
        private string _productUniqueId;

        [ObservableProperty]
        private ProductInfos? _productDetail;

        public DetailsViewModel(
            ContentPage page,
            NeonProductService neonService,
            LocalProductService localService,
            NeonUserService userService,
            LocalUserService localUserService)
        {
            _page = page;
            _neonService = neonService;
            _localService = localService;
            _userService = userService;
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
                Debug.WriteLine($"DetailsView: Tentative d'acc\u00e8s \u00e0 un produit non actif ({ProductDetail.State}). Redirection.");
                await _page.DisplayAlert("Erreur", "Ce produit n'est plus actif.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (ProductDetail != null)
            {
                Debug.WriteLine($"DetailsView: Produit charg\u00e9 : {ProductDetail.Name}");
            }
            else
            {
                Debug.WriteLine("DetailsView: Aucun ProductUniqueId fourni ou produit non trouv\u00e9.");
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
                string route = $"{nameof(perimapp.Views.ModifyProductView)}?ProductUniqueId={ProductDetail.ProductUniqueId}";
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
                $"Êtes-vous sûr de vouloir supprimer {ProductDetail.Name}? Il sera archivé temporairement.",
                "Oui",
                "Non"
            );

            if (confirmed)
            {
                string idToPass = ProductDetail.Id.ToString();

                bool localSuccess = await _localService.UpdateProductStateAsync(idToPass, "Deleted");
                bool neonSuccess = await _neonService.UpdateProductStateAsync(idToPass, "Deleted");

                if (localSuccess || neonSuccess)
                {
                    bool isExpired = ProductDetail.Dlc.Date < DateTime.Today;

                    if (isExpired)
                    {
                        bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

                        await _localUserService.IncrementLostProductCountAsync();
                        Debug.WriteLine("DetailsView: Compteur de produits perdus incr\u00e9ment\u00e9 localement.");

                        if (hasInternet)
                        {
                            string userIdStr = await SecureStorage.GetAsync("user_id");
                            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                            {
                                bool neonUpdateSuccess = await _userService.IncrementLostProductCountAsync(userId);
                                if (neonUpdateSuccess)
                                {
                                    Debug.WriteLine($"DetailsView: Compteur synchronis\u00e9 avec Neon pour user {userId}");
                                }
                                else
                                {
                                    Debug.WriteLine("DetailsView: \u00c9chec de la synchronisation avec Neon, les donn\u00e9es locales seront synchronis\u00e9es plus tard.");
                                }
                            }
                        }
                    }

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var productInList = AppData.CurrentProducts.FirstOrDefault(p => p.Id == ProductDetail.Id);
                        if (productInList != null)
                        {
                            productInList.State = "Deleted";
                            productInList.DeletedAt = DateTime.UtcNow;
                        }

                        ProductDetail.State = "Deleted";
                        ProductDetail.DeletedAt = DateTime.UtcNow;

                        await Shell.Current.GoToAsync(nameof(perimapp.Views.MainView));
                    });
                }
                else
                {
                    await _page.DisplayAlert("Erreur", "Impossible de supprimer le produit.", "OK");
                }
            }
        }
    }
}