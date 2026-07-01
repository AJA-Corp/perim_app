using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using perimapp.Views;
using System;
using System.Threading.Tasks;

namespace perimapp.ViewModels
{
    public partial class AddProductViewModel : ObservableObject
    {
        private readonly LocalProductService _localProductService;
        private readonly LocalUserService _localUserService;
        private readonly ApiProductService _apiProductService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IConnectivity _connectivity;

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

        public AddProductViewModel(
            LocalProductService localProductService, 
            LocalUserService localUserService, 
            ApiProductService apiProductService,
            INavigationService navigationService,
            IDialogService dialogService,
            IDispatcherService dispatcherService,
            IConnectivity connectivity)
        {
            _localProductService = localProductService;
            _localUserService = localUserService;
            _apiProductService = apiProductService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _dispatcherService = dispatcherService;
            _connectivity = connectivity;
        }

        protected virtual Task<PermissionStatus> CheckCameraPermissionHelperAsync()
        {
            return Permissions.CheckStatusAsync<Permissions.Camera>();
        }

        protected virtual Task<PermissionStatus> RequestCameraPermissionHelperAsync()
        {
            return Permissions.RequestAsync<Permissions.Camera>();
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
                await _dialogService.ShowAlertAsync("Erreur", "Code-barres invalide.", "OK");
                return;
            }

            var user = await _localUserService.LoadUserAsync();
            if (user == null)
            {
                await _dialogService.ShowAlertAsync("Erreur", "Utilisateur non identifié. Veuillez vous reconnecter.", "OK");
                await _navigationService.GoToAsync($"///{nameof(StartingView)}");
                return;
            }

            ProductInfos? product = null;

            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
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

            if (product == null && _connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var offService = new OpenFoodFactsService();
                product = await offService.GetProductFromApiAsync(barcode);
            }

            if (product == null)
            {
                bool answer = await _dialogService.ShowConfirmAsync("Produit introuvable", "Voulez-vous ajouter un nouveau produit perso ?", "Oui", "Non");

                if (!answer) return;

                string? result = await _dialogService.ShowPromptAsync(
                    "Nom du produit",
                    "Veuillez entrer le nom du produit",
                    "Entrez ici...",
                    "OK",
                    "Annuler"
                );

                if (!string.IsNullOrEmpty(result))
                {
                    _searchedProductData = new ProductInfos
                    {
                        Barcode = barcode,
                        Name = result
                    };

                    ProductNameText = result;
                    ProductImageSource = null!;
                    HasImage = false;
                    HasNoImage = true;
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
            ProductImageSource = _searchedProductData.UrlImage!;

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
                Dlc = DateOnly.FromDateTime(DlcDate),
                Quantity = CurrentQuantity,
                State = "Active"
            };

            await _localProductService.AddProductAsync(productToSave);

            _dispatcherService.BeginInvokeOnMainThread(() =>
            {
                AppData.CurrentProducts.Add(productToSave);
                NotificationScheduler.UpdateSchedules();
            });

            await _dialogService.ShowAlertAsync("Succès", "Produit ajouté avec succès.", "OK");
            await _navigationService.GoToAsync("..");
        }

        [RelayCommand]
        private async Task ScanBarcodeAsync()
        {
            var status = await CheckCameraPermissionHelperAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await RequestCameraPermissionHelperAsync();
            }

            if (status == PermissionStatus.Granted)
            {
                var ScannerView = new perimapp.Views.ScannerView();

                ScannerView.OnBarcodeScanned = (scannedCode) =>
                {
                    BarcodeText = scannedCode;
                    SearchBarcodeAsync().ConfigureAwait(false);
                };

                await _navigationService.PushModalAsync(ScannerView);
            }
            else
            {
                await _dialogService.ShowAlertAsync("Permission refusée", "La permission de la caméra est requise pour scanner un produit.", "OK");
            }
        }
    }
}