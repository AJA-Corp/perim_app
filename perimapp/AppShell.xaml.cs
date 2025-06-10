// AppShell.xaml.cs
using perimapp.Pages;
using perimapp.PopUp;

namespace perimapp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // ENREGISTREZ TOUTES VOS ROUTES ICI
        Routing.RegisterRoute(nameof(AddProductPage), typeof(AddProductPage)); 
        Routing.RegisterRoute(nameof(DetailsPage), typeof(DetailsPage)); 
        Routing.RegisterRoute(nameof(LoadingPage), typeof(LoadingPage)); 
        Routing.RegisterRoute(nameof(LogInPage), typeof(LogInPage));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(ModifyProductPage), typeof(ModifyProductPage)); 
        Routing.RegisterRoute(nameof(NotificationPopUp), typeof(NotificationPopUp)); 
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage)); 
        Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
        Routing.RegisterRoute(nameof(StartingPage), typeof(StartingPage));
    }
}