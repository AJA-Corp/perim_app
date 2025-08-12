using perimapp.Data;
using perimapp.Services;

namespace perimapp.Pages;

public partial class LogInPage : ContentPage
{
    private readonly NeonUserService _userService = new();

    public LogInPage()
    {
        InitializeComponent();
    }

    private async void OnNextLogInClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim() ?? "";
        string password = PasswordEntry.Text ?? "";
        string homeCode = CodeFoyerEntry.Text?.Trim() ?? "";

        if (
            string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(homeCode)
        )
        {
            await DisplayAlert("Erreur", "Tous les champs doivent être remplis.", "OK");
            return;
        }

        int userId = await _userService.AuthenticateUserAsync(email, password);

        if (userId > 0)
        {
            AppData.CurrentUserId = userId;
            Preferences.Default.Set("UserId", userId);
            await DisplayAlert("Succès", "Connexion réussie !", "OK");
            Console.WriteLine($"[DEBUG] Navigation vers route : {nameof(MainPage)}");
            await Shell.Current.GoToAsync(nameof(MainPage));
        }
        else
        {
            Console.WriteLine("[DEBUG]: Email ou mot de passe incorrect");
            await DisplayAlert("Erreur", "Email ou mot de passe incorrect.", "OK");
        }
    }

    private async void OnBackLogInClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(StartingPage));
    }
}
