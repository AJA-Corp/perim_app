using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Controls;
using System;
using System.IO;
using perimapp.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using perimapp.Data;

namespace perimapp.Pages
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<ProductInfos> Products { get; set; }

        public MainPage()
        {
            InitializeComponent();
            Products = new ObservableCollection<ProductInfos>();
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
                    // Désélectionne l'élément pour permettre de cliquer à nouveau sur le même
                    ((CollectionView)sender).SelectedItem = null;

                    // MODIFIÉ : Passer ProductUniqueId pour la navigation
                    await Shell.Current.GoToAsync($"{nameof(DetailsPage)}?productUniqueId={selectedProduct.ProductUniqueId}");
                }
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                // Chargement du fichier responseProductInfos.json depuis les ressources de l'application
                using Stream fileStream = await FileSystem.OpenAppPackageFileAsync("responseProductInfos.json");
                using StreamReader reader = new StreamReader(fileStream);
                string jsonContent = await reader.ReadToEndAsync();
                
                // Désérialisation du JSON en une liste de ProductMainPage
                List<ProductInfos>? loadedProducts = JsonSerializer.Deserialize<List<ProductInfos>>(jsonContent);

                if (loadedProducts != null)
                {
                    // Tri des produits par DaysRemaining
                    var sortedProducts = loadedProducts.OrderBy(p => p.DaysRemaining).ToList();
                    
                    Products.Clear();
                    foreach (var product in sortedProducts)
                    {
                        Products.Add(product);
                    }

                    // Met à jour la collection partagée dans AppData
                    AppData.CurrentProducts.Clear();
                    foreach (var product in Products)
                    {
                        AppData.CurrentProducts.Add(product);
                    }
                    Console.WriteLine($"MainPage a chargé {Products.Count} produits et mis à jour AppData.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des produits (MainPage) : {ex.Message}");
                await DisplayAlert("Erreur", "Impossible de charger les données des produits. " + ex.Message, "OK");
            }
        }
    }
}