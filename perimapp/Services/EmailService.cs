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
                using var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_config.SenderEmail, _config.SenderPassword)
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
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de l'email : {ex.Message}");
                return false;
            }
        }
    }
}