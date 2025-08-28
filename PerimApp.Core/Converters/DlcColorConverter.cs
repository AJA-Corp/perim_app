using PerimApp.Core.Models;

namespace PerimApp.Core.Converters
{
    /// <summary>
    /// Convertisseur pour déterminer la couleur selon la date limite de consommation
    /// </summary>
    public static class DlcColorConverter
    {
        public enum ColorResult
        {
            Red,     // Expiré
            Orange,  // Expire aujourd'hui ou demain
            Yellow,  // Expire dans 2-3 jours
            Green    // Expire dans plus de 3 jours
        }

        /// <summary>
        /// Détermine la couleur selon les jours restants
        /// </summary>
        /// <param name="daysRemaining">Nombre de jours restants</param>
        /// <returns>Couleur appropriée</returns>
        public static ColorResult GetColorFromDays(int daysRemaining)
        {
            return daysRemaining switch
            {
                < 0 => ColorResult.Red,        // Expiré
                0 or 1 => ColorResult.Orange,  // Aujourd'hui ou demain
                2 or 3 => ColorResult.Yellow,  // 2-3 jours
                _ => ColorResult.Green         // Plus de 3 jours
            };
        }

        /// <summary>
        /// Détermine la couleur pour un produit
        /// </summary>
        /// <param name="product">Le produit à analyser</param>
        /// <returns>Couleur appropriée</returns>
        public static ColorResult GetColorFromProduct(ProductInfos product)
        {
            if (product == null)
                return ColorResult.Green;

            return GetColorFromDays(product.DaysRemaining);
        }

        /// <summary>
        /// Obtient le code couleur hexadécimal
        /// </summary>
        /// <param name="colorResult">Résultat de couleur</param>
        /// <returns>Code couleur hexadécimal</returns>
        public static string GetHexColor(ColorResult colorResult)
        {
            return colorResult switch
            {
                ColorResult.Red => "#FF4444",     // Rouge
                ColorResult.Orange => "#FF8800",  // Orange
                ColorResult.Yellow => "#FFBB33",  // Jaune
                ColorResult.Green => "#00C851",   // Vert
                _ => "#00C851"
            };
        }

        /// <summary>
        /// Détermine si une couleur indique un danger
        /// </summary>
        /// <param name="colorResult">Résultat de couleur</param>
        /// <returns>True si c'est dangereux (rouge ou orange)</returns>
        public static bool IsDangerous(ColorResult colorResult)
        {
            return colorResult == ColorResult.Red || colorResult == ColorResult.Orange;
        }

        /// <summary>
        /// Obtient une description textuelle de l'urgence
        /// </summary>
        /// <param name="daysRemaining">Nombre de jours restants</param>
        /// <returns>Description de l'urgence</returns>
        public static string GetUrgencyDescription(int daysRemaining)
        {
            return daysRemaining switch
            {
                < 0 => "Produit expiré",
                0 => "Expire aujourd'hui",
                1 => "Expire demain",
                2 => "Expire après-demain",
                3 => "Expire dans 3 jours",
                <= 7 => "Expire cette semaine",
                <= 30 => "Expire ce mois",
                _ => "Expire plus tard"
            };
        }
    }
}