using PerimApp.Core.Models;

namespace PerimApp.Core.Utilities
{
    /// <summary>
    /// Utilitaires pour la validation des données
    /// </summary>
    public static class ValidationUtils
    {
        /// <summary>
        /// Valide un code-barres
        /// </summary>
        /// <param name="barcode">Le code-barres à valider</param>
        /// <returns>True si le code-barres est valide</returns>
        public static bool IsValidBarcode(long barcode)
        {
            // Un code-barres doit être positif et avoir au moins 8 chiffres
            return barcode > 0 && barcode.ToString().Length >= 8;
        }

        /// <summary>
        /// Valide une quantité
        /// </summary>
        /// <param name="quantity">La quantité à valider</param>
        /// <returns>True si la quantité est valide</returns>
        public static bool IsValidQuantity(int quantity)
        {
            return quantity > 0 && quantity <= 999;
        }

        /// <summary>
        /// Valide une date de péremption
        /// </summary>
        /// <param name="dlc">La date à valider</param>
        /// <returns>True si la date est valide</returns>
        public static bool IsValidDlc(DateTime dlc)
        {
            // La date doit être dans le futur (ou aujourd'hui) et dans les 10 ans
            var today = DateTime.Today;
            var maxDate = today.AddYears(10);
            
            return dlc.Date >= today && dlc.Date <= maxDate;
        }

        /// <summary>
        /// Valide une URL d'image
        /// </summary>
        /// <param name="urlImage">L'URL à valider</param>
        /// <returns>True si l'URL est valide</returns>
        public static bool IsValidImageUrl(string? urlImage)
        {
            if (string.IsNullOrWhiteSpace(urlImage))
                return true; // URL vide autorisée

            try
            {
                var uri = new Uri(urlImage);
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Nettoie et normalise un nom de produit
        /// </summary>
        /// <param name="name">Le nom à nettoyer</param>
        /// <returns>Nom nettoyé</returns>
        public static string CleanProductName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return name.Trim()
                      .Replace("  ", " ") // Supprime les espaces doubles
                      .Replace("\n", " ") // Remplace les retours à la ligne
                      .Replace("\t", " "); // Remplace les tabulations
        }

        /// <summary>
        /// Valide une catégorie de produit
        /// </summary>
        /// <param name="category">La catégorie à valider</param>
        /// <returns>True si la catégorie est valide</returns>
        public static bool IsValidCategory(string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return true; // Catégorie vide autorisée

            var validCategories = new[]
            {
                "Fruits", "Légumes", "Viandes", "Poissons", "Produits laitiers",
                "Céréales", "Boissons", "Conserves", "Surgelés", "Épices",
                "Condiments", "Boulangerie", "Pâtisserie", "Autre"
            };

            return validCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtient les erreurs de validation pour un produit
        /// </summary>
        /// <param name="product">Le produit à valider</param>
        /// <returns>Liste des erreurs</returns>
        public static List<string> GetProductValidationErrors(ProductInfos product)
        {
            var errors = new List<string>();

            if (product == null)
            {
                errors.Add("Le produit ne peut pas être null");
                return errors;
            }

            if (!IsValidBarcode(product.Barcode))
                errors.Add("Le code-barres n'est pas valide");

            if (string.IsNullOrWhiteSpace(product.Name))
                errors.Add("Le nom du produit est obligatoire");

            if (!IsValidQuantity(product.Quantity))
                errors.Add("La quantité doit être comprise entre 1 et 999");

            if (!IsValidDlc(product.Dlc))
                errors.Add("La date de péremption n'est pas valide");

            if (!IsValidImageUrl(product.UrlImage))
                errors.Add("L'URL de l'image n'est pas valide");

            if (product.AddedAt > DateTime.Now)
                errors.Add("La date d'ajout ne peut pas être dans le futur");

            return errors;
        }
    }

    /// <summary>
    /// Utilitaires pour les dates
    /// </summary>
    public static class DateUtils
    {
        /// <summary>
        /// Calcule le nombre de jours entre deux dates
        /// </summary>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <returns>Nombre de jours</returns>
        public static int DaysBetween(DateTime startDate, DateTime endDate)
        {
            return (endDate.Date - startDate.Date).Days;
        }

        /// <summary>
        /// Détermine si une date est dans le passé
        /// </summary>
        /// <param name="date">La date à vérifier</param>
        /// <returns>True si la date est passée</returns>
        public static bool IsInPast(DateTime date)
        {
            return date.Date < DateTime.Today;
        }

        /// <summary>
        /// Détermine si une date est aujourd'hui
        /// </summary>
        /// <param name="date">La date à vérifier</param>
        /// <returns>True si la date est aujourd'hui</returns>
        public static bool IsToday(DateTime date)
        {
            return date.Date == DateTime.Today;
        }

        /// <summary>
        /// Détermine si une date est dans le futur proche (dans les X jours)
        /// </summary>
        /// <param name="date">La date à vérifier</param>
        /// <param name="daysThreshold">Seuil en jours</param>
        /// <returns>True si la date est dans le futur proche</returns>
        public static bool IsInNearFuture(DateTime date, int daysThreshold = 7)
        {
            var threshold = DateTime.Today.AddDays(daysThreshold);
            return date.Date > DateTime.Today && date.Date <= threshold;
        }

        /// <summary>
        /// Formate une date pour l'affichage français
        /// </summary>
        /// <param name="date">La date à formater</param>
        /// <returns>Date formatée</returns>
        public static string FormatFrenchDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Formate une date avec l'heure pour l'affichage français
        /// </summary>
        /// <param name="date">La date à formater</param>
        /// <returns>Date et heure formatées</returns>
        public static string FormatFrenchDateTime(DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm");
        }
    }
}