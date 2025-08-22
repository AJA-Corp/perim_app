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
        private string _sortButtonText;
        public string SortButtonText
        {
            get => _sortButtonText;
            set
            {
                if (_sortButtonText != value)
                {
                    _sortButtonText = value;
                    OnPropertyChanged(nameof(SortButtonText));
                }
            }
        }
        
        // La collection de produits est une référence à AppData.CurrentProducts
        public ObservableCollection<ProductInfos> Products => AppData.CurrentProducts;

        // Propriété pour le nombre de produits affichés, liée à l'interface utilisateur
        private int _displayedProductsCount;
        public int DisplayedProductsCount
        {
            get => _displayedProductsCount;
            set
            {
                if (_displayedProductsCount != value)
                {
                    _displayedProductsCount = value;
                    OnPropertyChanged(nameof(DisplayedProductsCount)); // Notifie l'UI du changement
                }
            }
        }

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

            //Initialisation de la propriété du texte du bouton
            SortButtonText = "Tri: DLC (proche)";
        }

        // Chargement des produits lors de l'apparition de la page
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // CODE MODIFIÉ : Assurez-vous que cette ligne est le seul point de chargement
            await LoadProductsAsync();
            
            // Définit le tri par défaut une fois les produits chargés
            SortProducts("DLC (proche)");
            
            // AJOUTEZ CETTE LIGNE POUR VÉRIFIER LE COMPTEUR
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Products.Count après tri : {Products.Count}");
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
            List<ProductInfos> loadedProducts = null;
    
            // 1. Tentez de charger depuis la base de données distante
            try
            {
                string userIdString = await SecureStorage.GetAsync("user_id");
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    loadedProducts = await _productService.GetUserProductsAsync(userId);
                    if (loadedProducts != null && loadedProducts.Count > 0)
                    {
                        Console.WriteLine($"[DEBUG] {loadedProducts.Count} produits chargés depuis la BDD Neon.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Erreur lors du chargement des produits depuis la BDD Neon : {ex.Message}");
            }

            // 2. Si le chargement distant a échoué, tentez le chargement local
            if (loadedProducts == null || loadedProducts.Count == 0)
            {
                Console.WriteLine("[DEBUG] Chargement des produits depuis le fichier JSON local.");
                loadedProducts = await LoadProductsFromJsonAsync();
                if (loadedProducts != null)
                {
                    Console.WriteLine($"[DEBUG] {loadedProducts.Count} produits chargés depuis le fichier JSON.");
                }
            }

            // 3. Mise à jour de la collection observable et du compteur
            if (loadedProducts != null)
            {
                Products.Clear();
                foreach (var product in loadedProducts)
                {
                    Products.Add(product);
                }
                DisplayedProductsCount = Products.Count;
            }
            else
            {
                DisplayedProductsCount = 0;
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

        private async void OnLostProductsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(DeletedProductPage));
        }
        
        // Méthode pour le bouton de tri
        private async void OnSortButtonClicked(object sender, EventArgs e)
        {
            string result = await DisplayActionSheet(
                "Trier par", "Annuler", null, 
                "DLC (proche)", 
                "DLC (lointaine)"
            );

            if (result != "Annuler" && result != null)
            {
                SortProducts(result);
            }
        }

        // Méthode qui gère la logique de tri
        private void SortProducts(string sortOption)
        {
            IEnumerable<ProductInfos> sortedProducts = Products;

            switch (sortOption)
            {
                case "DLC (proche)":
                    sortedProducts = Products.OrderBy(p => p.DaysRemaining).ToList(); // CORRECTION ICI
                    SortButtonText = "Tri: DLC (proche)";
                    break;
                case "DLC (lointaine)":
                    sortedProducts = Products.OrderByDescending(p => p.DaysRemaining).ToList(); // CORRECTION ICI
                    SortButtonText = "Tri: DLC (lointaine)";
                    break;
            }

            // Le foreach peut maintenant s'exécuter sur la liste triée
            Products.Clear();
            foreach (var product in sortedProducts)
            {
                Products.Add(product);
            }
        }
    }
}
