namespace perimapp.Models
{
    public class EmailConfig
    {
        // SMTP Configuration - Update these values with your email credentials
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        
        // TODO: Replace with your email credentials
        public string SenderEmail { get; set; } = "aja.corp.perimapp@gmail.com"; // Replace with your Gmail address
        public string SenderPassword { get; set; } = "bguw hftm pcvn wiqt"; // Replace with your Gmail App Password
        public string SenderDisplayName { get; set; } = "Perim'App";
        
        // Validation
        public bool IsConfigured => !string.IsNullOrEmpty(SenderEmail) && 
                                   !string.IsNullOrEmpty(SenderPassword) &&
                                   SenderEmail != "votre-email@gmail.com" &&
                                   SenderPassword != "votre-mot-de-passe-application";
    }
}