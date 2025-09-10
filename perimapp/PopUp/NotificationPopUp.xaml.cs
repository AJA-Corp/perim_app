using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace perimapp.PopUp;

public partial class NotificationPopUp : Popup
{
    public NotificationPopUp()
    {
        InitializeComponent();
        
        // Limiter la hauteur du popup sur les petits écrans
        double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        MainScroll.MaximumHeightRequest = screenHeight * 0.7;

        LoadSettings();
    }

    private void LoadSettings()
    {
        var settingsJson = Preferences.Get("NotificationDays", string.Empty);
        List<int> notificationDays;

        if (!string.IsNullOrEmpty(settingsJson))
        {
            // Si des paramètres existent, on les charge
            notificationDays = JsonSerializer.Deserialize<List<int>>(settingsJson);
        }
        else
        {
            // Sinon, on définit les valeurs par défaut
            notificationDays = new List<int> { 1, 3, 7 };
        }

        // Met à jour l'état de chaque Switch en fonction de la liste (par défaut ou chargée)
        OneDayEntry.IsToggled = notificationDays.Contains(1);
        TwoDaysEntry.IsToggled = notificationDays.Contains(2);
        ThreeDaysEntry.IsToggled = notificationDays.Contains(3);
        FourDaysEntry.IsToggled = notificationDays.Contains(4);
        FiveDaysEntry.IsToggled = notificationDays.Contains(5);
        SixDaysEntry.IsToggled = notificationDays.Contains(6);
        SevenDaysEntry.IsToggled = notificationDays.Contains(7);
    }

    private void OnValidateClicked(object sender, EventArgs e)
    {
        var daysToNotify = new List<int>();

        if (OneDayEntry.IsToggled) daysToNotify.Add(1);
        if (TwoDaysEntry.IsToggled) daysToNotify.Add(2);
        if (ThreeDaysEntry.IsToggled) daysToNotify.Add(3);
        if (FourDaysEntry.IsToggled) daysToNotify.Add(4);
        if (FiveDaysEntry.IsToggled) daysToNotify.Add(5);
        if (SixDaysEntry.IsToggled) daysToNotify.Add(6);
        if (SevenDaysEntry.IsToggled) daysToNotify.Add(7);

        // Convertit la liste en JSON et la sauvegarde dans les préférences de l'appareil
        var settingsJson = JsonSerializer.Serialize(daysToNotify);
        Preferences.Set("NotificationDays", settingsJson);

        // On ferme le pop-up
        Close();
    }
}