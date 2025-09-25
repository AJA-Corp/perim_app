using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace perimapp.Pages;

public partial class BarcodeScannerPage : ContentPage
{
    private readonly Action<string> _onDetected;
    private bool _isScanning = false;

    public BarcodeScannerPage(Action<string> onDetected)
    {
        InitializeComponent();
        _onDetected = onDetected;
    }

    private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_isScanning)
            return;

        var result = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(result))
            return;

        _isScanning = true;

        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // 🔍 Stopper la détection pour éviter des doublons
                cameraView.IsDetecting = false;

                // ✅ Transmettre le code-barres détecté à la page précédente
                _onDetected?.Invoke(result);

                // ⏱️ Petit délai pour s’assurer que l’UI s’actualise
                await Task.Delay(200);

                // 🔙 Fermer la page scanner proprement
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                // 📛 Gestion d’erreurs pour éviter les crashs silencieux
                await DisplayAlert("Erreur Scanner", $"Une erreur est survenue : {ex.Message}", "OK");
                await Navigation.PopModalAsync();
            }
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // 🧹 Libération propre pour éviter les fuites mémoire
        cameraView.IsDetecting = true;
        _isScanning = false;
    }
}