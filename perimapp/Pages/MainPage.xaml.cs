using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services; // <-- Pour NeonProductService

namespace perimapp.Pages
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<ProductInfos> Products => AppData.CurrentProducts;

        private readonly NeonProductService _productService = new NeonProductService();

        public MainPage()
        {
            InitializeComponent();
            int savedUserId = Preferences.Default.Get("UserId", -1);
            Console.WriteLine(
                $"[DEBUG] ID utilisateur récupéré depuis Preferences : {savedUserId}"
            );
            NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = this;

            _ = LoadProductsAsync();
        }

        private async void OnProfileIconClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ProfilePage));
        }

        private async void OnAddProductClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AddProductPage));
        }

        private async void OnProductSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Any())
            {
                var selectedProduct = e.CurrentSelection.FirstOrDefault() as ProductInfos;

                if (selectedProduct != null)
                {
                    ((CollectionView)sender).SelectedItem = null;

                    // Utilise Id pour la navigation (car ProductUniqueId n'existe pas)
                    await Shell.Current.GoToAsync(
                        $"{nameof(DetailsPage)}?ProductUniqueId={selectedProduct.ProductUniqueId}"
                    );
                }
            }
        }

        private async Task LoadProductsAsync()
{
    try
    {
        // 1. Récupérer l'ID de l'utilisateur de manière sécurisée et asynchrone
        string userIdString = await SecureStorage.GetAsync("user_id");

        // Si l'ID est manquant ou invalide, on ne peut pas charger les produits de l'utilisateur
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            Console.WriteLine("Erreur : ID utilisateur non valide. Chargement des produits locaux.");
            
            // On charge la liste locale par défaut
            /*var loadedProducts = await LoadProductsFromJsonAsync();
            
            if (loadedProducts != null)
            {
                var sortedProducts = loadedProducts.OrderBy(p => p.DaysRemaining).ToList();

                Products.Clear();
                foreach (var product in sortedProducts)
                {
                    Products.Add(product);
                }
            }*/
            return; // On quitte la fonction
        }

        // 2. Si l'ID est valide, on tente de charger depuis la BDD Neon
        List<ProductInfos> loadedProducts = await _productService.GetUserProductsAsync(userId);

        if (loadedProducts == null || loadedProducts.Count == 0)
        {
            // Fallback sur JSON local si aucun produit n'est chargé ou en cas d'erreur
            loadedProducts = await LoadProductsFromJsonAsync();
        }

        if (loadedProducts != null)
        {
            var sortedProducts = loadedProducts.OrderBy(p => p.DaysRemaining).ToList();

            Products.Clear();
            foreach (var product in sortedProducts)
            {
                Products.Add(product);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur lors du chargement des produits : {ex.Message}");
        await DisplayAlert(
            "Erreur",
            "Impossible de charger les produits. " + ex.Message,
            "OK"
        );
    }
}

        private async Task<List<ProductInfos>> LoadProductsFromJsonAsync()
        {
            try
            {
                using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(
                    "responseProductInfos.json"
                );
                using StreamReader reader = new StreamReader(fileStream);
                string jsonContent = await reader.ReadToEndAsync();

                var products = System.Text.Json.JsonSerializer.Deserialize<List<ProductInfos>>(
                    jsonContent
                );
                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement JSON local : {ex.Message}");
                return new List<ProductInfos>();
            }
        }
    }
}
