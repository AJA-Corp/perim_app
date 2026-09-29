using perimapp.Data;
using perimapp.Services;
using perimapp.ViewModels;
using Microsoft.Maui.Controls;

namespace perimapp.Views;

public partial class LogInView : ContentPage
{
    public LogInView(AuthService authService, ApiProfileService apiProfileService, LocalUserService localUserService)
    {
        InitializeComponent();
        BindingContext = new LogInViewModel(authService, apiProfileService, localUserService);
    }

    private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        PasswordIcon.Source = PasswordEntry.IsPassword ? "visibility_off_white.png" : "visibility_white.png";
    }
}
