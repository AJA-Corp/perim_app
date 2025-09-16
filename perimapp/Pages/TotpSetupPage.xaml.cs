using perimapp.Services;
using perimapp.Models;
using System.IO;

namespace perimapp.Pages;

[QueryProperty(nameof(UserEmail), "email")]
public partial class TotpSetupPage : ContentPage
{
    private readonly NeonUserService _userService = new();
    private readonly TotpService _totpService = new();
    private UserProfileDetails _userProfile;
    private string _userEmail;

    public string UserEmail
    {
        get => _userEmail;
        set
        {
            _userEmail = value;
            if (!string.IsNullOrEmpty(value))
            {
                LoadTotpSetup();
            }
        }
    }

    public TotpSetupPage()
    {
        InitializeComponent();
    }

    private async void LoadTotpSetup()
    {
        try
        {
            // Récupérer les informations TOTP de l'utilisateur
            _userProfile = await _userService.GetUserTotpInfoAsync(UserEmail);
            
            if (_userProfile != null && !string.IsNullOrEmpty(_userProfile.TotpSecret))
            {
                // Afficher le secret en format lisible
                SecretLabel.Text = FormatSecret(_userProfile.TotpSecret);
                
                // Générer et afficher le QR code
                var qrCodeBytes = _totpService.GenerateQrCode(UserEmail, _userProfile.TotpSecret);
                QrCodeImage.Source = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
            }
            else
            {
                await DisplayAlert("Erreur", "Impossible de charger les informations TOTP.", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TotpSetupPage] Erreur : {ex.Message}");
            await DisplayAlert("Erreur", "Une erreur s'est produite lors du chargement.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private string FormatSecret(string secret)
    {
        // Formate le secret en groupes de 4 caractères pour faciliter la saisie manuelle
        var formatted = "";
        for (int i = 0; i < secret.Length; i += 4)
        {
            if (i > 0) formatted += " ";
            formatted += secret.Substring(i, Math.Min(4, secret.Length - i));
        }
        return formatted;
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        string totpCode = TotpCodeEntry.Text?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(totpCode) || totpCode.Length != 6)
        {
            await DisplayAlert("Erreur", "Veuillez entrer un code à 6 chiffres.", "OK");
            return;
        }

        // Valider le code TOTP
        bool isValid = _totpService.ValidateCode(_userProfile.TotpSecret, totpCode);
        
        if (isValid)
        {
            await DisplayAlert("Succès", "Authentification à deux facteurs configurée avec succès !", "OK");
            
            // Récupérer l'ID utilisateur et le stocker
            int userId = await _userService.AuthenticateUserWithTotpAsync(UserEmail, totpCode);
            if (userId > 0)
            {
                await SecureStorage.SetAsync("user_id", userId.ToString());
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                await DisplayAlert("Erreur", "Erreur lors de la finalisation de la configuration.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Erreur", "Code incorrect. Vérifiez votre application d'authentification.", "OK");
        }
    }
}