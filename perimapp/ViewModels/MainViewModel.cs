using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.PopUp;
using perimapp.Services;
using perimapp.Views;

namespace perimapp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly LocalProductService _localProductService;
        private readonly SyncService _syncService;

        public ObservableCollection<ProductInfos> Products => AppData.CurrentProducts;

        [ObservableProperty]
        private string _sortButtonText = "Tri: DLC (proche)";

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Products))]
        private int _displayedProductsCount;

        public MainViewModel(LocalProductService localProductService, SyncService syncService)
        {
            _localProductService = localProductService;
            _syncService = syncService;
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

                MainThread.BeginInvokeOnMainThread(() =>
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
                var errorPopup = new InfosPopUp("Erreur", "Une erreur est survenue lors du chargement des produits. Veuillez réessayer plus tard.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
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
            await Shell.Current.GoToAsync(nameof(ProfileView));
        }

        [RelayCommand]
        private async Task AddProductClickedAsync()
        {
            await Shell.Current.GoToAsync(nameof(AddProductView));
        }

        [RelayCommand]
        private async Task ProductSelectedAsync(ProductInfos selectedProduct)
        {
            if (selectedProduct != null)
            {
                await Shell.Current.GoToAsync($"{nameof(DetailsView)}?ProductUniqueId={selectedProduct.ProductUniqueId}");
            }
        }

        [RelayCommand]
        private async Task LostProductsClickedAsync()
        {
            await Shell.Current.GoToAsync(nameof(DeletedProductView));
        }

        [RelayCommand]
        private async Task SortButtonClickedAsync()
        {
            string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync(
                "Trier par", "Annuler", null,
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