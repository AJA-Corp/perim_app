using perimapp.Data;
using perimapp.Services;

namespace perimapp.Pages;

public partial class LogInPage : ContentPage
{
    private readonly NeonUserService _userService = new();
    private readonly EmailService _emailService = new();
    private readonly LoginVerificationService _verificationService = new();

    public LogInPage()
    {
        InitializeComponent();
    }
    
    int userId = -1;

    private async void OnNextLogInClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim() ?? "";
        string password = PasswordEntry.Text ?? "";
        string homeCodeText = CodeFoyerEntry.Text?.Trim() ?? "";
        string userEmail = "";
        string loginMethod = "";

        // Tentative 1 : Connexion avec email et mot de passe
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            userId = await _userService.AuthenticateUserAsync(email, password);
            userEmail = email;
            loginMethod = "email";
        }
        // Tentative 2 : Si email/password ne sont pas remplis, on essaie le code foyer
        else if (!string.IsNullOrWhiteSpace(homeCodeText))
        {
            // On convertit le texte du code foyer en un entier de manière sécurisée
            if (int.TryParse(homeCodeText, out int homeCode))
            {
                userId = await _userService.AuthenticateByHomeCodeAsync(homeCode);
                if (userId > 0)
                {
                    // Get user email for verification
                    userEmail = await _userService.GetUserEmailAsync(userId);
                }
                loginMethod = "homecode";
            }
            else
            {
                // Le texte du code foyer n'est pas un nombre
                await DisplayAlert("Erreur", "Le code foyer est invalide. Veuillez entrer un nombre.", "OK");
                return;
            }
        }
        else
        {
            // Aucun champ de connexion n'est rempli
            await DisplayAlert("Erreur", "Email ou mot de passe incorrect.", "OK");
            return;
        }

        // Gérer le résultat de la connexion, quelle que soit la méthode utilisée
        if (userId > 0 && !string.IsNullOrWhiteSpace(userEmail))
        {
            // Create verification session
            var sessionId = await _verificationService.CreateVerificationSessionAsync(userId, userEmail, loginMethod);
            var session = _verificationService.GetSession(sessionId);
            
            // Send confirmation email
            var emailSent = await _emailService.SendLoginConfirmationEmailAsync(userEmail, session.VerificationCode);
            
            if (emailSent)
            {
                // Navigate to email verification page
                await Shell.Current.GoToAsync($"{nameof(EmailVerificationPage)}?sessionId={sessionId}");
            }
            else
            {
                await DisplayAlert("Erreur", "Impossible d'envoyer l'email de confirmation. Veuillez réessayer.", "OK");
            }
        }
        else
        {
            // Si l'ID est -1, la connexion a échoué (mauvais identifiants ou code)
            await DisplayAlert("Erreur", "Identifiants ou code foyer incorrect.", "OK");
        }
    }

    private async void OnBackLogInClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(StartingPage));
    }
}
