using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;

namespace perimapp.Pages
{
    [QueryProperty(nameof(ProductIdString), "productId")]
    public partial class DetailsPage : ContentPage
    {
        private int _productId;
        public string ProductIdString
        {
            get => _productId.ToString();
            set
            {
                if (int.TryParse(value, out var id))
                {
                    _productId = id;
                    LoadProductDetail();
                }
                else
                {
                    Debug.WriteLine($"DetailsPage: productId invalide : {value}");
                }
            }
        }

        private ProductInfos? _productDetail;
        public ProductInfos? ProductDetail
        {
            get => _productDetail;
            set
            {
                _productDetail = value;
                OnPropertyChanged();
                BindingContext = _productDetail;
            }
        }

        public DetailsPage()
        {
            InitializeComponent();
        }

        private void LoadProductDetail()
        {
            // Recherche dans la liste chargée
            ProductDetail = AppData.CurrentProducts.FirstOrDefault(p => p.Id == _productId);

            if (ProductDetail == null)
            {
                Debug.WriteLine($"DetailsPage: Produit non trouvé avec Id = {_productId}");
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Erreur", "Produit non trouvé.", "OK");
                    await Shell.Current.GoToAsync("..");
                });
            }
            else
            {
                Debug.WriteLine($"DetailsPage: Produit chargé : {ProductDetail.Name}");
                CheckImageUrlAsync(ProductDetail.UrlImage);
            }
        }

        private async void CheckImageUrlAsync(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                    var response = await client.GetAsync(url);
                    Debug.WriteLine(
                        response.IsSuccessStatusCode
                            ? $"DetailsPage: L'URL de l'image est accessible."
                            : $"DetailsPage: L'URL de l'image n'est pas accessible. Statut: {response.StatusCode}"
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"DetailsPage: Erreur lors de la vérification de l'URL de l'image : {ex.Message}"
                    );
                }
            }
            else
            {
                Debug.WriteLine("DetailsPage: URL d'image vide ou nulle.");
            }
        }

        private async void OnImageTapped(object sender, TappedEventArgs e)
        {
            if (ProductDetail != null && !string.IsNullOrEmpty(ProductDetail.UrlImage))
            {
                try
                {
                    await Launcher.OpenAsync(new Uri(ProductDetail.UrlImage));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"DetailsPage: Erreur lors de l'ouverture de l'URL de l'image : {ex.Message}"
                    );
                    await DisplayAlert(
                        "Erreur",
                        "Impossible d'ouvrir l'image dans le navigateur.",
                        "OK"
                    );
                }
            }
            else
            {
                await DisplayAlert("Info", "Pas d'image à afficher ou URL manquante.", "OK");
            }
        }

        private async void OnModifyButtonClicked(object sender, EventArgs e)
        {
            if (ProductDetail != null)
            {
                string route = $"{nameof(ModifyProductPage)}?productId={ProductDetail.Id}";
                Debug.WriteLine($"DetailsPage: Navigation vers {route}");
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                await DisplayAlert(
                    "Erreur",
                    "Impossible de modifier le produit. ID manquant.",
                    "OK"
                );
            }
        }
    }
}
