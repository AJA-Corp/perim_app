using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perimapp.Pages
{
    public partial class StartingPage : ContentPage
    {
        public StartingPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void OnLogInClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(LogInPage));
            }
            catch (Exception error)
            {
                Console.WriteLine("[DEBUG] " + error);
                await DisplayAlert(
                    "Erreur",
                    "Une erreur est survenue lors de la navigation.",
                    "OK"
                );
            }
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(SignUpPage));
            }
            catch (Exception error)
            {
                Console.WriteLine("[DEBUG] " + error);
                await DisplayAlert(
                    "Erreur",
                    "Une erreur est survenue lors de la navigation.",
                    "OK"
                );
            }
        }
    }
}
