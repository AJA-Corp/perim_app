using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;

namespace perimapp.ViewModels
{
    public partial class ModifyProductViewModel : ObservableObject
    {
        private readonly NeonProductService _productService;
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

        public ModifyProductViewModel(
            ContentPage page,
            NeonProductService productService,
            LocalProductService localProductService)
        {
            _page = page;
            _productService = productService;
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
                ProductInfos? product = AppData.CurrentProducts.FirstOrDefault(p =>
                    p.ProductUniqueId == uniqueId
                );

                if (product != null)
                {
                    CurrentProduct = product;
                    Debug.WriteLine($"ModifyProductView: Produit \u00e0 modifier charg\u00e9 : {CurrentProduct.Name}");
                }
                else
                {
                    Debug.WriteLine("ModifyProductView: Produit non trouv\u00e9 avec ProductUniqueId : " + uniqueId);
                    await _page.DisplayAlert("Erreur", "Produit \u00e0 modifier non trouv\u00e9.", "OK");
                    await Shell.Current.GoToAsync(nameof(perimapp.Views.MainView));
                }
            }
            else
            {
                Debug.WriteLine("ModifyProductView: Aucun ProductUniqueId fourni pour la modification.");
                await _page.DisplayAlert(
                    "Erreur",
                    "Impossible de modifier. Aucun ID de produit fourni.",
                    "OK"
                );
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private void IncrementQuantity()
        {
            if (CurrentQuantity >= 99) return;
            CurrentQuantity++;
            if (CurrentProduct != null)
            {
                CurrentProduct.Quantity = CurrentQuantity;
            }
        }

        [RelayCommand]
        private void DecrementQuantity()
        {
            if (CurrentQuantity > 1)
            {
                CurrentQuantity--;
                if (CurrentProduct != null)
                {
                    CurrentProduct.Quantity = CurrentQuantity;
                }
            }
        }

        [RelayCommand]
        private async Task SaveProductAsync()
        {
            if (CurrentProduct != null)
            {
                if (string.IsNullOrWhiteSpace(CurrentCustomName))
                {
                    await _page.DisplayAlert("Erreur", "Le nom du produit ne peut pas \u00eatre vide.", "OK");
                    return;
                }

                if (CurrentQuantity < 1)
                {
                    await _page.DisplayAlert(
                        "Erreur",
                        "La quantit\u00e9 doit \u00eatre sup\u00e9rieure ou \u00e9gale \u00e0 1.",
                        "OK"
                    );
                    return;
                }
                
                CurrentProduct.Quantity = CurrentQuantity;

                if (CurrentProduct.HomeCode.HasValue && 
                    !string.IsNullOrWhiteSpace(CurrentCustomName) && 
                    CurrentCustomName.Trim() != CurrentProduct.Name.Trim())
                {
                    try
                    {
                        bool customNameSaved = await _productService.SetCustomProductNameAsync(
                            CurrentProduct.Barcode, 
                            CurrentProduct.HomeCode.Value, 
                            CurrentCustomName.Trim());

                        await _localProductService.SaveCustomProductNameAsync(
                            CurrentProduct.Barcode, 
                            CurrentProduct.HomeCode.Value, 
                            CurrentCustomName.Trim());

                        if (customNameSaved)
                        {
                            CurrentProduct.CustomName = CurrentCustomName.Trim();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DEBUG] Error saving custom name: {ex.Message}");
                    }
                }

                bool updated = await _productService.UpdateUserProductAsync(CurrentProduct);
                if (!updated)
                {
                    await _page.DisplayAlert(
                        "Erreur",
                        "Impossible de sauvegarder le produit en base.",
                        "OK"
                    );
                    return;
                }

                await _page.DisplayAlert("Succ\u00e8s", "Produit modifi\u00e9 avec succ\u00e8s !", "OK");

                try
                {
                    await Shell.Current.GoToAsync(
                        $"{nameof(perimapp.Views.DetailsView)}?ProductUniqueId={CurrentProduct.ProductUniqueId}"
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] - Erreur navigation : {ex.Message}");
                    await Shell.Current.GoToAsync(nameof(perimapp.Views.MainView));
                }
            }
        }
    }
}