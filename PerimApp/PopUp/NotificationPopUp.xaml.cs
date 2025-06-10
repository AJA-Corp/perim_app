using CommunityToolkit.Maui.Views;

namespace perimapp.PopUp;

public partial class NotificationPopUp : Popup
{
    public NotificationPopUp()
    {
        InitializeComponent();

        double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        MainScroll.MaximumHeightRequest = screenHeight * 0.7;
    }


    private async void OnValidateClicked(object sender, EventArgs e)
    {
        Console.WriteLine("Le bouton 'Valider' a été cliqué !");
    /*
        // Votre code commenté reste ici
        // if (int.TryParse(RedEntry.Text, out int red) &&
        //     int.TryParse(OrangeEntry.Text, out int orange) &&
        //     int.TryParse(YellowEntry.Text, out int yellow) &&
        //     int.TryParse(GreenEntry.Text, out int green) &&
        //     int.TryParse(BlueEntry.Text, out int blue))
        // {
        //     if (red < orange && orange < yellow && yellow < green && green < blue)
        //     {
        //         await Application.Current.MainPage.DisplayAlert("Succès", "Valeurs enregistrées avec succès.", "OK");
        //         Close(); // Ferme le popup
        //     }
        //     else
        //     {
        //         await Application.Current.MainPage.DisplayAlert("Erreur", "Les valeurs doivent suivre l’ordre : rouge < orange < jaune < vert < bleu.", "OK");
        //     }
        // }
        // else
        // {
        //     await Application.Current.MainPage.DisplayAlert("Erreur", "Saisie invalide. Tous les champs doivent contenir des nombres.", "OK");
        // }
    */
    }
}