// Pages/LoadingPage.xaml.cs
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace perimapp.Pages;

public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        InitializeComponent();
        _ = LaunchAppAsync(); // Lancer la redirection
    }

    private async Task LaunchAppAsync()
    {
        await Task.Delay(5000); // Attendre 2 secondes (personnalisable)
        await Navigation.PushAsync(new StartingPage());
    }
}