using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using perimapp.Data;
using perimapp.Models;
using perimapp.PopUp;
using perimapp.Services;
using perimapp.Views;
using CommunityToolkit.Maui.Extensions;

namespace perimapp.ViewModels
{
    public partial class ModifyProductViewModel : ObservableObject
    {
        private readonly LocalProductService _localProductService;

        [ObservableProperty]
        private string? _productUniqueId;

        [ObservableProperty]
        private ProductInfos? _currentProduct;

        [ObservableProperty]
        private int _currentQuantity;

        private DateTime _currentDlcDate = DateTime.Today;

        public DateTime CurrentDlcDate
        {
            get => _currentDlcDate;
            set => SetProperty(ref _currentDlcDate, value);
        }

        [ObservableProperty]
        private string _currentCustomName;

        public ModifyProductViewModel(LocalProductService localProductService)
        {
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
                CurrentDlcDate = value.Dlc?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
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
                    var errorPopup = new InfosPopUp("Erreur", "Produit à modifier non trouvé.", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                    await Shell.Current.GoToAsync($"///{nameof(MainView)}");
                }
            }
            else
            {
                Debug.WriteLine("ModifyProductView: Aucun ProductUniqueId fourni pour la modification.");
                var errorPopup = new InfosPopUp("Erreur", "Impossible de modifier. Aucun ID de produit fourni.", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
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
                    var errorPopup = new InfosPopUp("Erreur", "Le nom du produit ne peut pas être vide.", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                    return;
                }

                if (CurrentQuantity < 1)
                {
                    var errorPopup = new InfosPopUp("Erreur", "La quantité doit être supérieure ou égale à 1.", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                    return;
                }

                CurrentProduct.Quantity = CurrentQuantity;
                CurrentProduct.Dlc = DateOnly.FromDateTime(CurrentDlcDate.Date);

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

                NotificationScheduler.UpdateSchedules();

                var successPopup = new InfosPopUp("Succès", "Produit modifié avec succès !", "OK");
                await Shell.Current.CurrentPage.ShowPopupAsync(successPopup);

                try
                {
                    await Shell.Current.GoToAsync($"{nameof(DetailsView)}?ProductUniqueId={CurrentProduct.ProductUniqueId}");
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