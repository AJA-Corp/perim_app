using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using perimapp.Views;

namespace perimapp.ViewModels
{
    public partial class AddProductViewModel : ObservableObject
    {
        private ContentPage _page;
        private readonly LocalProductService _localProductService;
        private readonly LocalUserService _localUserService;
        private readonly ApiProductService _apiProductService;

        private ProductInfos? _searchedProductData;

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

        public AddProductViewModel(ContentPage page, LocalProductService localProductService, LocalUserService localUserService, ApiProductService apiProductService)
        {
            _page = page;
            _localProductService = localProductService;
            _localUserService = localUserService;
            _apiProductService = apiProductService;
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
        public async Task SearchBarcodeAsync()
        {
            if (!long.TryParse(BarcodeText, out long barcode))
            {
                await _page.DisplayAlert("Erreur", "Code-barres invalide.", "OK");
                return;
            }

            var user = await _localUserService.LoadUserAsync();
            if (user == null)
            {
                await _page.DisplayAlert("Erreur", "Utilisateur non identifié. Veuillez vous reconnecter.", "OK");
                await Shell.Current.GoToAsync($"///{nameof(StartingView)}");
                return;
            }

            ProductInfos? product = null;

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                product = await _apiProductService.SearchProductAsync(barcode);

                if (product != null && !string.IsNullOrEmpty(user.HomeCode))
                {
                    string? remoteCustomName = await _apiProductService.GetCustomProductNameAsync(barcode, user.HomeCode);

                    if (!string.IsNullOrWhiteSpace(remoteCustomName))
                    {
                        product.CustomName = remoteCustomName;

                        await _localProductService.SaveCustomProductNameAsync(barcode, user.HomeCode, remoteCustomName);
                    }
                }
            }

            if (product == null && Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                var offService = new OpenFoodFactsService();
                product = await offService.GetProductFromApiAsync(barcode);
            }

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
                        _searchedProductData = new ProductInfos
                        {
                            Barcode = barcode,
                            Name = result
                        };

                        ProductNameText = result;
                        ProductImageSource = null;
                        HasImage = false;
                        HasNoImage = true;
                    }
                }
                return;
            }

            _searchedProductData = product;

            string? localCustomName = await _localProductService.GetCustomProductNameAsync(barcode, user.HomeCode);
            if (!string.IsNullOrEmpty(localCustomName))
            {
                _searchedProductData.CustomName = localCustomName;
            }

            ProductNameText = _searchedProductData.DisplayName;
            ProductImageSource = _searchedProductData.UrlImage;

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
        private async Task ValidateProductAsync()
        {
            if (_searchedProductData == null)
            {
                await SearchBarcodeAsync();

                if (_searchedProductData == null) return;
            }

            var user = await _localUserService.LoadUserAsync();
            if (user == null) return;

            var productToSave = new ProductInfos
            {
                Barcode = _searchedProductData.Barcode,
                Name = _searchedProductData.Name,
                CustomName = _searchedProductData.CustomName,
                UrlImage = _searchedProductData.UrlImage,
                Category = _searchedProductData.Category,
                Conservation = _searchedProductData.Conservation,
                HomeCode = user.HomeCode,
                AddedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Dlc = DlcDate,
                Quantity = CurrentQuantity,
                State = "Active"
            };

            await _localProductService.AddProductAsync(productToSave);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AppData.CurrentProducts.Add(productToSave);
                perimapp.Services.NotificationScheduler.UpdateSchedules();
            });

            await _page.DisplayAlert("Succès", "Produit ajouté avec succès.", "OK");
            await Shell.Current.GoToAsync("..");
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
                await _page.DisplayAlert("Erreur", "La permission de la caméra est requise pour scanner un produit.", "OK");
            }
        }
    }
}