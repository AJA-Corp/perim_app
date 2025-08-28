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

namespace perimapp.Pages
{
    public partial class MainPage : ContentPage
    {
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
            Console.WriteLine($"[DEBUG] ID utilisateur récupéré depuis Preferences : {savedUserId}");
            NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = this;

            // CODE MODIFIÉ : Supprimé le "_ = LoadProductsAsync();" du constructeur//
            // Le chargement géré par la méthode OnAppearing()
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
    }
}
