using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perimapp.Pages;

// Supposons que ce code se trouve dans votre fichier .xaml.cs pour la page/popup qui contient ce contrôle.

public partial class AddProductPage : ContentPage // ou Popup
{
    private int _currentQuantity = 1; // Initialisation à 1

    public AddProductPage()
    {
        InitializeComponent();
        QuantityEntry.Text = _currentQuantity.ToString(); // Assurez-vous que l'Entry affiche la valeur initiale

        // ABONNEMENT À L'ÉVÉNEMENT TEXTCHANGED
        QuantityEntry.TextChanged += QuantityEntry_TextChanged;
    }

    private void OnIncrementQuantityClicked(object sender, EventArgs e)
    {
        // Avant d'incrémenter, assurez-vous que la valeur de l'Entry est bien prise en compte
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
            // Assurez-vous que la quantité n'est pas inférieure à 1
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
}