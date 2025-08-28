using System;
using System.Text.Json.Serialization;

namespace PerimApp.Core.Models
{
    public class ProductInfos
    {
        public int Id { get; set; }
        public long Barcode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UrlImage { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Conservation { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
        public DateTime Dlc { get; set; }
        public int Quantity { get; set; }

        /// <summary>
        /// Calcule le nombre de jours restants avant la date limite de consommation
        /// </summary>
        public int DaysRemaining => (Dlc.Date - DateTime.Today.Date).Days;

        /// <summary>
        /// Texte formaté pour l'affichage sur la page principale
        /// </summary>
        public string DaysRemainingTextMainPage
        {
            get
            {
                int days = DaysRemaining;
                if (days < 0) return "Exp.";
                if (days == 0) return "Auj.";
                if (days == 1) return "1j";
                return $"{days}j";
            }
        }
        
        /// <summary>
        /// Taille de police adaptée selon le nombre de jours
        /// </summary>
        public double DaysRemainingFontSize
        {
            get
            {
                if (DaysRemaining >= 1000 && DaysRemaining <= 9999)
                {
                    return 20; // 4 chiffres, on réduit la taille
                }
                return 24; // Taille de police par défaut
            }
        }

        /// <summary>
        /// Texte formaté pour l'affichage sur la page de détails
        /// </summary>
        public string DaysRemainingTextDetailsPage
        {
            get
            {
                int days = DaysRemaining;
                if (days < 0) return "Expiré";
                if (days == 0) return "Aujourd'hui";
                if (days == 1) return "1 jour";
                return $"{days} jours";
            }
        }

        /// <summary>
        /// Identifiant unique du produit
        /// </summary>
        public string ProductUniqueId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Détermine si le produit est expiré
        /// </summary>
        public bool IsExpired => DaysRemaining < 0;

        /// <summary>
        /// Détermine si le produit expire aujourd'hui
        /// </summary>
        public bool ExpiresNow => DaysRemaining == 0;

        /// <summary>
        /// Détermine si le produit expire bientôt (dans les 3 jours)
        /// </summary>
        public bool ExpiresSoon => DaysRemaining > 0 && DaysRemaining <= 3;

        /// <summary>
        /// Valide les données du produit
        /// </summary>
        public bool IsValid()
        {
            return Barcode > 0 
                && !string.IsNullOrWhiteSpace(Name) 
                && Quantity > 0 
                && Dlc > DateTime.MinValue
                && AddedAt > DateTime.MinValue;
        }

        /// <summary>
        /// Crée une copie du produit
        /// </summary>
        public ProductInfos Clone()
        {
            return new ProductInfos
            {
                Id = this.Id,
                Barcode = this.Barcode,
                Name = this.Name,
                UrlImage = this.UrlImage,
                Category = this.Category,
                Conservation = this.Conservation,
                AddedAt = this.AddedAt,
                Dlc = this.Dlc,
                Quantity = this.Quantity,
                ProductUniqueId = this.ProductUniqueId
            };
        }
    }
}