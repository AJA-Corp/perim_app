// AppShell.xaml.cs
using perimapp.Views;
using perimapp.PopUp;

namespace perimapp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // ENREGISTREZ TOUTES VOS ROUTES ICI
        Routing.RegisterRoute(nameof(AddProductView), typeof(AddProductView));
        Routing.RegisterRoute(nameof(DeletedProductView), typeof(DeletedProductView));
        Routing.RegisterRoute(nameof(DetailsView), typeof(DetailsView));
        Routing.RegisterRoute(nameof(EmailVerificationView), typeof(EmailVerificationView));
        Routing.RegisterRoute(nameof(LogInView), typeof(LogInView));
        Routing.RegisterRoute(nameof(MainView), typeof(MainView));
        Routing.RegisterRoute(nameof(ModifyProductView), typeof(ModifyProductView));
        Routing.RegisterRoute(nameof(NotificationPopUp), typeof(NotificationPopUp));
        Routing.RegisterRoute(nameof(ProfileView), typeof(ProfileView));
        Routing.RegisterRoute(nameof(ScannerView), typeof(ScannerView));
        Routing.RegisterRoute(nameof(SignUpView), typeof(SignUpView));
        Routing.RegisterRoute(nameof(StartingView), typeof(StartingView));

        CurrentItem = new ShellContent { Content = new LoadingView() };
    }
}
