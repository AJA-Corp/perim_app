namespace perimapp.Models
{
    public class EmailConfig
    {
        // Default configuration uses a development mode (logs to console instead of sending emails)
        public bool DevelopmentMode { get; set; } = true;
        
        // SMTP Configuration
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = "";
        public string SenderPassword { get; set; } = ""; // Should be an App Password for Gmail
        public string SenderDisplayName { get; set; } = "Perim'App";
        
        // Alternative email services configuration
        public EmailProvider Provider { get; set; } = EmailProvider.Development;
        
        // Validation
        public bool IsConfigured => !string.IsNullOrEmpty(SenderEmail) && !string.IsNullOrEmpty(SenderPassword);
    }
    
    public enum EmailProvider
    {
        Development,
        Gmail,
        Outlook,
        Custom
    }
}