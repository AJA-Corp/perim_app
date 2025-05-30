// AppShell.xaml.cs
using perimapp.Pages; // Assurez-vous d'importer le namespace où se trouvent vos pages

namespace perimapp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // ENREGISTREZ TOUTES VOS ROUTES ICI
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage)); 
        Routing.RegisterRoute(nameof(StartingPage), typeof(StartingPage));
        Routing.RegisterRoute(nameof(LogInPage), typeof(LogInPage));
        Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage)); 
    }
}