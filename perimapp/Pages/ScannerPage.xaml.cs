using BarcodeScanning;
using System.Linq;

namespace perimapp.Pages;

public partial class ScannerPage : ContentPage
{
    // Action (callback) pour renvoyer le résultat à AddProductPage
    public Action<string> OnBarcodeScanned { get; set; }

    public ScannerPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Camera.CameraEnabled = true; // Allume la caméra quand la page s'affiche
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Camera.CameraEnabled = false; // Éteint la caméra quand on quitte
    }

    private void Camera_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
    {
        if (e.BarcodeResults.Count > 0)
        {
            // Récupère le premier code-barres trouvé
            var barcode = e.BarcodeResults.FirstOrDefault()?.DisplayValue;

            // Coupe la caméra immédiatement pour éviter les scans multiples
            Camera.CameraEnabled = false;

            // On doit retourner sur le Thread principal pour fermer la page et modifier l'UI
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                OnBarcodeScanned?.Invoke(barcode);
                await Navigation.PopModalAsync();
            });
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        Camera.CameraEnabled = false;
        await Navigation.PopModalAsync();
    }
}