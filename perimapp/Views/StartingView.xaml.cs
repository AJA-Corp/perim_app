using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using perimapp.ViewModels;

namespace perimapp.Views
{
    public partial class StartingView : ContentPage
    {
        public StartingView()
        {
            InitializeComponent();
            BindingContext = new StartingViewModel(this);
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void OnLogInClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(LogInView));
            }
            catch (Exception error)
            {
                Console.WriteLine("[DEBUG] " + error);
                await DisplayAlertAsync(
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
                await Shell.Current.GoToAsync(nameof(SignUpView));
            }
            catch (Exception error)
            {
                Console.WriteLine("[DEBUG] " + error);
                await DisplayAlertAsync(
                    "Erreur",
                    "Une erreur est survenue lors de la navigation.",
                    "OK"
                );
            }
        }
    }
}
