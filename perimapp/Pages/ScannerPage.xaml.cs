using BarcodeScanning;
using System.Linq; // Nécessaire pour FirstOrDefault()

namespace perimapp.Pages;

public partial class ScannerPage : ContentPage
{
    public Action<string> OnBarcodeScanned { get; set; }

    // Ce verrou empêche la caméra de déclencher 15 fois le même scan
    private bool _isProcessing = false;

    public ScannerPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isProcessing = false; // On déverrouille quand la page s'ouvre
        Camera.CameraEnabled = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // C'est ici et UNIQUEMENT ici qu'on coupe la caméra pour éviter les conflits
        Camera.CameraEnabled = false;
    }

    private void Camera_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
    {
        // Si on est déjà en train de traiter un code, on ignore les suivants
        if (_isProcessing) return;

        if (e.BarcodeResults.Count > 0)
        {
            var barcode = e.BarcodeResults.FirstOrDefault()?.DisplayValue;

            if (!string.IsNullOrEmpty(barcode))
            {
                _isProcessing = true; // On verrouille immédiatement !

                // On retourne sur le fil principal de l'interface (MainThread)
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // 1. On ferme d'abord la page proprement (ce qui déclenchera OnDisappearing)
                    await Navigation.PopModalAsync();

                    // 2. Ensuite on envoie le code à la page précédente
                    OnBarcodeScanned?.Invoke(barcode);
                });
            }
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (_isProcessing) return;
        _isProcessing = true; // On verrouille pour éviter les doubles clics

        // On ferme juste la page, OnDisappearing s'occupera d'éteindre la caméra
        await Navigation.PopModalAsync();
    }
}