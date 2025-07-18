using Microsoft.Maui.Controls;
using perimapp.Models;
using perimapp.Data;
using System.Linq;
using System.Diagnostics; // Pour Debug.WriteLine
using System.Threading.Tasks; // Pour Task.Delay
using perimapp.Pages; // Assurez-vous d'avoir ceci pour nameof(ModifyProductPage)

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
                BindingContext = _productDetail; // Met à jour le BindingContext lorsque ProductDetail est défini
            }
        }

        public DetailsPage()
        {
            InitializeComponent();
            // N'appelez pas ProductDetail = null; ici, car ProductUniqueId n'est pas encore set.
            // Laissez OnAppearing gérer la récupération initiale.
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // S'assure que le ProductUniqueId est bien défini AVANT de tenter de charger le produit
            if (!string.IsNullOrEmpty(ProductUniqueId) && ProductDetail?.ProductUniqueId != ProductUniqueId)
            {
                // Vérifie si le produit n'a pas déjà été chargé ou si l'ID a changé
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

                    // Testez si l'URL est réellement accessible sur Internet (votre code existant)
                    if (!string.IsNullOrEmpty(ProductDetail.url_image))
                    {
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                client.Timeout = TimeSpan.FromSeconds(10);
                                var response = await client.GetAsync(ProductDetail.url_image);
                                if (response.IsSuccessStatusCode)
                                {
                                    Debug.WriteLine($"DetailsPage: L'URL de l'image est accessible ! Statut: {response.StatusCode}");
                                }
                                else
                                {
                                    Debug.WriteLine($"DetailsPage: L'URL de l'image N'EST PAS accessible. Statut: {response.StatusCode}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"DetailsPage: Erreur lors de la vérification de l'URL de l'image : {ex.Message}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("DetailsPage: url_image est nulle ou vide.");
                    }
                }
            }
            else if (string.IsNullOrEmpty(ProductUniqueId))
            {
                Debug.WriteLine("DetailsPage: Aucun ProductUniqueId fourni dans les paramètres de la requête.");
                await DisplayAlert("Erreur", "Aucun ID de produit fourni.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            // Si ProductDetail.ProductUniqueId == ProductUniqueId, cela signifie que le produit est déjà chargé.
            // Cela évite de recharger inutilement si la page réapparaît sans changement d'ID.
        }

        private async void OnImageTapped(object sender, TappedEventArgs e)
        {
            if (ProductDetail != null && !string.IsNullOrEmpty(ProductDetail.url_image))
            {
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

        // MODIFICATION ICI : Passez le ProductUniqueId à ModifyProductPage
        private async void OnModifyButtonClicked(object sender, EventArgs e)
        {
            if (ProductDetail != null && !string.IsNullOrEmpty(ProductDetail.ProductUniqueId))
            {
                // Construit la chaîne de requête avec l'ID
                string route = $"{nameof(ModifyProductPage)}?ProductUniqueId={ProductDetail.ProductUniqueId}";
                Debug.WriteLine($"DetailsPage: Navigating to {route}");
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                // Gérer le cas où le ProductDetail n'est pas disponible (normalement, cela ne devrait pas arriver si OnAppearing fonctionne bien)
                await DisplayAlert("Erreur", "Impossible de modifier le produit. ID manquant.", "OK");
            }
        }
    }
}