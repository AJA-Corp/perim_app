using Microsoft.Maui.Controls;
using perimapp.Models;
using perimapp.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks; // Pour Task

namespace perimapp.Pages
{
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
                    OnPropertyChanged();
                    LoadProductForModification(_productUniqueId);
                }
            }
        }

        private ProductInfos? _currentProduct;
        public ProductInfos? CurrentProduct
        {
            get => _currentProduct;
            set
            {
                if (_currentProduct != value)
                {
                    _currentProduct = value;
                    OnPropertyChanged();
                    BindingContext = _currentProduct;
                }
            }
        }

        public ModifyProductPage()
        {
            InitializeComponent();
        }

        private async void LoadProductForModification(string? uniqueId)
        {
            if (!string.IsNullOrEmpty(uniqueId))
            {
                ProductInfos? product = AppData.CurrentProducts.FirstOrDefault(p => p.ProductUniqueId == uniqueId);

                if (product != null)
                {
                    CurrentProduct = product;
                    Debug.WriteLine($"ModifyProductPage: Produit à modifier chargé : {CurrentProduct.product_name}");

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
                    Debug.WriteLine("ModifyProductPage: Produit non trouvé avec ProductUniqueId : " + uniqueId);
                    await DisplayAlert("Erreur", "Produit à modifier non trouvé.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            else
            {
                Debug.WriteLine("ModifyProductPage: Aucun ProductUniqueId fourni pour la modification.");
                await DisplayAlert("Erreur", "Impossible de modifier. Aucun ID de produit fourni.", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        // ------------------------------------------------------------------------------------------------------------------
        // GESTIONNAIRES SPÉCIFIQUES AUX COMPORTEMENTS DEMANDÉS

        // Pour Product Name Entry: Pas de gestionnaire spécifique pour le texte qui ne doit pas disparaître.
        // Le binding Text="{Binding product_name}" est suffisant.

        /// <summary>
        /// Gère l'événement Completed pour ProductDlcEntry (quand l'utilisateur valide la saisie ou quitte le champ).
        /// Valide le format de la date.
        /// </summary>
        private async void ProductDlcEntry_Completed(object sender, EventArgs e)
        {
            if (CurrentProduct == null) return;

            Entry entry = (Entry)sender;
            string newDateText = entry.Text;

            if (DateTime.TryParseExact(newDateText, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                CurrentProduct.product_dlc = parsedDate;
            }
            else
            {
                await DisplayAlert("Erreur de format", "Veuillez entrer la date au format JJ/MM/AAAA.", "OK");
                entry.Text = CurrentProduct.product_dlc.ToString("dd/MM/yyyy");
            }
        }

        // Pour Product Quantity Entry: L'objectif est que le curseur soit à la fin et que la quantité soit validée.

        // GESTIONNAIRE DU FOCUS POUR LA QUANTITÉ (pour positionner le curseur)
        // **IMPORTANT : Ce gestionnaire est maintenant sur QuantityEntry, pas ProductQuantityEntry**
        private void QuantityEntry_Focused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                entry.CursorPosition = entry.Text?.Length ?? 0;
            }
        }

        // GESTIONNAIRE DE PERTE DE FOCUS POUR LA QUANTITÉ (pour la validation)
        // **IMPORTANT : Ce gestionnaire est maintenant sur QuantityEntry**
        private async void QuantityEntry_Unfocused(object sender, FocusEventArgs e)
        {
            if (CurrentProduct == null) return;

            Entry entry = (Entry)sender;
            string newQuantityText = entry.Text;

            if (int.TryParse(newQuantityText, out int quantity) && quantity >= 1)
            {
                CurrentProduct.product_quantity = quantity.ToString(); // Mettre à jour le modèle
            }
            else
            {
                // Si la saisie est invalide, avertir et restaurer la dernière valeur valide
                await DisplayAlert("Saisie invalide", "Veuillez entrer une quantité numérique valide (minimum 1).", "OK");
                // Restaurer l'ancienne valeur du modèle
                entry.Text = CurrentProduct.product_quantity ?? "1"; // Utiliser la valeur du modèle, ou "1" par défaut
            }
        }

        // Les boutons +/- interagissent directement avec CurrentProduct.product_quantity
        // (qui est bindé à QuantityEntry.Text)
        private void OnIncrementQuantityClicked(object sender, EventArgs e)
        {
            if (CurrentProduct != null && int.TryParse(CurrentProduct.product_quantity, out int quantity))
            {
                quantity++;
                CurrentProduct.product_quantity = quantity.ToString();
            }
            else if (CurrentProduct != null)
            {
                CurrentProduct.product_quantity = "1"; // Si non valide, initialiser à 1
            }
        }

        private void OnDecrementQuantityClicked(object sender, EventArgs e)
        {
            if (CurrentProduct != null && int.TryParse(CurrentProduct.product_quantity, out int quantity) && quantity > 1)
            {
                quantity--;
                CurrentProduct.product_quantity = quantity.ToString();
            }
            // Si la quantité est 1 ou moins, ou invalide, ne rien faire.
        }

        // --- Implémentation de INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            if (CurrentProduct != null)
            {
                if (string.IsNullOrWhiteSpace(CurrentProduct.product_name))
                {
                    await DisplayAlert("Erreur", "Le nom du produit ne peut pas être vide.", "OK");
                    return;
                }

                if (!int.TryParse(CurrentProduct.product_quantity, out int qty) || qty < 1)
                {
                    await DisplayAlert("Erreur", "La quantité doit être un nombre valide et supérieur ou égal à 1.", "OK");
                    return;
                }
                
                Debug.WriteLine($"Produit {CurrentProduct.product_name} ({CurrentProduct.ProductUniqueId}) sauvegardé avec : ");
                Debug.WriteLine($"  Quantité: {CurrentProduct.product_quantity}");
                Debug.WriteLine($"  DLC: {CurrentProduct.product_dlc:dd/MM/yyyy}");

                await DisplayAlert("Succès", "Produit modifié avec succès !", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Erreur", "Aucun produit à sauvegarder.", "OK");
            }
        }
    }
}