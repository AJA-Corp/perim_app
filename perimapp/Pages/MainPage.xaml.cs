// MainPage.xaml.cs
using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Controls;
using System;
using System.IO;
using perimapp.Models; // Assurez-vous que ce using est correct pour votre dossier Models
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace perimapp.Pages
{
    public partial class MainPage : ContentPage
    {
        // La collection doit maintenant être de type ProductMainPage
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
                using Stream fileStream = await FileSystem.OpenAppPackageFileAsync("response.json");
                using StreamReader reader = new StreamReader(fileStream);
                string jsonContent = await reader.ReadToEndAsync();
                
                List<ProductMainPage>? loadedProducts = JsonSerializer.Deserialize<List<ProductMainPage>>(jsonContent);

                if (loadedProducts != null)
                {
                    var sortedProducts = loadedProducts.OrderBy(p => p.DaysRemaining).ToList();
                    
                    foreach (var product in sortedProducts)
                    {
                        Products.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des produits : {ex.Message}");
                await DisplayAlert("Erreur", "Impossible de charger les données des produits. " + ex.Message, "OK");
            }
        }
    }
}