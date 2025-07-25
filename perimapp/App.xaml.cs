using Microsoft.Maui.Storage;
using perimapp.Data;
using perimapp.Pages;

namespace perimapp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        int savedUserId = Preferences.Default.Get("UserId", -1);
        Console.WriteLine($"[DEBUG] ID utilisateur récupéré depuis Preferences : {savedUserId}");

        if (savedUserId > 0)
        {
            AppData.CurrentUserId = savedUserId;
            return new Window(new NavigationPage(new MainPage())); // Utilisateur déjà connecté
        }
        else
        {
            return new Window(new NavigationPage(new StartingPage())); // Connexion nécessaire
        }
    }
}
