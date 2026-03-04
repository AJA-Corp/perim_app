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
using System.Windows.Input;

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
        //Propriété pour le pull to refresh 
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    OnPropertyChanged(nameof(IsRefreshing));
                }
            }
        }
        public ICommand RefreshCommand { get; }

        
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
            //Refresh la MainPage
            RefreshCommand = new Command(async () => await OnRefresh());
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
            
            //rafraichissement auto
             // IsRefreshing = true;
             // await OnRefresh();
            
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du chargement des produits : {ex.Message}");
                    await DisplayAlert("Erreur", "Impossible de charger les produits. " + ex.Message, "OK");
                }
            }
        }
        
        private async Task OnRefresh()
        {
            try
            {
                // Recharge la liste des produits
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du rafraîchissement : {ex.Message}");
                await DisplayAlert("Erreur", "Impossible d'actualiser les produits.", "OK");
            }
            finally
            {    
                // Arrête l'animation du RefreshView
                IsRefreshing = false;
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

        private double _lastScrollY = 0;
        private bool _isButtonVisible = true;

        private async void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            double currentY = e.VerticalOffset;
            double delta = currentY - _lastScrollY;

            if (Math.Abs(delta) < 5) return;

            if (delta > 0 && _isButtonVisible)
            {
                _isButtonVisible = false;
                await FloatingBinButton.TranslateToAsync(0, 100, 250, Easing.CubicIn);
            }
            else if (delta < 0 && !_isButtonVisible)
            {
                _isButtonVisible = true;
                await FloatingBinButton.TranslateToAsync(0, 0, 250, Easing.CubicOut);
            }

            _lastScrollY = currentY;
        }
    }
}
