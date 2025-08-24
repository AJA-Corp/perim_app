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
                // Charger les produits depuis le fichier local
                List<ProductInfos> loadedProducts = await localService.LoadProductsAsync();

                // Si l’utilisateur est en ligne, récupérer les produits serveur et fusionner
                string userIdString = await SecureStorage.GetAsync("user_id");
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    var serveurProducts = await _productService.GetUserProductsAsync(userId);

                    foreach (var p in serveurProducts)
                    {
                        // Ajout seulement si non présent localement (pour les doublons)
                        if (!loadedProducts.Any(x => x.ProductUniqueId == p.ProductUniqueId))
                        {
                            loadedProducts.Add(p);
                            // Sauvegarde les nouveaux produits DB dans le fichier local
                            await localService.AddProductAsync(p);
                        }
                    }
                }

                // Mise à jour de la collection observable et du compteur
                Products.Clear();
                foreach (var product in loadedProducts.OrderBy(p => p.DaysRemaining))
                {
                    Products.Add(product);
                }
                DisplayedProductsCount = Products.Count;
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
