namespace perimapp.Models
{
    public class EmailConfig
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = "perimapp.notification@gmail.com";
        public string SenderPassword { get; set; } = "perimapp123!"; // In production, use secure configuration
        public string SenderDisplayName { get; set; } = "Perim'App";
    }
}