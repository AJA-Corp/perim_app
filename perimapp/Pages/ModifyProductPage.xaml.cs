using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;

namespace perimapp.Pages
{
    [QueryProperty(nameof(ProductUniqueId), "ProductUniqueId")]
    public partial class ModifyProductPage : ContentPage, INotifyPropertyChanged
    {
        private string _productUniqueId;
        public string ProductUniqueId
        {
            get => _productUniqueId;
            set
            {
                /*
                if (int.TryParse(value, out int id))
                {
                    _productId = id;
                }
                else
                {
                    Debug.WriteLine($"ModifyProductPage: productId invalide : {value}");
                }
                */
                _productUniqueId = value;
            }
        }

        private ProductInfos? _currentProduct;
        public ProductInfos? CurrentProduct
        {
            get => _currentProduct;
            set
            {
                _currentProduct = value;
                OnPropertyChanged();
                // BindingContext = _currentProduct;
            }
        }

        public ModifyProductPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadProductForModification();
        }

        private void LoadProductForModification()
        {
            CurrentProduct = AppData.CurrentProducts.FirstOrDefault(p =>
                p.ProductUniqueId == _productUniqueId
            );

            if (CurrentProduct == null)
            {
                Debug.WriteLine(
                    $"ModifyProductPage: Produit non trouvé avec Id = {_productUniqueId}"
                );
                /*
                await DisplayAlert("Erreur", "Produit à modifier non trouvé.", "OK");
                await Shell.Current.GoToAsync("..");
                */
            }
            else
            {
                Debug.WriteLine($"ModifyProductPage: Produit chargé : {CurrentProduct.Name}");
                /*
                await CheckImageUrlAsync(CurrentProduct.UrlImage);
                */
            }
        }

        private async Task CheckImageUrlAsync(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                    var response = await client.GetAsync(url);
                    Debug.WriteLine(
                        response.IsSuccessStatusCode
                            ? $"ModifyProductPage: URL image accessible."
                            : $"ModifyProductPage: URL image inaccessible. Status: {response.StatusCode}"
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"ModifyProductPage: Erreur vérification URL image : {ex.Message}"
                    );
                }
            }
            else
            {
                Debug.WriteLine("ModifyProductPage: URL image vide ou nulle.");
            }
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            if (CurrentProduct == null)
            {
                await DisplayAlert("Erreur", "Aucun produit chargé.", "OK");
                return;
            }

            // Appel au service
            var service = new NeonProductService();
            bool success = await service.UpdateUserProductAsync(CurrentProduct);

            if (success)
            {
                // Met à jour localement le produit dans AppData
                var index = AppData
                    .CurrentProducts.ToList()
                    .FindIndex(p => p.Id == CurrentProduct.Id);
                if (index != -1)
                {
                    AppData.CurrentProducts[index] = CurrentProduct;
                }

                await DisplayAlert("Succès", "Produit mis à jour avec succès.", "OK");
                await Shell.Current.GoToAsync($".."); // Retour à la page précédente
            }
            else
            {
                await DisplayAlert("Erreur", "La mise à jour a échoué.", "OK");
            }
        }

        // Implémentation INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
