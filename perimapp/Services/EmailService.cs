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
                // Check if email configuration is valid
                if (!_config.IsConfigured)
                {
                    Console.WriteLine("ERREUR: Configuration email manquante!");
                    Console.WriteLine("Veuillez configurer votre email et mot de passe dans EmailConfig.cs");
                    Console.WriteLine($"Code de vérification pour {recipientEmail}: {confirmationCode}");
                    return false;
                }

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
                Console.WriteLine("Vérifiez vos informations d'identification dans EmailConfig.cs");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de l'email : {ex.Message}");
                return false;
            }
        }
    }
}