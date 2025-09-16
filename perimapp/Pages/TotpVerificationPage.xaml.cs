using perimapp.Services;

namespace perimapp.Pages;

[QueryProperty(nameof(UserEmail), "email")]
public partial class TotpVerificationPage : ContentPage
{
    private readonly NeonUserService _userService = new();
    private string _userEmail;

    public string UserEmail
    {
        get => _userEmail;
        set => _userEmail = value;
    }

    public TotpVerificationPage()
    {
        InitializeComponent();
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        string totpCode = TotpCodeEntry.Text?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(totpCode) || totpCode.Length != 6)
        {
            await DisplayAlert("Erreur", "Veuillez entrer un code à 6 chiffres.", "OK");
            return;
        }

        // Valider le code TOTP et obtenir l'ID utilisateur
        int userId = await _userService.AuthenticateUserWithTotpAsync(UserEmail, totpCode);
        
        if (userId > 0)
        {
            // Connexion réussie
            await SecureStorage.SetAsync("user_id", userId.ToString());
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
        else
        {
            await DisplayAlert("Erreur", "Code incorrect. Vérifiez votre application d'authentification.", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}