using perimapp.Views;
using perimapp.PopUp;

namespace perimapp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(AddProductView), typeof(AddProductView));
        Routing.RegisterRoute(nameof(DeletedProductView), typeof(DeletedProductView));
        Routing.RegisterRoute(nameof(DetailsView), typeof(DetailsView));
        Routing.RegisterRoute(nameof(EmailVerificationView), typeof(EmailVerificationView));
        Routing.RegisterRoute(nameof(LogInView), typeof(LogInView));
        Routing.RegisterRoute(nameof(ModifyProductView), typeof(ModifyProductView));
        Routing.RegisterRoute(nameof(ProfileView), typeof(ProfileView));
        Routing.RegisterRoute(nameof(ScannerView), typeof(ScannerView));
        Routing.RegisterRoute(nameof(SignUpView), typeof(SignUpView));
    }
}
