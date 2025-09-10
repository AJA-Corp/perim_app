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
using Microsoft.Maui.Storage;
using Microsoft.Maui.Networking;
using System.Windows.Input;

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
                            // Charger les produits depuis le serveur
                            var serveurProducts = await _productService.GetUserProductsAsync(userId);

                            //  Remplacer le cache local par les produits en ligne
                            await localService.SaveProductsAsync(serveurProducts);

                            //  Utiliser ces produits pour l'affichage
                            products = serveurProducts;
                        }
                        catch
                        {
                            // Si le serveur ne répond pas, on retombe sur le local
                            products = await localService.LoadProductsAsync();
                        }
                    }
                    else
                    {
                        //  Pas de connexion → produits locaux uniquement
                        products = await localService.LoadProductsAsync();
                    }

                    // Mise à jour de la liste globale et de l'UI
                    AppData.CurrentProducts.Clear();

                    // NOUVEAU CODE : S'assurer que les produits sont triés à l'affichage initial
                    foreach (var product in products.OrderBy(p => p.DaysRemaining))
                        AppData.CurrentProducts.Add(product);

                    DisplayedProductsCount = AppData.CurrentProducts.Count;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du chargement des produits : {ex.Message}");
                    await DisplayAlert("Erreur", "Impossible de charger les produits. " + ex.Message, "OK");
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