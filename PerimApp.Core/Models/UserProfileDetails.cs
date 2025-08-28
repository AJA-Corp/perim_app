using System.Text.Json.Serialization;

namespace PerimApp.Core.Models
{
    public class UserProfileDetails
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("home_code")]
        public int HomeCode { get; set; }

        [JsonPropertyName("lost_products")]
        public string LostProducts { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de produits enregistrés (non inclus dans la sérialisation JSON)
        /// </summary>
        [JsonIgnore]
        public int RegisteredProductsCount { get; set; }

        /// <summary>
        /// Nom complet de l'utilisateur
        /// </summary>
        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// Valide les données de l'utilisateur
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && IsValidEmail(Email)
                && !string.IsNullOrWhiteSpace(Password)
                && HomeCode >= 100000
                && HomeCode <= 999999;
        }

        /// <summary>
        /// Valide le format de l'email
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valide la force du mot de passe
        /// </summary>
        public bool IsPasswordStrong()
        {
            if (string.IsNullOrWhiteSpace(Password))
                return false;

            return Password.Length >= 8
                && Password.Any(char.IsUpper)
                && Password.Any(char.IsLower)
                && Password.Any(char.IsDigit);
        }
    }
}