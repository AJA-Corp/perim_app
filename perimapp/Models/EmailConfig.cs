namespace perimapp.Models
{
    public class EmailConfig
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        
        public string SenderEmail { get; set; } = "aja.corp.perimapp@gmail.com";
        public string SenderPassword { get; set; } = "bguw hftm pcvn wiqt";
        public string SenderDisplayName { get; set; } = "Perim'App";
        
        // Validation
        public bool IsConfigured => !string.IsNullOrEmpty(SenderEmail) && 
                                   !string.IsNullOrEmpty(SenderPassword) &&
                                   SenderEmail != "votre-email@gmail.com" &&
                                   SenderPassword != "votre-mot-de-passe-application";
    }
}