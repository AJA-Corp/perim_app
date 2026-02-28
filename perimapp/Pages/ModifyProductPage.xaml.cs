using System; // Pour TimeSpan, DateTime, Math.Max
using System.ComponentModel;
using System.Diagnostics; // Pour Debug.WriteLine
using System.Linq;
using System.Net.Http; // Pour HttpClient
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using perimapp.Data; // Pour AppData
using perimapp.Models;
using perimapp.Services;

namespace perimapp.Pages
{
    [QueryProperty(nameof(ProductUniqueId), "ProductUniqueId")]
    public partial class ModifyProductPage : ContentPage, INotifyPropertyChanged
    {
        private readonly NeonProductService _productService;
        private readonly LocalProductService _localProductService;
        private string _currentCustomName = string.Empty;

        // Propriété bindable pour récupérer l'ID unique du produit passé en paramètre
        private string? _productUniqueId;
        public string? ProductUniqueId
        {
            get => _productUniqueId;
            set
            {
                if (_productUniqueId != value)
                {
                    _productUniqueId = value;
                    OnPropertyChanged(); // Notifie le changement pour les bindings (ex: header)
                    LoadProductForModification(_productUniqueId); // Charge les détails du produit
                }
            }
        }

        private ProductInfos? _currentProduct;

        // Propriété bindable qui représente le produit en cours de modification
        public ProductInfos? CurrentProduct
        {
            get => _currentProduct;
            set
            {
                if (_currentProduct != value)
                {
                    _currentProduct = value;
                    OnPropertyChanged(); // Notifie les changements pour l'UI

                    // Initialiser _currentQuantity et l'affichage de l'Entry lors du chargement du produit
                    if (_currentProduct != null)
                    {
                        // Initialiser la quantité numérique à partir du modèle (qui est un int)
                        _currentQuantity = Math.Max(1, _currentProduct.Quantity); // UTILISE .Quantity
                        
                        // Initialiser le nom personnalisé actuel
                        _currentCustomName = _currentProduct.DisplayName;

                        // Mettre à jour le texte de l'Entry de quantité
                        if (QuantityEntry != null)
                        {
                            QuantityEntry.Text = _currentQuantity.ToString();
                            Debug.WriteLine(
                                $"ModifyProductPage: Initial quantity set to {_currentQuantity}"
                            );
                        }
                        
                        // Mettre à jour l'Entry du nom avec le nom d'affichage
                        if (ProductNameEntry != null)
                        {
                            ProductNameEntry.Text = _currentCustomName;
                            Debug.WriteLine(
                                $"ModifyProductPage: Initial product name set to {_currentCustomName}"
                            );
                        }
                
                        // Le DatePicker se met à jour automatiquement grâce à la liaison de données.
                        // Il n'y a donc plus besoin de faire référence à ProductDlcEntry.
                    }
                }
            }
        }

        // Champ privé pour stocker la quantité numérique, synchronisé avec CurrentProduct.Quantity
        private int _currentQuantity;

        public ModifyProductPage()
        {
            InitializeComponent();
            _productService = App.Services.GetService<NeonProductService>();
            _localProductService = new LocalProductService();
            BindingContext = this;
        }

        // Méthode pour charger les détails du produit en fonction de l'ID unique
        private async void LoadProductForModification(string? uniqueId)
        {
            if (!string.IsNullOrEmpty(uniqueId))
            {
                ProductInfos? product = AppData.CurrentProducts.FirstOrDefault(p =>
                    p.ProductUniqueId == uniqueId
                );

                if (product != null)
                {
                    CurrentProduct = product;
                    Debug.WriteLine(
                        $"ModifyProductPage: Produit à modifier chargé : {CurrentProduct.Name}"
                    );

                    CheckImageUrlAsync(CurrentProduct.UrlImage);
                }
                else
                {
                    Debug.WriteLine(
                        "ModifyProductPage: Produit non trouvé avec ProductUniqueId : " + uniqueId
                    );
                    await DisplayAlert("Erreur", "Produit à modifier non trouvé.", "OK");
                    //await Shell.Current.GoToAsync("..");
                    await Shell.Current.GoToAsync(nameof(MainPage));
                }
            }
            else
            {
                Debug.WriteLine(
                    "ModifyProductPage: Aucun ProductUniqueId fourni pour la modification."
                );
                await DisplayAlert(
                    "Erreur",
                    "Impossible de modifier. Aucun ID de produit fourni.",
                    "OK"
                );
                await Shell.Current.GoToAsync("..");
            }
        }

