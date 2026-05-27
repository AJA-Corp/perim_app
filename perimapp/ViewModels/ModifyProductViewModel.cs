using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using perimapp.Views;

namespace perimapp.ViewModels
{
    public partial class ModifyProductViewModel : ObservableObject
    {
        private readonly LocalProductService _localProductService;
        private readonly ContentPage _page;

        [ObservableProperty]
        private string? _productUniqueId;

        [ObservableProperty]
        private ProductInfos? _currentProduct;

        [ObservableProperty]
        private int _currentQuantity;

        [ObservableProperty]
        private string _currentCustomName;

        public ModifyProductViewModel(ContentPage page, LocalProductService localProductService)
        {
            _page = page;
            _localProductService = localProductService;
        }

        partial void OnProductUniqueIdChanged(string? value)
        {
            LoadProductForModification(value);
        }

        partial void OnCurrentProductChanged(ProductInfos? value)
        {
            if (value != null)
            {
                CurrentQuantity = Math.Max(1, value.Quantity);
                CurrentCustomName = value.DisplayName;
            }
        }

        private async void LoadProductForModification(string? uniqueId)
        {
            if (!string.IsNullOrEmpty(uniqueId))
            {
                ProductInfos? product = AppData.CurrentProducts.FirstOrDefault(p => p.ProductUniqueId == uniqueId);

                if (product != null)
                {
                    CurrentProduct = product;
                    Debug.WriteLine($"ModifyProductView: Produit à modifier chargé : {CurrentProduct.Name}");
                }
                else
                {
                    Debug.WriteLine("ModifyProductView: Produit non trouvé avec ProductUniqueId : " + uniqueId);
                    await _page.DisplayAlert("Erreur", "Produit à modifier non trouvé.", "OK");
                    await Shell.Current.GoToAsync($"///{nameof(MainView)}");
                }
            }
            else
            {
                Debug.WriteLine("ModifyProductView: Aucun ProductUniqueId fourni pour la modification.");
                await _page.DisplayAlert("Erreur", "Impossible de modifier. Aucun ID de produit fourni.", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private void IncrementQuantity()
        {
            if (CurrentQuantity >= 99) return;
            CurrentQuantity++;
        }

        [RelayCommand]
        private void DecrementQuantity()
        {
            if (CurrentQuantity > 1)
            {
                CurrentQuantity--;
            }
        }

        [RelayCommand]
        private async Task SaveProductAsync()
        {
            if (CurrentProduct != null)
            {
                if (string.IsNullOrWhiteSpace(CurrentCustomName))
                {
                    await _page.DisplayAlert("Erreur", "Le nom du produit ne peut pas être vide.", "OK");
                    return;
                }

                if (CurrentQuantity < 1)
                {
                    await _page.DisplayAlert("Erreur", "La quantité doit être supérieure ou égale à 1.", "OK");
                    return;
                }

                CurrentProduct.Quantity = CurrentQuantity;

                if (CurrentCustomName.Trim() != CurrentProduct.Name?.Trim())
                {
                    CurrentProduct.CustomName = CurrentCustomName.Trim();

                    if (!string.IsNullOrWhiteSpace(CurrentProduct.HomeCode))
                    {
                        await _localProductService.SaveCustomProductNameAsync(
                            CurrentProduct.Barcode,
                            CurrentProduct.HomeCode,
                            CurrentProduct.CustomName);
                    }
                }

                await _localProductService.UpdateProductLocalAsync(CurrentProduct);

                perimapp.Services.NotificationScheduler.UpdateSchedules();

                await _page.DisplayAlert("Succès", "Produit modifié avec succès !", "OK");

                try
                {
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] - Erreur navigation : {ex.Message}");
                    await Shell.Current.GoToAsync($"///{nameof(MainView)}");
                }
            }
        }
    }
}