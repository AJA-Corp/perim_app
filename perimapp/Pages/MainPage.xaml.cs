using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services; 
using Microsoft.Maui.Storage;
using Microsoft.Maui.Networking;

namespace perimapp.Pages
{
    public partial class MainPage : ContentPage
    
    {
        private readonly NeonProductService _neonProductService;
        private readonly LocalProductService _localProductService;

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
                    OnPropertyChanged(nameof(DisplayedProductsCount)); 
                }
            }
        }

        // Constructeur avec Injection de Dépendances (DI)
        public MainPage(NeonProductService neonProductService, LocalProductService localProductService)
        {
            InitializeComponent();
            
            // Initialisation des champs readonly
            _neonProductService = neonProductService;
            _localProductService = localProductService;

            // L'ID utilisateur devrait maintenant être géré par l'authentification/le service utilisateur
            int savedUserId = Preferences.Default.Get("UserId", -1);
            Console.WriteLine(
                $"[DEBUG] ID utilisateur récupéré depuis Preferences : {savedUserId}"
            );
            NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = this;

            // Initialiser la propriété avec une valeur par défaut
            SortButtonText = "Tri: DLC (proche)";
        }

        // Chargement des produits lors de l'apparition de la page
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // CODE MODIFIÉ : Assurez-vous que cette ligne est le seul point de chargement
            await LoadProductsAsync();
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

                    // Assurez-vous que DetailsPage est défini dans AppShell
                    // await Shell.Current.GoToAsync(
                    //     $"{nameof(DetailsPage)}?ProductUniqueId={selectedProduct.ProductUniqueId}"
                    // );
                }
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                List<ProductInfos> allProducts;

                    // Récupérer l'ID utilisateur
                    string userIdString = await SecureStorage.GetAsync("user_id");

                    // Vérifie si Internet est dispo
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
                        Console.WriteLine($"Erreur de récupération serveur : {ex.Message}. Chargement local...");
                        allProducts = await _localProductService.LoadProductsAsync();
                    }
                }
                else
                {
                    // Pas de connexion → produits locaux uniquement
                    allProducts = await _localProductService.LoadProductsAsync();
                }
                // Sérialise la liste pour voir le contenu complet, y compris les champs 'State'.
                var jsonDebug = JsonSerializer.Serialize(allProducts, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"[DEBUG-JSON] Liste complète des produits chargée ({allProducts.Count} éléments) :");
                Console.WriteLine(jsonDebug);

                // ÉTAPE 1 : Filtrer pour afficher UNIQUEMENT les produits 'Active'
                var activeProducts = allProducts
                                        .Where(p => p.State == "Active") // FILTRE ESSENTIEL
                                        .OrderBy(p => p.DaysRemaining) // Tri par défaut
                                        .ToList();

                // Mise à jour de la liste globale et de l'UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AppData.CurrentProducts.Clear();

                    // Remplir avec la liste filtrée et triée
                    foreach (var product in activeProducts)
                        AppData.CurrentProducts.Add(product);

                    DisplayedProductsCount = AppData.CurrentProducts.Count;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des produits : {ex.Message}");
                await DisplayAlert("Erreur", "Impossible de charger les produits. " + ex.Message, "OK");
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
                    sortedProducts = Products.OrderBy(p => p.DaysRemaining).ToList(); 
                    SortButtonText = "Tri: DLC (proche)";
                    break;
                case "DLC (lointaine)":
                    sortedProducts = Products.OrderByDescending(p => p.DaysRemaining).ToList(); 
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
