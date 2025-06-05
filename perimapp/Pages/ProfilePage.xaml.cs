using Microsoft.Maui.Controls;
using perimapp.Popups;
using CommunityToolkit.Maui.Views;

namespace perimapp.Pages
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
            // Vous pouvez ajouter d'autres initialisations spécifiques à cette page ici si nécessaire
        }
        
        private void OnActivateNotificationsClicked(object sender, EventArgs e)
        {
            var popup = new NotificationFormPopup();
            this.ShowPopup(popup);
            //await DisplayAlert("Notifications", "Les notifications ont été activées.", "OK");
        }

        // Vous pouvez ajouter ici des gestionnaires d'événements spécifiques à cette page
        // Par exemple, si vous avez des boutons ou d'autres interactions dans ProfilePage.xaml
    }
}