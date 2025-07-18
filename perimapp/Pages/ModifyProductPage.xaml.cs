using Microsoft.Maui.Controls;
using perimapp.Models; // Assurez-vous que votre modèle ProductInfos est défini ici
using perimapp.Data;   // Pour accéder à AppData.CurrentProducts
using System.ComponentModel; // Nécessaire pour INotifyPropertyChanged
using System.Runtime.CompilerServices; // Nécessaire pour [CallerMemberName]
using System.Linq; // Nécessaire pour FirstOrDefault
using System.Diagnostics; // Pour Debug.WriteLine

namespace perimapp.Pages
{
    // Indique que cette page peut recevoir un paramètre de requête nommé "ProductUniqueId"
    // et l'assigner à la propriété publique ProductUniqueId de cette page.
    [QueryProperty(nameof(ProductUniqueId), "ProductUniqueId")]
    public partial class ModifyProductPage : ContentPage, INotifyPropertyChanged
    {
        private string? _productUniqueId;
        public string? ProductUniqueId
        {
            get => _productUniqueId;
            set
            {
                if (_productUniqueId != value)
                {
                    _productUniqueId = value;
                    OnPropertyChanged(); // Notifie les éventuels bindings que ProductUniqueId a changé
                    LoadProductForModification(_productUniqueId); // Tente de charger le produit avec ce nouvel ID
                }
            }
        }

        // Cette propriété contiendra l'objet ProductInfos complet une fois qu'il sera chargé.
        // C'est à cette propriété que votre XAML sera lié.
        private ProductInfos? _currentProduct;
        public ProductInfos? CurrentProduct
        {
            get => _currentProduct;
            set
            {
                if (_currentProduct != value)
                {
                    _currentProduct = value;
                    OnPropertyChanged(); // Notifie les bindings dans le XAML
                    BindingContext = _currentProduct; // Lie le XAML de la page à cet objet ProductInfos
                }
            }
        }

        public ModifyProductPage()
        {
            InitializeComponent();
            // Le BindingContext est défini dynamiquement lorsque CurrentProduct est chargé.
        }

        // Cette méthode est appelée lorsque ProductUniqueId est défini par la navigation.
        private async void LoadProductForModification(string? uniqueId)
        {
            if (!string.IsNullOrEmpty(uniqueId))
            {
                // Cherche le produit correspondant dans votre liste de produits actuelle
                ProductInfos? product = AppData.CurrentProducts.FirstOrDefault(p => p.ProductUniqueId == uniqueId);

                if (product != null)
                {
                    // Si le produit est trouvé, assigne-le à CurrentProduct, ce qui mettra à jour le BindingContext.
                    CurrentProduct = product;
                    Debug.WriteLine($"ModifyProductPage: Produit à modifier chargé : {CurrentProduct.product_name}");

                    // Optionnel: Vérifier l'URL de l'image comme dans DetailsPage, si nécessaire
                    if (!string.IsNullOrEmpty(CurrentProduct.url_image))
                    {
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                client.Timeout = TimeSpan.FromSeconds(10);
                                var response = await client.GetAsync(CurrentProduct.url_image);
                                if (response.IsSuccessStatusCode)
                                {
                                    Debug.WriteLine($"ModifyProductPage: L'URL de l'image est accessible ! Statut: {response.StatusCode}");
                                }
                                else
                                {
                                    Debug.WriteLine($"ModifyProductPage: L'URL de l'image N'EST PAS accessible. Statut: {response.StatusCode}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"ModifyProductPage: Erreur lors de la vérification de l'URL de l'image : {ex.Message}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ModifyProductPage: url_image est nulle ou vide.");
                    }
                }
                else
                {
                    // Gérer le cas où le produit n'est pas trouvé dans la liste
                    Debug.WriteLine("ModifyProductPage: Produit non trouvé avec ProductUniqueId : " + uniqueId);
                    await DisplayAlert("Erreur", "Produit à modifier non trouvé.", "OK");
                    await Shell.Current.GoToAsync(".."); // Retourne à la page précédente
                }
            }
            else
            {
                // Gérer le cas où aucun ID n'est fourni
                Debug.WriteLine("ModifyProductPage: Aucun ProductUniqueId fourni pour la modification.");
                await DisplayAlert("Erreur", "Impossible de modifier. Aucun ID de produit fourni.", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        // --- Implémentation de INotifyPropertyChanged ---
        // Essentiel pour que les mises à jour des propriétés (comme ProductUniqueId, CurrentProduct)
        // soient reflétées dans l'interface utilisateur ou les bindings.
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // -------------------------------------------------------------
        // Vous devrez ajouter ici la logique pour la modification du produit,
        // la gestion des champs de saisie, et le bouton "Valider" / "Sauvegarder".
        // Par exemple :
        // private async void OnSaveButtonClicked(object sender, EventArgs e)
        // {
        //     if (CurrentProduct != null)
        //     {
        //         // Ici, vous prendriez les valeurs des champs de saisie (Entry, DatePicker, etc.)
        //         // et les assigneriez à CurrentProduct.product_name, CurrentProduct.product_dlc, etc.
        //         // Exemple : CurrentProduct.product_name = YourEntryName.Text;
        //
        //         // Ensuite, vous appelleriez une méthode de votre couche de données
        //         // pour sauvegarder ces modifications.
        //         // AppData.UpdateProduct(CurrentProduct); // Supposons que vous avez une telle méthode
        //
        //         await DisplayAlert("Succès", "Produit modifié avec succès !", "OK");
        //         await Shell.Current.GoToAsync(".."); // Retourne à la page précédente
        //     }
        // }
    }
}