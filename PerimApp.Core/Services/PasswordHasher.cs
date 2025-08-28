using Isopoh.Cryptography.Argon2;

namespace PerimApp.Core.Services
{
    /// <summary>
    /// Service de hachage de mots de passe utilisant Argon2
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Hache un mot de passe en utilisant Argon2
        /// </summary>
        /// <param name="password">Le mot de passe à hacher</param>
        /// <returns>Le mot de passe haché</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Le mot de passe ne peut pas être vide", nameof(password));

            return Argon2.Hash(password);
        }

        /// <summary>
        /// Vérifie un mot de passe contre son hash
        /// </summary>
        /// <param name="password">Le mot de passe en clair</param>
        /// <param name="hash">Le hash à vérifier</param>
        /// <returns>True si le mot de passe correspond au hash</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;
            
            if (string.IsNullOrWhiteSpace(hash))
                return false;

            try
            {
                return Argon2.Verify(hash, password);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valide la force d'un mot de passe
        /// </summary>
        /// <param name="password">Le mot de passe à valider</param>
        /// <returns>True si le mot de passe est suffisamment fort</returns>
        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            return password.Length >= 8
                && password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(c => !char.IsLetterOrDigit(c)); // Caractère spécial
        }

        /// <summary>
        /// Obtient des recommandations pour améliorer un mot de passe
        /// </summary>
        /// <param name="password">Le mot de passe à analyser</param>
        /// <returns>Liste des recommandations</returns>
        public static List<string> GetPasswordRecommendations(string password)
        {
            var recommendations = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                recommendations.Add("Le mot de passe ne peut pas être vide");
                return recommendations;
            }

            if (password.Length < 8)
                recommendations.Add("Le mot de passe doit contenir au moins 8 caractères");

            if (!password.Any(char.IsUpper))
                recommendations.Add("Le mot de passe doit contenir au moins une majuscule");

            if (!password.Any(char.IsLower))
                recommendations.Add("Le mot de passe doit contenir au moins une minuscule");

            if (!password.Any(char.IsDigit))
                recommendations.Add("Le mot de passe doit contenir au moins un chiffre");

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                recommendations.Add("Le mot de passe doit contenir au moins un caractère spécial");

            return recommendations;
        }
    }
}