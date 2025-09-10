using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using perimapp.Helpers;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Networking;
using System.Windows.Input;
using System.Threading;

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
                    OnPropertyChanged(nameof(DisplayedProductsCount)); // Notifie l'UI du changement
                }
            }
        }

        private readonly NeonProductService _productService = new NeonProductService();
        private CancellationTokenSource _loadingCancellationToken;
        private bool _isFirstLoad = true;


        public MainPage()
        {
            InitializeComponent();
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
            
            // Only load products if this is the first time or if cache is invalid
            if (_isFirstLoad || !ProductCacheService.IsCacheValid())
            {
                await LoadProductsAsync();
                _isFirstLoad = false;
            }
            else
            {
                // Use cached data for instant loading
                var cachedProducts = ProductCacheService.GetCachedProducts();
                if (cachedProducts != null)
                {
                    UpdateProductCollection(cachedProducts);
                }
            }
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
            // Cancel any existing loading operation
            _loadingCancellationToken?.Cancel();
            _loadingCancellationToken = new CancellationTokenSource();
            var cancellationToken = _loadingCancellationToken.Token;

            try
            {
                // Check cache first for instant loading
                var cachedProducts = ProductCacheService.GetCachedProducts();
                if (cachedProducts != null)
                {
                    UpdateProductCollection(cachedProducts);
                    return; // Use cached data, don't hit the database
                }

                var localService = new LocalProductService();
                List<ProductInfos> products;

                // Récupérer l'ID utilisateur
                string userIdString = await SecureStorage.GetAsync("user_id");

                // Vérifie si Internet est dispo
                bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

                if (hasInternet && !string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    try
                    {
                        // Check for cancellation before network call
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        // Charger les produits depuis le serveur en arrière-plan
                        products = await Task.Run(async () => await _productService.GetUserProductsAsync(userId), cancellationToken);

                        // Check for cancellation before updating cache
                        cancellationToken.ThrowIfCancellationRequested();

                        // Update cache with fresh data
                        ProductCacheService.SetCachedProducts(products);
                        
                        // Remplacer le cache local par les produits en ligne
                        await localService.SaveProductsAsync(products);
                    }
                    catch (OperationCanceledException)
                    {
                        return; // Operation was cancelled, exit gracefully
                    }
                    catch
                    {
                        // Si le serveur ne répond pas, on retombe sur le local
                        products = await localService.LoadProductsAsync();
                        ProductCacheService.SetCachedProducts(products);
                    }
                }
                else
                {
                    // Pas de connexion → produits locaux uniquement
                    products = await localService.LoadProductsAsync();
                    ProductCacheService.SetCachedProducts(products);
                }

                // Check for cancellation before UI update
                cancellationToken.ThrowIfCancellationRequested();

                // Update UI on main thread
                UpdateProductCollection(products);
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled, no action needed
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des produits : {ex.Message}");
                await DisplayAlert("Erreur", "Impossible de charger les produits. " + ex.Message, "OK");
            }
        }

        private void UpdateProductCollection(List<ProductInfos> products)
        {
            // Use efficient collection update instead of Clear() + AddRange()
            var sortedProducts = products.OrderBy(p => p.DaysRemaining).ToList();
            
            // Update the collection efficiently
            AppData.CurrentProducts.ReplaceWith(sortedProducts);
            
            DisplayedProductsCount = AppData.CurrentProducts.Count;
        }
        
        private async Task OnRefresh()
        {
            try
            {
                // Invalidate cache to force fresh data
                ProductCacheService.InvalidateCache();
                
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

        // Méthode qui gère la logique de tri optimisée
        private void SortProducts(string sortOption)
        {
            var currentProducts = Products.ToList(); // Create a snapshot to avoid collection modification

            List<ProductInfos> sortedProducts;

            switch (sortOption)
            {
                case "DLC (proche)":
                    sortedProducts = currentProducts.OrderBy(p => p.DaysRemaining).ToList(); 
                    SortButtonText = "Tri: DLC (proche)";
                    break;
                case "DLC (lointaine)":
                    sortedProducts = currentProducts.OrderByDescending(p => p.DaysRemaining).ToList(); 
                    SortButtonText = "Tri: DLC (lointaine)";
                    break;
                default:
                    return; // No change needed
            }

            // Efficient collection update
            Products.ReplaceWith(sortedProducts);
        }
    }
}