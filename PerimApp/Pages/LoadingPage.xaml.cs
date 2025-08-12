// Pages/LoadingPage.xaml.cs
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Data;

namespace perimapp.Pages;

public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        InitializeComponent();
        //_ = LaunchAppAsync(); // Lancer la redirection
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        int savedUserId = Preferences.Default.Get("UserId", -1);
        Console.WriteLine($"[DEBUG] ID utilisateur récupéré depuis Preferences : {savedUserId}");

        await Task.Delay(5000); // petit délai pour laisser Shell s'initialiser

        if (savedUserId > 0)
        {
            AppData.CurrentUserId = savedUserId;
            await Shell.Current.GoToAsync(nameof(MainPage));
        }
        else
        {
            await Shell.Current.GoToAsync(nameof(StartingPage));
        }
    }

    /*
    private async Task LaunchAppAsync()
    {
        await Task.Delay(5000); // Attendre 2 secondes (personnalisable)
        await Navigation.PushAsync(new StartingPage());
    }
    */
}
