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

        // Nouveau : Champ privé pour stocker la quantité numérique
        private int _currentQuantity;

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

                    if (_currentProduct != null)
                    {
                        // Initialiser le texte de la DLC Entry lors du chargement
                        ProductDlcEntry.Text = _currentProduct.product_dlc.ToString("dd/MM/yyyy");

                        // Initialiser _currentQuantity à partir du produit chargé
                        if (int.TryParse(_currentProduct.product_quantity, out int parsedQuantity))
                        {
                            _currentQuantity = Math.Max(1, parsedQuantity); // Assurez-vous que la quantité est au moins 1
                        }
                        else
                        {
                            _currentQuantity = 1; // Valeur par défaut si la conversion échoue
                        }

                        // Mettre à jour l'Entry de la quantité pour refléter la valeur initiale (ou corrigée)
                        // Cela mettra également à jour CurrentProduct.product_quantity via le binding
                        if (QuantityEntry != null) // Vérification pour s'assurer que l'Entry est initialisée
                        {
                            QuantityEntry.Text = _currentQuantity.ToString();
                        }
                    }
                }
            }
        }

        public ModifyProductPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
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
                        $"ModifyProductPage: Produit à modifier chargé : {CurrentProduct.product_name}"
                    );

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
                                    Debug.WriteLine(
                                        $"ModifyProductPage: L'URL de l'image est accessible ! Statut: {response.StatusCode}"
                                    );
                                }
                                else
                                {
                                    Debug.WriteLine(
                                        $"ModifyProductPage: L'URL de l'image N'EST PAS accessible. Statut: {response.StatusCode}"
                                    );
                                }
                            }
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
                else
                {
                    Debug.WriteLine(
                        "ModifyProductPage: Produit non trouvé avec ProductUniqueId : " + uniqueId
                    );
                    await DisplayAlert("Erreur", "Produit à modifier non trouvé.", "OK");
                    await Shell.Current.GoToAsync("..");
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

        // ------------------------------------------------------------------------------------------------------------------
        // NOUVELLE MÉTHODE POUR SYNCHRONISER _currentQuantity AVEC LE CONTENU DE L'ENTRY
        private void UpdateCurrentQuantityFromEntry()
        {
            if (QuantityEntry == null)
                return; // Sécurité

            // Tente de parser le texte de l'Entry dans _currentQuantity
            if (int.TryParse(QuantityEntry.Text, out int parsedValue))
            {
                _currentQuantity = Math.Max(1, parsedValue); // S'assure que la valeur est au moins 1
            }
            else
            {
                // Si la saisie n'est pas un nombre valide, on force _currentQuantity à 1
                // et on met à jour l'Entry pour refléter cette correction.
                _currentQuantity = 1;
                QuantityEntry.Text = _currentQuantity.ToString();
                Debug.WriteLine(
                    "UpdateCurrentQuantityFromEntry: Saisie invalide détectée, quantité réinitialisée à 1."
                );
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
                CurrentProduct.product_dlc = parsedDate; // Met à jour le modèle
            }
            else
            {
                await DisplayAlert(
                    "Erreur de format",
                    "Veuillez entrer la date au format JJ/MM/AAAA. (Ex: 01/01/2025)",
                    "OK"
                );
                entry.Text = CurrentProduct.product_dlc.ToString("dd/MM/yyyy");
            }
        }

        /// <summary>
        /// Gère l'événement Focused pour QuantityEntry pour positionner le curseur à la fin.
        /// </summary>
        private void QuantityEntry_Focused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                entry.CursorPosition = entry.Text?.Length ?? 0;
            }
        }

        /// <summary>
        /// Gère l'événement Unfocused pour QuantityEntry (pour la validation).
        /// </summary>
        private async void QuantityEntry_Unfocused(object sender, FocusEventArgs e)
        {
            Debug.WriteLine("QuantityEntry_Unfocused called.");
            if (CurrentProduct == null)
                return;

            Entry entry = (Entry)sender;
            string newQuantityText = entry.Text;

            if (int.TryParse(newQuantityText, out int quantity) && quantity >= 1)
            {
                _currentQuantity = quantity; // Mettre à jour _currentQuantity basé sur le texte de l'Entry
                CurrentProduct.product_quantity = _currentQuantity.ToString(); // Assurer que le modèle est en sync
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
                // Restaurer la dernière valeur valide connue de _currentQuantity
                entry.Text = _currentQuantity.ToString();
                CurrentProduct.product_quantity = _currentQuantity.ToString(); // Assurer que le modèle est également restauré
                Debug.WriteLine(
                    $"QuantityEntry_Unfocused: Quantité invalide. Restaurée à {_currentQuantity}"
                );
            }
        }

        // --- GESTIONNAIRES DES BOUTONS D'INC/DEC DE QUANTITÉ (basés sur votre exemple AddProductPage) ---
        private void OnIncrementQuantityClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("OnIncrementQuantityClicked called.");
            if (CurrentProduct == null)
                return;

            // Assurez-vous que _currentQuantity est synchronisé avec le texte actuel de l'Entry
            UpdateCurrentQuantityFromEntry();

            // AJOUTÉ : Vérifie si la quantité est déjà à 99
            if (_currentQuantity >= 99)
            {
                Debug.WriteLine(
                    "OnIncrementQuantityClicked: Quantity is already 99. Not incrementing further."
                );
                return; // Ne fait rien si la quantité est déjà à 99
            }

            _currentQuantity++;
            QuantityEntry.Text = _currentQuantity.ToString(); // Met à jour l'UI et le modèle via binding
            Debug.WriteLine($"Incremented quantity to: {_currentQuantity}");
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("OnDecrementQuantityClicked called.");
            if (CurrentProduct == null)
                return;

            // Assurez-vous que _currentQuantity est synchronisé avec le texte actuel de l'Entry
            UpdateCurrentQuantityFromEntry();

            if (_currentQuantity > 1) // Ne pas descendre en dessous de 1
            {
                _currentQuantity--;
                QuantityEntry.Text = _currentQuantity.ToString(); // Met à jour l'UI et le modèle via binding
                Debug.WriteLine($"Decremented quantity to: {_currentQuantity}");
            }
            else
            {
                Debug.WriteLine(
                    "OnDecrementQuantityClicked: Quantity is already 1. Not decrementing."
                );
            }
        }

            // Appel au service
            var service = new NeonProductService();
            bool success = await service.UpdateUserProductAsync(CurrentProduct);

            if (success)
            {
                // Effectuer une dernière synchronisation et validation avant la sauvegarde finale
                // UpdateCurrentQuantityFromEntry(); // Pas strictement nécessaire si QuantityEntry_Unfocused est toujours appelé avant la sauvegarde
                // mais peut être une bonne sécurité si l'utilisateur ne quitte pas le champ.
                // Ou mieux, on utilise la logique de validation de QuantityEntry_Unfocused
                // Pour s'assurer que CurrentProduct.product_quantity est à jour

                // On s'assure que le champ de quantité est bien validé et synchronisé
                // Ceci re-déclenchera la validation et la synchro si le champ est toujours focus.
                if (QuantityEntry.IsFocused)
                {
                    // Forcer la perte de focus pour déclencher Unfocused (si nécessaire)
                    // Ou appeler directement la logique de validation
                    QuantityEntry_Unfocused(QuantityEntry, null);
                }

                if (string.IsNullOrWhiteSpace(CurrentProduct.product_name))
                {
                    AppData.CurrentProducts[index] = CurrentProduct;
                }

                // La validation de la quantité est déjà gérée dans QuantityEntry_Unfocused et UpdateCurrentQuantityFromEntry
                // On peut juste vérifier la valeur finale dans le modèle
                if (!int.TryParse(CurrentProduct.product_quantity, out int qty) || qty < 1)
                {
                    await DisplayAlert(
                        "Erreur",
                        "La quantité doit être un nombre valide et supérieur ou égal à 1.",
                        "OK"
                    );
                    return;
                }

                Debug.WriteLine(
                    $"Produit {CurrentProduct.product_name} ({CurrentProduct.ProductUniqueId}) sauvegardé avec : "
                );
                Debug.WriteLine($"  Quantité: {CurrentProduct.product_quantity}");
                Debug.WriteLine($"  DLC: {CurrentProduct.product_dlc:dd/MM/yyyy}");

                await DisplayAlert("Succès", "Produit modifié avec succès !", "OK");
                await Shell.Current.GoToAsync("..");
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
