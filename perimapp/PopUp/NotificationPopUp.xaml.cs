using CommunityToolkit.Maui.Views;
using System.Text.Json;

namespace perimapp.PopUp;

public partial class NotificationPopUp : Popup
{
    public NotificationPopUp()
    {
        InitializeComponent();
        
        var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
        double density = displayInfo.Density > 0 ? displayInfo.Density : 1;
        double screenHeight = displayInfo.Height / density;
        MainScroll.MaximumHeightRequest = screenHeight * 0.7;

        LoadSettings();
    }

    private void LoadSettings()
    {
        var settingsJson = Preferences.Get("NotificationDays", string.Empty);
        List<int> notificationDays;

        if (!string.IsNullOrEmpty(settingsJson))
        {
            try
            {
                notificationDays = JsonSerializer.Deserialize<List<int>>(settingsJson) ?? new List<int> { 1, 3, 7 };
            }
            catch
            {
                notificationDays = new List<int> { 1, 3, 7 };
            }
        }
        else
        {
            notificationDays = new List<int> { 1, 3, 7 };
        }
        OneDayEntry.IsToggled = notificationDays.Contains(1);
        TwoDaysEntry.IsToggled = notificationDays.Contains(2);
        ThreeDaysEntry.IsToggled = notificationDays.Contains(3);
        FourDaysEntry.IsToggled = notificationDays.Contains(4);
        FiveDaysEntry.IsToggled = notificationDays.Contains(5);
        SixDaysEntry.IsToggled = notificationDays.Contains(6);
        SevenDaysEntry.IsToggled = notificationDays.Contains(7);
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        var daysToNotify = new List<int>();

        if (OneDayEntry.IsToggled) daysToNotify.Add(1);
        if (TwoDaysEntry.IsToggled) daysToNotify.Add(2);
        if (ThreeDaysEntry.IsToggled) daysToNotify.Add(3);
        if (FourDaysEntry.IsToggled) daysToNotify.Add(4);
        if (FiveDaysEntry.IsToggled) daysToNotify.Add(5);
        if (SixDaysEntry.IsToggled) daysToNotify.Add(6);
        if (SevenDaysEntry.IsToggled) daysToNotify.Add(7);

        var settingsJson = JsonSerializer.Serialize(daysToNotify);
        Preferences.Set("NotificationDays", settingsJson);

        Services.NotificationScheduler.UpdateSchedules();

        await CloseAsync();
    }
}