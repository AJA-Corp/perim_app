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
        }

        private async void OnLogInClicked(object sender, EventArgs e)
        {
            
            await Shell.Current.GoToAsync(nameof(LogInPage));
            
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {

            await Shell.Current.GoToAsync(nameof(SignUpPage));

        }
    }
}