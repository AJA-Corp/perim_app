using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Controls;
using System;
using System.IO;
using perimapp.Models; // Assurez-vous que ce using est correct pour votre dossier Models
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using perimapp.Data; // Ajouté pour accéder à AppData

namespace perimapp.Pages
{
    public partial class MainPage : ContentPage
    {
        // La collection est de type ProductMainPage, liée à la CollectionView
        public ObservableCollection<ProductMainPage> Products { get; set; }

        public MainPage()
        {
            InitializeComponent();
            Products = new ObservableCollection<ProductMainPage>();
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

        private async Task LoadProductsAsync()
        {
            try
            {
                // Chargement du fichier responseMainPage.json depuis les ressources de l'application
                using Stream fileStream = await FileSystem.OpenAppPackageFileAsync("responseMainPage.json");
                using StreamReader reader = new StreamReader(fileStream);
                string jsonContent = await reader.ReadToEndAsync();
                
                // Désérialisation du JSON en une liste de ProductMainPage
                List<ProductMainPage>? loadedProducts = JsonSerializer.Deserialize<List<ProductMainPage>>(jsonContent);

                if (loadedProducts != null)
                {
                    // Tri des produits par DaysRemaining comme vous le faites
                    var sortedProducts = loadedProducts.OrderBy(p => p.DaysRemaining).ToList();
                    
                    // Ajout des produits à la collection locale de MainPage (pour l'affichage)
                    Products.Clear(); // S'assurer que la collection est vide avant de la remplir
                    foreach (var product in sortedProducts)
                    {
                        Products.Add(product);
                    }

                    // *** NOUVEAU : Met à jour la collection partagée dans AppData ***
                    // Cela garantit que AppData.CurrentProducts reflète fidèlement
                    // les produits actuellement affichés dans MainPage.
                    AppData.CurrentProducts.Clear(); // Effacer d'abord si elle contient des données précédentes
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