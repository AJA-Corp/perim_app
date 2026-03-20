using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Networking;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using perimapp.Pages;

namespace perimapp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly NeonProductService _neonProductService;
        private readonly LocalProductService _localProductService;
        private readonly ContentPage _page;

        public ObservableCollection<ProductInfos> Products => AppData.CurrentProducts;

        [ObservableProperty]
        private string _sortButtonText = "Tri: DLC (proche)";

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Products))]
        private int _displayedProductsCount;

        public MainViewModel(ContentPage page, NeonProductService neonProductService, LocalProductService localProductService)
        {
            _page = page;
            _neonProductService = neonProductService;
            _localProductService = localProductService;

            int savedUserId = Preferences.Default.Get("UserId", -1);
            Console.WriteLine($"[DEBUG] ID utilisateur r\u00e9cup\u00e9r\u00e9 depuis Preferences : {savedUserId}");
        }

        [RelayCommand]
        public async Task LoadProductsAsync()
        {
            try
            {
                List<ProductInfos> allProducts;

                string userIdString = await SecureStorage.GetAsync("user_id");
                bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

                if (hasInternet && !string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    try
                    {
                        var serveurProducts = await _neonProductService.GetUserProductsAsync(userId);
                        await _localProductService.SaveProductsAsync(serveurProducts);
                        allProducts = serveurProducts;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur de r\u00e9cup\u00e9ration serveur : {ex.Message}. Chargement local...");
                        allProducts = await _localProductService.LoadProductsAsync();
                    }
                }
                else
                {
                    allProducts = await _localProductService.LoadProductsAsync();
                }

                var jsonDebug = JsonSerializer.Serialize(allProducts, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"[DEBUG-JSON] Liste compl\u00e8te des produits charg\u00e9e ({allProducts.Count} \u00e9l\u00e9ments) :");
                Console.WriteLine(jsonDebug);

                // FILTRE ESSENTIEL
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
                await _page.DisplayAlert("Erreur", "Impossible de charger les produits. " + ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du rafra\u00eechissement : {ex.Message}");
                await _page.DisplayAlert("Erreur", "Impossible d'actualiser les produits.", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task ProfileIconClickedAsync()
        {
            await Shell.Current.GoToAsync(nameof(ProfilePage));
        }

        [RelayCommand]
        private async Task AddProductClickedAsync()
        {
            await Shell.Current.GoToAsync(nameof(AddProductPage));
        }

        [RelayCommand]
        private async Task ProductSelectedAsync(ProductInfos selectedProduct)
        {
            if (selectedProduct != null)
            {
                await Shell.Current.GoToAsync($"{nameof(DetailsPage)}?ProductUniqueId={selectedProduct.ProductUniqueId}");
            }
        }

        [RelayCommand]
        private async Task LostProductsClickedAsync()
        {
            await Shell.Current.GoToAsync(nameof(DeletedProductPage));
        }

        [RelayCommand]
        private async Task SortButtonClickedAsync()
        {
            string result = await _page.DisplayActionSheet(
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