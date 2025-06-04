// MainPage.xaml.cs
using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Controls;
using System;
using System.IO;
using perimapp.Models; // Assurez-vous que ce using est correct pour votre dossier Models
using System.Threading.Tasks;
using System.Collections.Generic;

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

        private async Task LoadProductsAsync()
        {
            try
            {
                using Stream fileStream = await FileSystem.OpenAppPackageFileAsync("response.json");
                using StreamReader reader = new StreamReader(fileStream);
                string jsonContent = await reader.ReadToEndAsync();

                // Désérialise en une liste de ProductMainPage
                List<ProductMainPage>? loadedProducts = JsonSerializer.Deserialize<List<ProductMainPage>>(jsonContent);

                if (loadedProducts != null)
                {
                    foreach (var product in loadedProducts)
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