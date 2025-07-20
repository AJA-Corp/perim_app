using System.Diagnostics; // Pour Debug.WriteLine
using System.Linq;
using System.Threading.Tasks; // Pour Task.Delay
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.Pages; // Assurez-vous d'avoir ceci pour nameof(ModifyProductPage)

namespace perimapp.Pages
{
    [QueryProperty(nameof(ProductUniqueId), "ProductUniqueId")]
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
            }
        }

        public DetailsPage()
        {
            InitializeComponent();
            BindingContext = this;
            // N'appelez pas ProductDetail = null; ici, car ProductUniqueId n'est pas encore set.
            // Laissez OnAppearing gérer la récupération initiale.
        }

        private void LoadProductDetail()
        {
            // Recherche dans la liste chargée
            ProductDetail = AppData.CurrentProducts.FirstOrDefault(p => p.Id == _productId);

            // S'assure que le ProductUniqueId est bien défini AVANT de tenter de charger le produit
            if (
                !string.IsNullOrEmpty(ProductUniqueId)
                && ProductDetail?.ProductUniqueId != ProductUniqueId
            )
            {
                // Vérifie si le produit n'a pas déjà été chargé ou si l'ID a changé
                ProductDetail = AppData.CurrentProducts.FirstOrDefault(p =>
                    p.ProductUniqueId == ProductUniqueId
                );

                if (ProductDetail == null)
                {
                    Debug.WriteLine(
                        "DetailsPage: Produit non trouvé avec ProductUniqueId : " + ProductUniqueId
                    );
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
                    Debug.WriteLine($"DetailsPage: Produit chargé : {ProductDetail.Name}");
                    Debug.WriteLine(
                        $"DetailsPage: URL de l'image (du modèle) : {ProductDetail.UrlImage}"
                    );

                    // Testez si l'URL est réellement accessible sur Internet (votre code existant)
                    if (!string.IsNullOrEmpty(ProductDetail.UrlImage))
                    {
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                client.Timeout = TimeSpan.FromSeconds(10);
                                var response = await client.GetAsync(ProductDetail.UrlImage);
                                if (response.IsSuccessStatusCode)
                                {
                                    Debug.WriteLine(
                                        $"DetailsPage: L'URL de l'image est accessible ! Statut: {response.StatusCode}"
                                    );
                                }
                                else
                                {
                                    Debug.WriteLine(
                                        $"DetailsPage: L'URL de l'image N'EST PAS accessible. Statut: {response.StatusCode}"
                                    );
                                }
                            }
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
                        Debug.WriteLine("DetailsPage: url_image est nulle ou vide.");
                    }
                }
            }
            else
            {
                Debug.WriteLine(
                    "DetailsPage: Aucun ProductUniqueId fourni dans les paramètres de la requête."
                );
                await DisplayAlert("Erreur", "Aucun ID de produit fourni.", "OK");
                await Shell.Current.GoToAsync("..");
        }

        private async void OnImageTapped(object sender, TappedEventArgs e)
        {
            if (ProductDetail != null && !string.IsNullOrEmpty(ProductDetail.UrlImage))
            {
                try
                {
                    await Launcher.OpenAsync(new Uri(ProductDetail.UrlImage));
                    Debug.WriteLine(
                        $"DetailsPage: Tentative d'ouverture de l'URL de l'image dans le navigateur : {ProductDetail.UrlImage}"
                    );
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
                // Construit la chaîne de requête avec l'ID
                string route =
                    $"{nameof(ModifyProductPage)}?ProductUniqueId={ProductDetail.ProductUniqueId}";
                Debug.WriteLine($"DetailsPage: Navigating to {route}");
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                // Gérer le cas où le ProductDetail n'est pas disponible (normalement, cela ne devrait pas arriver si OnAppearing fonctionne bien)
                await DisplayAlert(
                    "Erreur",
                    "Impossible de modifier le produit. ID manquant.",
                    "OK"
                );
            }
        }
    }
}
