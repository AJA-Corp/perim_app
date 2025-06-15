using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perimapp.Pages;

// Supposons que ce code se trouve dans votre fichier .xaml.cs pour la page/popup qui contient ce contrôle.

public partial class AddProductPage : ContentPage // ou Popup
{
    // Propriété pour stocker la quantité. C'est mieux d'utiliser un BindableProperty
    // ou une propriété avec INotifyPropertyChanged si vous utilisez un ViewModel.
    private int _currentQuantity = 1; // Initialisation à 1

    public AddProductPage()
    {
        InitializeComponent();
        QuantityEntry.Text = _currentQuantity.ToString(); // Assurez-vous que l'Entry affiche la valeur initiale
    }

    private void OnIncrementQuantityClicked(object sender, EventArgs e)
    {
        _currentQuantity++;
        QuantityEntry.Text = _currentQuantity.ToString();
    }

    private void OnDecrementQuantityClicked(object sender, EventArgs e)
    {
        if (_currentQuantity > 1) // Ne pas descendre en dessous de 1
        {
            _currentQuantity--;
            QuantityEntry.Text = _currentQuantity.ToString();
        }
    }

    // Gérer la saisie manuelle dans le champ Entry
    private void QuantityEntry_Unfocused(object sender, FocusEventArgs e)
    {
        if (int.TryParse(QuantityEntry.Text, out int quantity) && quantity >= 1)
        {
            _currentQuantity = quantity;
        }
        else
        {
            // Si la saisie n'est pas valide (non numérique ou < 1), réinitialiser à la dernière quantité valide
            QuantityEntry.Text = _currentQuantity.ToString();
        }
    }

    // Optionnel: Effacer le texte quand l'utilisateur clique sur l'Entry pour faciliter la saisie
    private void QuantityEntry_Focused(object sender, FocusEventArgs e)
    {
        if (QuantityEntry.Text == _currentQuantity.ToString()) // Seulement si c'est la valeur actuelle
        {
            QuantityEntry.Text = string.Empty;
        }
    }
}