        // Vérifie si l'URL de l'image est accessible
        private async void CheckImageUrlAsync(string? url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                    var response = await client.GetAsync(url);
                    Debug.WriteLine(
                        response.IsSuccessStatusCode
                            ? $"ModifyProductPage: L'URL de l'image est accessible. Statut: {response.StatusCode}"
                            : $"ModifyProductPage: L'URL de l'image n'est pas accessible. Statut: {response.StatusCode}"
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"ModifyProductPage: Erreur lors de la vérification de l'URL de l'image : {ex.Message}"
                    );
                }
            }
            else
            {
                Debug.WriteLine("ModifyProductPage: url_image est nulle ou vide.");
            }
        }

        // ------------------------------------------------------------------------------------------------------------------
        // LOGIQUE SPÉCIFIQUE AUX CHAMPS DE MODIFICATION

        // Synchronise la valeur de _currentQuantity avec le texte de l'Entry
        private void UpdateCurrentQuantityFromEntry()
        {
            if (QuantityEntry == null)
                return;

            if (int.TryParse(QuantityEntry.Text, out int parsedValue))
            {
                _currentQuantity = Math.Max(1, parsedValue);
            }
            else
            {
                _currentQuantity = 1;
                QuantityEntry.Text = _currentQuantity.ToString();
                Debug.WriteLine(
                    "UpdateCurrentQuantityFromEntry: Saisie invalide détectée, quantité réinitialisée à 1."
                );
            }
        }

        /*
        // Gère la validation de la DLC lors de la complétion de la saisie
        private async void ProductDlcEntry_Completed(object sender, EventArgs e)
        {
            if (CurrentProduct == null)
                return;

            Entry entry = (Entry)sender;
            string newDateText = entry.Text;

            if (
                DateTime.TryParseExact(
                    newDateText,
                    "dd/MM/yyyy",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsedDate
                )
            )
            {
                CurrentProduct.Dlc = parsedDate; // UTILISE .Dlc
                Debug.WriteLine(
                    $"ProductDlcEntry_Completed: DLC mise à jour à {parsedDate:dd/MM/yyyy}"
                );
            }
            else
            {
                await DisplayAlert(
                    "Erreur de format",
                    "Veuillez entrer la date au format JJ/MM/AAAA. (Ex: 01/01/2025)",
                    "OK"
                );
                entry.Text = CurrentProduct.Dlc.ToString("dd/MM/yyyy"); // UTILISE .Dlc
                Debug.WriteLine(
                    $"ProductDlcEntry_Completed: Format de date invalide. Restauré à {CurrentProduct.Dlc:dd/MM/yyyy}"
                );
            }
        }
        */

        // Place le curseur à la fin du texte lorsque l'Entry de quantité est focus
        private void QuantityEntry_Focused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                entry.CursorPosition = entry.Text?.Length ?? 0;
            }
        }

        // Gère les changements du nom du produit
        private void ProductNameEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                _currentCustomName = entry.Text ?? string.Empty;
            }
        }

        // Gère la validation de la quantité lorsque l'Entry perd le focus
        private async void QuantityEntry_Unfocused(object sender, FocusEventArgs e)
        {
            Debug.WriteLine("QuantityEntry_Unfocused called.");
            if (CurrentProduct == null)
                return;

            Entry entry = (Entry)sender;
            string newQuantityText = entry.Text;

            if (int.TryParse(newQuantityText, out int quantity) && quantity >= 1)
            {
                _currentQuantity = quantity;
                CurrentProduct.Quantity = _currentQuantity; // UTILISE .Quantity
                Debug.WriteLine(
                    $"QuantityEntry_Unfocused: Quantité valide définie à {_currentQuantity}"
                );
            }
            else
            {
                await DisplayAlert(
                    "Saisie invalide",
                    "Veuillez entrer une quantité numérique valide (minimum 1).",
                    "OK"
                );
                entry.Text = _currentQuantity.ToString();
                CurrentProduct.Quantity = _currentQuantity; // UTILISE .Quantity
                Debug.WriteLine(
                    $"QuantityEntry_Unfocused: Quantité invalide. Restaurée à {_currentQuantity}"
                );
            }
        }

        // Incrémente la quantité, avec une limite à 99
        private void OnIncrementQuantityClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("OnIncrementQuantityClicked called.");
            if (CurrentProduct == null)
                return;

            UpdateCurrentQuantityFromEntry();

            if (_currentQuantity >= 99)
            {
                Debug.WriteLine(
                    "OnIncrementQuantityClicked: Quantity is already 99. Not incrementing further."
                );
                return;
            }

            _currentQuantity++;
            QuantityEntry.Text = _currentQuantity.ToString();
            CurrentProduct.Quantity = _currentQuantity; // UTILISE .Quantity
            Debug.WriteLine($"Incremented quantity to: {_currentQuantity}");
        }

        // Décrémente la quantité, avec une limite à 1
        private void OnDecrementQuantityClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("OnDecrementQuantityClicked called.");
            if (CurrentProduct == null)
                return;

            UpdateCurrentQuantityFromEntry();

            if (_currentQuantity > 1)
            {
                _currentQuantity--;
                QuantityEntry.Text = _currentQuantity.ToString();
                CurrentProduct.Quantity = _currentQuantity; // UTILISE .Quantity
                Debug.WriteLine($"Decremented quantity to: {_currentQuantity}");
            }
            else
            {
                Debug.WriteLine(
                    "OnDecrementQuantityClicked: Quantity is already 1. Not decrementing."
                );
            }
        }

        // Gère le clic sur le bouton "Enregistrer"
        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            if (CurrentProduct != null)
            {
                if (DlcPicker != null)
                {
                    CurrentProduct.Dlc = DlcPicker.Date ?? DateTime.Now;
                    Debug.WriteLine($"[DEBUG] Synced DLC from DatePicker: {CurrentProduct.Dlc:dd/MM/yyyy}");
                }
                
                if (QuantityEntry.IsFocused)
                {
                    QuantityEntry_Unfocused(QuantityEntry, null);
                }

                // Validate product name
                if (string.IsNullOrWhiteSpace(_currentCustomName))
                {
                    await DisplayAlert("Erreur", "Le nom du produit ne peut pas être vide.", "OK");
                    return;
                }

                // Vérifie la quantité directement sur le modèle
                if (CurrentProduct.Quantity < 1) // UTILISE .Quantity
                {
                    await DisplayAlert(
                        "Erreur",
                        "La quantité doit être supérieure ou égale à 1.",
                        "OK"
                    );
                    return;
                }

                // Save custom name if it's different from original name and user has home code
                if (CurrentProduct.HomeCode.HasValue && 
                    !string.IsNullOrWhiteSpace(_currentCustomName) && 
                    _currentCustomName.Trim() != CurrentProduct.Name.Trim())
                {
                    try
                    {
                        // Save custom name to remote database
                        bool customNameSaved = await _productService.SetCustomProductNameAsync(
                            CurrentProduct.Barcode, 
                            CurrentProduct.HomeCode.Value, 
                            _currentCustomName.Trim());

                        // Save custom name locally for offline access
                        await _localProductService.SaveCustomProductNameAsync(
                            CurrentProduct.Barcode, 
                            CurrentProduct.HomeCode.Value, 
                            _currentCustomName.Trim());

                        if (customNameSaved)
                        {
                            CurrentProduct.CustomName = _currentCustomName.Trim();
                            Debug.WriteLine($"[DEBUG] Custom name saved: {CurrentProduct.CustomName}");
                        }
                        else
                        {
                            Debug.WriteLine("[DEBUG] Failed to save custom name to remote database");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DEBUG] Error saving custom name: {ex.Message}");
                        // Continue with product update even if custom name save fails
                    }
                }

                bool updated = await _productService.UpdateUserProductAsync(CurrentProduct);
                if (!updated)
                {
                    await DisplayAlert(
                        "Erreur",
                        "Impossible de sauvegarder le produit en base.",
                        "OK"
                    );
                    return;
                }

                await DisplayAlert("Succès", "Produit modifié avec succès !", "OK");

                Debug.WriteLine(
                    $"Produit {CurrentProduct.DisplayName} ({CurrentProduct.ProductUniqueId}) sauvegardé avec : "
                );
                Debug.WriteLine($"  Quantité: {CurrentProduct.Quantity}"); // UTILISE .Quantity
                Debug.WriteLine($"  DLC: {CurrentProduct.Dlc:dd/MM/yyyy}"); // UTILISE .Dlc
                Debug.WriteLine($"  Catégorie: {CurrentProduct.Category}");
                Debug.WriteLine($"  URL Image: {CurrentProduct.UrlImage}");
                Debug.WriteLine($"  Nom personnalisé: {CurrentProduct.CustomName}");

                //await Shell.Current.GoToAsync("..");
                try
                {
                    await Shell.Current.GoToAsync(
                        $"{nameof(DetailsPage)}?ProductUniqueId={CurrentProduct.ProductUniqueId}"
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] - Erreur navigation : {ex.Message}");
                    await Shell.Current.GoToAsync(nameof(MainPage));
                }
            }
            else
            {
                await DisplayAlert("Erreur", "Aucun produit à sauvegarder.", "OK");
            }
        }

        // --- Implémentation de INotifyPropertyChanged ---
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
