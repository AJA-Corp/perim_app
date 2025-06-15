using Microsoft.Maui.Controls;
using perimapp.Models;
using perimapp.Data;
using System.Linq;
using System.Diagnostics; // Pour Debug.WriteLine
using System.Threading.Tasks; // Pour Task.Delay

namespace perimapp.Pages
{
    [QueryProperty(nameof(ProductUniqueId), "productUniqueId")]
    public partial class DetailsPage : ContentPage
    {
        public string? ProductUniqueId { get; set; }

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

        protected override async void OnAppearing() // Changement en async void
        {
            base.OnAppearing();

            if (!string.IsNullOrEmpty(ProductUniqueId))
            {
                ProductDetail = AppData.CurrentProducts.FirstOrDefault(p => p.ProductUniqueId == ProductUniqueId);

                if (ProductDetail == null)
                {
                    Debug.WriteLine("DetailsPage: Produit non trouvé avec ProductUniqueId : " + ProductUniqueId);
                    await DisplayAlert("Erreur", "Produit non trouvé.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    Debug.WriteLine($"DetailsPage: Produit chargé : {ProductDetail.product_name}");
                    Debug.WriteLine($"DetailsPage: URL de l'image (du modèle) : {ProductDetail.url_image}");

                    // Testez si l'URL est réellement accessible sur Internet
                    if (!string.IsNullOrEmpty(ProductDetail.url_image))
                    {
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                // Ajouter un timeout pour éviter de bloquer indéfiniment
                                client.Timeout = TimeSpan.FromSeconds(10);
                                var response = await client.GetAsync(ProductDetail.url_image);
                                if (response.IsSuccessStatusCode)
                                {
                                    Debug.WriteLine($"DetailsPage: L'URL de l'image est accessible ! Statut: {response.StatusCode}");
                                }
                                else
                                {
                                    Debug.WriteLine($"DetailsPage: L'URL de l'image N'EST PAS accessible. Statut: {response.StatusCode}");
                                    // Afficher l'erreur à l'utilisateur si c'est critique
                                    // await DisplayAlert("Erreur Image", $"L'image du produit n'a pas pu être chargée. Statut: {response.StatusCode}", "OK");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"DetailsPage: Erreur lors de la vérification de l'URL de l'image : {ex.Message}");
                            // await DisplayAlert("Erreur Image", $"Une erreur est survenue lors du chargement de l'image: {ex.Message}", "OK");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("DetailsPage: url_image est nulle ou vide.");
                    }
                }
            }
            else
            {
                Debug.WriteLine("DetailsPage: Aucun ProductUniqueId fourni dans les paramètres de la requête.");
                await DisplayAlert("Erreur", "Aucun ID de produit fourni.", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        // Nouveau gestionnaire d'événements pour le TapGestureRecognizer sur l'image
        private async void OnImageTapped(object sender, TappedEventArgs e)
        {
            if (ProductDetail != null && !string.IsNullOrEmpty(ProductDetail.url_image))
            {
                // Tente d'ouvrir l'URL de l'image dans le navigateur par défaut
                try
                {
                    await Launcher.OpenAsync(new Uri(ProductDetail.url_image));
                    Debug.WriteLine($"DetailsPage: Tentative d'ouverture de l'URL de l'image dans le navigateur : {ProductDetail.url_image}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DetailsPage: Erreur lors de l'ouverture de l'URL de l'image : {ex.Message}");
                    await DisplayAlert("Erreur", "Impossible d'ouvrir l'image dans le navigateur.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Info", "Pas d'image à afficher ou URL manquante.", "OK");
            }
        }

        private async void OnModifyButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ModifyProductPage));
        }
    }
}