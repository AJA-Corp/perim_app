using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using perimapp.Data;
using perimapp.Services;
using perimapp.Models;

namespace perimapp.Pages;

public partial class AddProductPage : ContentPage // ou Popup
{
    private int _currentQuantity = 1; // Initialisation à 1

    public AddProductPage()
    {
        InitializeComponent();
        QuantityEntry.Text = _currentQuantity.ToString(); // Assurez-vous que l'Entry affiche la valeur initiale

        // ABONNEMENT À L'ÉVÉNEMENT TEXTCHANGED
        QuantityEntry.TextChanged += QuantityEntry_TextChanged;

        // Ajuste la largeur du sélecteur de date au démarrage et quand ça change
        SizeChanged += (_, __) => AdjustDatePickerWidth();
        DlcPicker.DateSelected += (_, __) => AdjustDatePickerWidth();
        // Appel initial
        AdjustDatePickerWidth();
    }

    private void AdjustDatePickerWidth()
    {
        if (Width <= 0)
            return;

        string format = string.IsNullOrWhiteSpace(DlcPicker.Format)
            ? "dd/MM/yyyy"
            : DlcPicker.Format;
        string sample = ((DateTime)DlcPicker.Date).ToString(format, CultureInfo.CurrentCulture);

        double fontSize = DlcPicker.FontSize > 0 ? DlcPicker.FontSize : 18;
        var probe = new Label
        {
            Text = sample,
            FontSize = fontSize,
            FontFamily = DlcPicker.FontFamily,
        };

        double measured = probe.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;

        double target = measured + 24; // padding interne
        double max = Math.Min(Width * 0.6, 260);
        double min = 140;
        target = Math.Max(min, Math.Min(max, target));

        DlcPicker.WidthRequest = target;
        DlcBorder.WidthRequest = target + 16;
    }

    private void OnIncrementQuantityClicked(object sender, EventArgs e)
    {
        // Avant d'incrémenter, regarder si la valeur de l'Entry est bien prise en compte
        UpdateCurrentQuantityFromEntry();

        _currentQuantity++;
        QuantityEntry.Text = _currentQuantity.ToString();
    }

    private void OnDecrementQuantityClicked(object sender, EventArgs e)
    {
        // Avant de décrémenter, assurez-vous que la valeur de l'Entry est bien prise en compte
        UpdateCurrentQuantityFromEntry();

        if (_currentQuantity > 1) // Ne pas descendre en dessous de 1
        {
            _currentQuantity--;
            QuantityEntry.Text = _currentQuantity.ToString();
        }
    }

    // NOUVELLE MÉTHODE POUR METTRE À JOUR _currentQuantity À PARTIR DE L'ENTRY
    private void UpdateCurrentQuantityFromEntry()
    {
        // Tente de parser le texte actuel de l'Entry
        if (int.TryParse(QuantityEntry.Text, out int parsedQuantity))
        {
            // Assurez que la quantité n'est pas inférieure à 1
            _currentQuantity = Math.Max(1, parsedQuantity);
        }
        else
        {
            // Si la saisie n'est pas un nombre valide, réinitialiser à 1 ou à la dernière quantité valide connue
            // Pour l'exemple, on réinitialise à 1 ou à l'ancienne _currentQuantity
            _currentQuantity = Math.Max(1, _currentQuantity); // Garde la valeur actuelle si invalide, mais assure minimum 1
            QuantityEntry.Text = _currentQuantity.ToString(); // Met à jour l'Entry pour afficher la valeur corrigée
        }
    }

