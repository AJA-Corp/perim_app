using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class MainViewModel : ObservableObject
    {
        private readonly LocalProductService _localProductService;
        private readonly SyncService _syncService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IDispatcherService _dispatcherService;

        public ObservableCollection<ProductInfos> Products => AppData.CurrentProducts;

        [ObservableProperty]
        private string _sortButtonText = "Tri: DLC (proche)";

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Products))]
        private int _displayedProductsCount;

        public MainViewModel(
            LocalProductService localProductService, 
            SyncService syncService,
            INavigationService navigationService,
            IDialogService dialogService,
            IDispatcherService dispatcherService)
        {
            _localProductService = localProductService;
            _syncService = syncService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _dispatcherService = dispatcherService;
        }

        [RelayCommand]
        public async Task LoadProductsAsync()
        {
            try
            {
                var allProducts = await _localProductService.LoadProductsAsync();

                var activeProducts = allProducts
                    .Where(p => p.State == "Active")
                    .OrderBy(p => p.DaysRemaining)
                    .ToList();

                _dispatcherService.BeginInvokeOnMainThread(() =>
                {
                    AppData.CurrentProducts.Clear();
                    foreach (var product in activeProducts)
                        AppData.CurrentProducts.Add(product);

                    DisplayedProductsCount = AppData.CurrentProducts.Count;
                    perimapp.Services.NotificationScheduler.UpdateSchedules();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des produits : {ex.Message}");
                await _dialogService.ShowAlertAsync("Erreur", "Une erreur est survenue lors du chargement des produits. Veuillez réessayer plus tard.", "OK");
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                await _syncService.ProcessSyncAsync();

                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du rafraîchissement : {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task ProfileIconClickedAsync()
        {
            await _navigationService.GoToAsync(nameof(ProfileView));
        }

        [RelayCommand]
        private async Task AddProductClickedAsync()
        {
            await _navigationService.GoToAsync(nameof(AddProductView));
        }

        [RelayCommand]
        private async Task ProductSelectedAsync(ProductInfos selectedProduct)
        {
            if (selectedProduct != null)
            {
                await _navigationService.GoToAsync($"{nameof(DetailsView)}?ProductUniqueId={selectedProduct.ProductUniqueId}");
            }
        }

        [RelayCommand]
        private async Task LostProductsClickedAsync()
        {
            await _navigationService.GoToAsync(nameof(DeletedProductView));
        }

        [RelayCommand]
        private async Task SortButtonClickedAsync()
        {
            string result = await _dialogService.ShowActionSheetAsync(
                "Trier par", "Annuler", null!,
                "DLC (proche)",
                "DLC (lointaine)"
            );

            if (result != "Annuler" && result != null)
            {
                SortProducts(result);
            }
        }

        private void SortProducts(string sortOption)
        {
            IEnumerable<ProductInfos> sortedProducts = Products;

            switch (sortOption)
            {
                case "DLC (proche)":
                    sortedProducts = Products.OrderBy(p => p.DaysRemaining).ToList();
                    SortButtonText = "Tri: DLC (proche)";
                    break;
                case "DLC (lointaine)":
                    sortedProducts = Products.OrderByDescending(p => p.DaysRemaining).ToList();
                    SortButtonText = "Tri: DLC (lointaine)";
                    break;
            }

            Products.Clear();
            foreach (var product in sortedProducts)
            {
                Products.Add(product);
            }
        }
    }
}