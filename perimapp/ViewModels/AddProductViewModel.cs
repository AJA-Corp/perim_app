using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using Microsoft.Maui.ApplicationModel;

namespace perimapp.ViewModels
{
    public partial class AddProductViewModel : ObservableObject
    {
        private ContentPage _page;

        [ObservableProperty]
        private int _currentQuantity = 1;

        [ObservableProperty]
        private string _barcodeText;

        [ObservableProperty]
        private string _productNameText = "Nom du produit";

        [ObservableProperty]
        private bool _hasImage;

        [ObservableProperty]
        private bool _hasNoImage = true;

        [ObservableProperty]
        private string _productImageSource;

        [ObservableProperty]
        private DateTime _dlcDate = DateTime.Today;

        public AddProductViewModel(ContentPage page)
        {
            _page = page;
        }

        [RelayCommand]
        private void IncrementQuantity()
        {
            CurrentQuantity++;
        }

        [RelayCommand]
        private void DecrementQuantity()
        {
            if (CurrentQuantity > 1)
            {
                CurrentQuantity--;
            }
        }

        [RelayCommand]
        private async Task ValidateProductAsync()
        {
            string userIdString = await SecureStorage.GetAsync("user_id");

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                await _page.DisplayAlert("Erreur", "Utilisateur non identifi\u00e9. Veuillez vous reconnecter.", "OK");
                await Shell.Current.GoToAsync(nameof(perimapp.Views.StartingView));
                return;
            }

            if (!long.TryParse(BarcodeText, out long barcode))
            {
                await _page.DisplayAlert("Erreur", "Code-barres invalide.", "OK");
                return;
            }

            var service = new NeonProductService();
            var product = await service.GetProductDataAsync(barcode);

            if (product == null)
            {
                var apiService = new OpenFoodFactsService();
                var apiProduct = await apiService.GetProductFromApiAsync(barcode);

                if (apiProduct == null)
                {
                    await _page.DisplayAlert("Erreur", "Produit introuvable dans la base et API.", "OK");
                    return;
                }

                await service.AddProductDataAsync(apiProduct);
                product = apiProduct;
            }

            product.Dlc = DlcDate;
            product.Quantity = CurrentQuantity;
            product.AddedAt = DateTime.Now;

            bool ok = await service.AddUserProductAsync(product, userId);

            // Sauvegarde locale pour le hors-ligne
            var localService = new LocalProductService();
            await localService.AddProductAsync(product);

            AppData.CurrentProducts.Add(product);
            perimapp.Services.NotificationScheduler.UpdateSchedules();

            if (ok)
            {
                await _page.DisplayAlert("Succ\u00e8s", "Produit ajout\u00e9 avec succ\u00e8s.", "OK");
                await Shell.Current.GoToAsync(nameof(perimapp.Views.MainView));
            }
            else
            {
                await _page.DisplayAlert("Erreur", "Impossible d'ajouter le produit.", "OK");
            }
        }

        [RelayCommand]
        public async Task SearchBarcodeAsync()
        {
            if (!long.TryParse(BarcodeText, out long barcode))
            {
                await _page.DisplayAlert("Erreur", "Code-barres invalide.", "OK");
                return;
            }

            // Get user's home code for custom name lookup
            string userIdString = await SecureStorage.GetAsync("user_id");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                await _page.DisplayAlert("Erreur", "Utilisateur non identifi\u00e9. Veuillez vous reconnecter.", "OK");
                await Shell.Current.GoToAsync(nameof(perimapp.Views.StartingView));
                return;
            }

            var userService = new NeonUserService();
            var user = await userService.GetUserProfileAsync(userId);
            
            var service = new NeonProductService();
            ProductInfos? product = null;

            // Try to get product with custom name if user has home code
            if (user?.HomeCode != null)
            {
                product = await service.GetProductDataWithCustomNameAsync(barcode, user.HomeCode);
            }
            
            // Fallback to regular product data if no custom name version found
            if (product == null)
            {
                product = await service.GetProductDataAsync(barcode);
            }

            if (product == null)
            {
                var apiService = new OpenFoodFactsService();
                product = await apiService.GetProductFromApiAsync(barcode);

                if (product == null)
                {
                    bool reponse = await _page.DisplayAlert(
                        "Erreur",
                        "Produit introuvable. Voulez-vous ajouter un nouveau produit perso. ?",
                        "Oui",
                        "Non"
                    );

                    if (reponse)
                    {
                        string result = await _page.DisplayPromptAsync(
                            "Nom du produit",
                            "Entrez le nom du produit",
                            "OK",
                            "Annuler",
                            "Entrez ici",
                            maxLength: 255,
                            keyboard: Keyboard.Text
                        );

                        if (!string.IsNullOrEmpty(result))
                        {
                            BarcodeText = string.Empty;
                            ProductNameText = result;
                            ProductImageSource = null;
                            HasImage = false;
                            HasNoImage = true;
                        }
                    }
                    return;
                }
            }

            // Display the appropriate name (custom name if available, otherwise original name)
            ProductNameText = product.DisplayName;
            ProductImageSource = product.UrlImage;
            
            if (!string.IsNullOrEmpty(ProductImageSource))
            {
                HasImage = true;
                HasNoImage = false;
            }
            else
            {
                HasImage = false;
                HasNoImage = true;
            }
        }

        [RelayCommand]
        private async Task ScanBarcodeAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status == PermissionStatus.Granted)
            {
                var ScannerView = new perimapp.Views.ScannerView();

                ScannerView.OnBarcodeScanned = (scannedCode) =>
                {
                    BarcodeText = scannedCode;
                    SearchBarcodeAsync().ConfigureAwait(false);
                };

                await _page.Navigation.PushModalAsync(ScannerView);
            }
            else
            {
                await _page.DisplayAlert("Erreur", "La permission de la cam\u00e9ra est requise pour scanner un produit.", "OK");
            }
        }
    }
}