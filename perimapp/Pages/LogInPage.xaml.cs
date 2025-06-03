using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perimapp.Pages;

public partial class LogInPage : ContentPage
{
    public LogInPage()
    {
        InitializeComponent();
    }
    
    private async void OnNextLogInClicked(object sender, EventArgs e)
    {
            
        await Shell.Current.GoToAsync(nameof(MainPage));
            
    }

    private async void OnBackLogInClicked(object sender, EventArgs e)
    {

        await Shell.Current.GoToAsync(nameof(StartingPage));

    }
}