using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using perimapp.Views;

namespace perimapp.ViewModels
{
    public partial class ModifyProductViewModel : ObservableObject
    {
        private readonly LocalProductService _localProductService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

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

        public ModifyProductViewModel(
            LocalProductService localProductService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _localProductService = localProductService;
            _navigationService = navigationService;
            _dialogService = dialogService;
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
                    await _dialogService.ShowAlertAsync("Erreur", "Produit à modifier non trouvé.", "OK");
                    await _navigationService.GoToAsync($"///{nameof(MainView)}");
                }
            }
            else
            {
                Debug.WriteLine("ModifyProductView: Aucun ProductUniqueId fourni pour la modification.");
                await _dialogService.ShowAlertAsync("Erreur", "Impossible de modifier. Aucun ID de produit fourni.", "OK");
                await _navigationService.GoToAsync("..");
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
                    await _dialogService.ShowAlertAsync("Erreur", "Le nom du produit ne peut pas être vide.", "OK");
                    return;
                }

                if (CurrentQuantity < 1)
                {
                    await _dialogService.ShowAlertAsync("Erreur", "La quantité doit être supérieure ou égale à 1.", "OK");
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

                await _dialogService.ShowAlertAsync("Succès", "Produit modifié avec succès !", "OK");

                try
                {
                    await _navigationService.GoToAsync($"{nameof(DetailsView)}?ProductUniqueId={CurrentProduct.ProductUniqueId}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] - Erreur navigation : {ex.Message}");
                    await _navigationService.GoToAsync($"///{nameof(MainView)}");
                }
            }
        }
    }
}