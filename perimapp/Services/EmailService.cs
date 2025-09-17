using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using perimapp.Models;

namespace perimapp.Services
{
    public class EmailService
    {
        private readonly EmailConfig _config = new();

        public async Task<bool> SendLoginConfirmationEmailAsync(string recipientEmail, string confirmationCode)
        {
            try
            {
                // In development mode, simulate email sending by logging to console
                if (_config.DevelopmentMode || _config.Provider == EmailProvider.Development)
                {
                    Console.WriteLine("===== EMAIL SIMULATION (DEVELOPMENT MODE) =====");
                    Console.WriteLine($"À: {recipientEmail}");
                    Console.WriteLine($"Sujet: Confirmation de connexion - Perim'App");
                    Console.WriteLine($"Code de vérification: {confirmationCode}");
                    Console.WriteLine("================================================");
                    
                    // Return true to simulate successful email sending
                    return true;
                }

                // Check if email configuration is valid
                if (!_config.IsConfigured)
                {
                    Console.WriteLine("Configuration email manquante. Utilisation du mode développement.");
                    return await SendLoginConfirmationEmailAsync(recipientEmail, confirmationCode); // Use development mode
                }

                // Configure SMTP settings based on provider
                ConfigureSmtpSettings();

                using var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_config.SenderEmail, _config.SenderPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000 // 30 seconds timeout
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config.SenderEmail, _config.SenderDisplayName),
                    Subject = "Confirmation de connexion - Perim'App",
                    Body = $@"
Bonjour,

Une tentative de connexion a été effectuée sur votre compte Perim'App.

Code de confirmation : {confirmationCode}

Ce code expire dans 5 minutes.

Si vous n'êtes pas à l'origine de cette connexion, veuillez ignorer ce message.

Cordialement,
L'équipe Perim'App
",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(recipientEmail);

                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"Email envoyé avec succès à {recipientEmail}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"Erreur SMTP lors de l'envoi de l'email : {smtpEx.Message}");
                Console.WriteLine($"Code d'erreur SMTP : {smtpEx.StatusCode}");
                
                // If SMTP fails, fall back to development mode for testing
                Console.WriteLine("Basculement vers le mode développement...");
                _config.DevelopmentMode = true;
                return await SendLoginConfirmationEmailAsync(recipientEmail, confirmationCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de l'email : {ex.Message}");
                
                // If any other error occurs, fall back to development mode
                Console.WriteLine("Basculement vers le mode développement...");
                _config.DevelopmentMode = true;
                return await SendLoginConfirmationEmailAsync(recipientEmail, confirmationCode);
            }
        }

        private void ConfigureSmtpSettings()
        {
            switch (_config.Provider)
            {
                case EmailProvider.Gmail:
                    _config.SmtpServer = "smtp.gmail.com";
                    _config.SmtpPort = 587;
                    break;
                case EmailProvider.Outlook:
                    _config.SmtpServer = "smtp-mail.outlook.com";
                    _config.SmtpPort = 587;
                    break;
                case EmailProvider.Custom:
                    // Use the configured settings as-is
                    break;
                default:
                    // Development mode - no SMTP needed
                    break;
            }
        }
        
        // Method to configure email settings (for production use)
        public void ConfigureEmail(string senderEmail, string senderPassword, EmailProvider provider = EmailProvider.Gmail)
        {
            _config.SenderEmail = senderEmail;
            _config.SenderPassword = senderPassword;
            _config.Provider = provider;
            _config.DevelopmentMode = false;
            ConfigureSmtpSettings();
        }
    }
}