using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks; // Pour Task.Delay
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.Pages; // Assurez-vous d'avoir ceci pour nameof(ModifyProductPage)
using perimapp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace perimapp.Pages
{
    // MODIFICATION : Implémentation de INotifyPropertyChanged
    [QueryProperty(nameof(ProductUniqueId), "ProductUniqueId")]
    public partial class DetailsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly NeonProductService _neonService;
        private readonly LocalProductService _localService;
        private readonly NeonUserService _userService;

        private string _productUniqueId;
        public string ProductUniqueId
        {
            get => _productUniqueId;
            set
            {
                _productUniqueId = value;
                LoadProductDetail();
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
        
        // Constructeur avec injection des services
        public DetailsPage(NeonProductService neonService, LocalProductService localService, NeonUserService userService)
        {
            InitializeComponent();
            _neonService = neonService;
            _localService = localService;
            _userService = userService;
            BindingContext = this;
        }

        // Implémentation de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void LoadProductDetail()
        {
            // Recherche dans la liste chargée
            ProductDetail = AppData.CurrentProducts.FirstOrDefault(p =>
                p.ProductUniqueId == _productUniqueId
            );
            
            // Vérification de sécurité supplémentaire : Le produit doit être "Active" pour voir les détails
            if (ProductDetail != null && ProductDetail.State != "Active")
            {
                Debug.WriteLine($"DetailsPage: Tentative d'accès à un produit non actif ({ProductDetail.State}). Redirection.");
                await DisplayAlertAsync("Erreur", "Ce produit n'est plus actif.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

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
                    await DisplayAlertAsync("Erreur", "Produit non trouvé.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            
            if (ProductDetail != null)
            {
                Debug.WriteLine($"DetailsPage: Produit chargé : {ProductDetail.Name}");
                CheckImageUrlAsync(ProductDetail.UrlImage);
            }
            else
            {
                Debug.WriteLine(
                    "DetailsPage: Aucun ProductUniqueId fourni ou produit non trouvé."
                );
                if (string.IsNullOrEmpty(ProductUniqueId))
                {
                    await DisplayAlertAsync("Erreur", "Aucun ID de produit fourni.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
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
                    Debug.WriteLine($"DetailsPage: Erreur de connexion/timeout lors de la vérification de l'image : {ex.Message}");
                    
                    if (!string.IsNullOrEmpty(ProductDetail?.UrlImage))
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
                        catch (Exception innerEx)
                        {
                            Debug.WriteLine($"DetailsPage: Erreur : {innerEx.Message}");
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
                 Debug.WriteLine("DetailsPage: url_image est nulle ou vide.");
            }
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
                    await DisplayAlertAsync(
                        "Erreur",
                        "Impossible d'ouvrir l'image dans le navigateur.",
                        "OK"
                    );
                }
            }
            else
            {
                await DisplayAlertAsync("Info", "Pas d'image à afficher ou URL manquante.", "OK");
            }
        }

        private async void OnModifyButtonClicked(object sender, EventArgs e)
        {
            if (ProductDetail != null)
            {
                string route =
                    $"{nameof(ModifyProductPage)}?ProductUniqueId={ProductDetail.ProductUniqueId}";
                Debug.WriteLine($"DetailsPage: Navigating to {route}");
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                await DisplayAlertAsync(
                    "Erreur",
                    "Impossible de modifier le produit. ID manquant.",
                    "OK"
                );
            }
        }

        // Bouton SUPPRIMER
        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            if (ProductDetail == null) return;

            bool confirmed = await DisplayAlertAsync(
                "Supprimer le produit",
                $"Êtes-vous sûr de vouloir supprimer {ProductDetail.Name}? Il sera archivé temporairement.",
                "Oui",
                "Non"
            );

            if (confirmed)
            {
                string idToPass = ProductDetail.Id.ToString();

                bool localSuccess = await _localService.UpdateProductStateAsync(idToPass, "Deleted");
                bool neonSuccess = await _neonService.UpdateProductStateAsync(idToPass, "Deleted");

                if (localSuccess || neonSuccess)
                {
                    bool isExpired = ProductDetail.Dlc.Date < DateTime.Today;

                    if (isExpired)
                    {
                        string? userIdStr = await SecureStorage.GetAsync("user_id");
                        if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                        {
                            await _userService.IncrementLostProductCountAsync(userId);
                            Debug.WriteLine($"DetailsPage: Produit périmé supprimé, compteur incrémenté pour user {userId}");
                        }
                    }

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var productInList = AppData.CurrentProducts.FirstOrDefault(p => p.Id == ProductDetail.Id);
                        if (productInList != null)
                        {
                            productInList.State = "Deleted";
                            productInList.DeletedAt = DateTime.UtcNow;
                        }

                        ProductDetail.State = "Deleted";
                        ProductDetail.DeletedAt = DateTime.UtcNow;

                        await Shell.Current.GoToAsync(nameof(MainPage)); 
                    });
                }
                else
                {
                    await DisplayAlertAsync("Erreur", "Impossible de supprimer le produit.", "OK");
                }
            }
        }
    }
}