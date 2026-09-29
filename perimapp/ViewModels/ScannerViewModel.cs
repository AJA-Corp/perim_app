using System;
using System.Linq;
using System.Threading.Tasks;
using BarcodeScanning;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace perimapp.ViewModels
{
    public partial class ScannerViewModel : ObservableObject
    {
        private bool _isProcessing = false;
        
        public Action<string> OnBarcodeScanned { get; set; }
        
        [ObservableProperty]
        private bool _isCameraEnabled = false;

        [ObservableProperty]
        private bool _isTorchOn = false;

        [ObservableProperty]
        private Color _flashButtonBackgroundColor = Color.FromArgb("#80000000");

        public ScannerViewModel()
        {
        }

        public void StartCamera()
        {
            _isProcessing = false;
            IsCameraEnabled = true;
        }

        public void StopCamera()
        {
            IsCameraEnabled = false;
        }

        public void HandleBarcodeScanned(OnDetectionFinishedEventArg e)
        {
            if (_isProcessing) return;

            if (e.BarcodeResults.Count > 0)
            {
                var barcode = e.BarcodeResults.FirstOrDefault()?.DisplayValue;

                if (!string.IsNullOrEmpty(barcode))
                {
                    _isProcessing = true; 

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            await Shell.Current.CurrentPage.Navigation.PopModalAsync();

                            if (OnBarcodeScanned != null)
                                OnBarcodeScanned.Invoke(barcode);
                            else
                                Console.WriteLine("Aucun callback d\u00e9fini pour le code-barres scann\u00e9");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erreur lors de la fermeture du scanner : {ex.Message}");
                        }
                    });
                }
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            if (_isProcessing) return;
            _isProcessing = true; 

            await Shell.Current.CurrentPage.Navigation.PopModalAsync();
        }

        [RelayCommand]
        private void ToggleFlash()
        {
            IsTorchOn = !IsTorchOn;

            FlashButtonBackgroundColor = IsTorchOn 
                ? Color.FromArgb("#58BF7F") 
                : Color.FromArgb("#80000000");
        }
    }
}