    // Événement TextChanged : Appelé chaque fois que le texte de l'Entry change
    private void QuantityEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        // On ne met pas à jour _currentQuantity ici directement, car cela pourrait être lent ou créer des boucles.
        // On se contente de s'assurer que la validation et la mise à jour se feront lors du unfocus ou du clic bouton.
        // La méthode UpdateCurrentQuantityFromEntry() est appelée explicitement avant les opérations sur les boutons.
    }

    // Gérer la saisie manuelle dans le champ Entry (cet événement reste important pour la validation finale)
    private void QuantityEntry_Unfocused(object sender, FocusEventArgs e)
    {
        UpdateCurrentQuantityFromEntry(); // Assure que la validation finale est faite quand l'Entry perd le focus
        QuantityEntry.Text = _currentQuantity.ToString(); // Met à jour l'Entry avec la valeur validée
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        string userIdString = await SecureStorage.GetAsync("user_id");

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            await DisplayAlertAsync("Erreur", "Utilisateur non identifié. Veuillez vous reconnecter.", "OK");
            await Shell.Current.GoToAsync(nameof(StartingPage));
            return;
        }

        if (!long.TryParse(BarcodeEntry.Text, out long barcode))
        {
            await DisplayAlertAsync("Erreur", "Code-barres invalide.", "OK");
            return;
        }

        var service = new NeonProductService();
        var product = await service.GetProductDataAsync(barcode);

        if (product == null)
        {
            var apiService = new OpenFoodFactsService();
            var apiProduct = await apiService.GetProductFromApiAsync(barcode);

            if (apiProduct == null)
            {
                await DisplayAlertAsync("Erreur", "Produit introuvable dans la base et API.", "OK");
                return;
            }

            await service.AddProductDataAsync(apiProduct);
            product = apiProduct;
        }

        product.Dlc = DlcPicker.Date ?? DateTime.Now;
        product.Quantity = _currentQuantity;
        product.AddedAt = DateTime.Now;

        bool ok = await service.AddUserProductAsync(product, userId);

        // Sauvegarde locale pour le hors-ligne
        var localService = new LocalProductService();
        await localService.AddProductAsync(product);

        AppData.CurrentProducts.Add(product);

        if (ok)
        {
            await DisplayAlertAsync("Succès", "Produit ajouté avec succès.", "OK");
            await Shell.Current.GoToAsync(nameof(MainPage));
        }
        else
        {
            await DisplayAlertAsync("Erreur", "Impossible d'ajouter le produit.", "OK");
        }
    }

    private async void BarcodeEntry_OnCompleted(object sender, EventArgs e)
    {
        if (!long.TryParse(BarcodeEntry.Text, out long barcode))
        {
            await DisplayAlertAsync("Erreur", "Code-barres invalide.", "OK");
            return;
        }

        // Get user's home code for custom name lookup
        string userIdString = await SecureStorage.GetAsync("user_id");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            await DisplayAlertAsync("Erreur", "Utilisateur non identifié. Veuillez vous reconnecter.", "OK");
            await Shell.Current.GoToAsync(nameof(StartingPage));
            return;
        }

        var userService = new NeonUserService();
        var user = await userService.GetUserProfileAsync(userId);
        
        var service = new NeonProductService();
        ProductInfos? product = null;

        // Try to get product with custom name if user has home code
        if (user?.HomeCode != null)
        {
            product = await service.GetProductDataWithCustomNameAsync(barcode, user.HomeCode);
        }
        
        // Fallback to regular product data if no custom name version found
        if (product == null)
        {
            product = await service.GetProductDataAsync(barcode);
        }

        if (product == null)
        {
            var apiService = new OpenFoodFactsService();
            product = await apiService.GetProductFromApiAsync(barcode);

            if (product == null)
            {
                bool reponse = await DisplayAlertAsync(
                    "Erreur",
                    "Produit introuvable. Voulez-vous ajouter un nouveau produit perso. ?",
                    "Oui",
                    "Non"
                );

                if (reponse)
                {
                    string result = await DisplayPromptAsync(
                        "Nom du produit",
                        "Entrez le nom du produit",
                        "OK",
                        "Annuler",
                        "Entrez ici",
                        maxLength: 255,
                        keyboard: Keyboard.Text
                    );

                    if (!string.IsNullOrEmpty(result))
                    {
                        BarcodeEntry.Text = string.Empty;
                        ProductName.Text = result;
                        ProductImage.Source = null;
                    }
                }
                return;
            }
        }

        // Display the appropriate name (custom name if available, otherwise original name)
        ProductName.Text = product.DisplayName;
        ProductImage.Source = product.UrlImage;
    }
}
