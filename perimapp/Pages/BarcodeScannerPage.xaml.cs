using ZXing.Net.Maui;

namespace perimapp.Pages;

public partial class BarcodeScannerPage : ContentPage
{
    public BarcodeScannerPage()
    {
        InitializeComponent();
    }

    private bool _barcodeDetected = false;
    private bool isScanning = false;

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (isScanning)
            return; // ignore les détections suivantes

        var result = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(result))
            return;

        isScanning = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // stoppe la détection
                cameraView.IsDetecting = false;

                // mise à jour de l'UI
                ResultLabel.Text = $"✅ Détecté : {result}";

                // envoie le code-barres à la page précédente
                MessagingCenter.Send(this, "BarcodeScanned", result);

                // courte pause pour s'assurer que le message est envoyé
                await Task.Delay(200);

                // fermer la page scanner
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                // log si crash
                System.Diagnostics.Debug.WriteLine("Erreur scanner: " + ex);
            }
            finally
            {
                isScanning = false;
            }
        });
    }
}
