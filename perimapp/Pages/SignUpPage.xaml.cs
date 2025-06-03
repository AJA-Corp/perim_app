using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perimapp.Pages;

public partial class SignUpPage : ContentPage
{
    public SignUpPage()
    {
        InitializeComponent();
    }
    
    private async void OnNextSignUpClicked(object sender, EventArgs e)
    {
            
        await Shell.Current.GoToAsync(nameof(MainPage));
            
    }

    private async void OnBackSignUpClicked(object sender, EventArgs e)
    {

        await Shell.Current.GoToAsync(nameof(StartingPage));

    }
}