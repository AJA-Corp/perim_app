using SQLite;
using System;

namespace perimapp.Models
{
    public class ProductInfos
    {
        [PrimaryKey, AutoIncrement]   // clé primaire pour SQLite
        public int Id { get; set; }

        [Indexed]  //Pour rechercher rapidement par code-barres
        public long Barcode { get; set; }

        public string Name { get; set; }
        public string UrlImage { get; set; }
        public string Category { get; set; }
        public string Conservation { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime Dlc { get; set; }
        public int Quantity { get; set; }
        public string State { get; set; }
        public DateTime? DeletedAt { get; set; } // Utilisé pour l'étape de purge (48h)
        public DateTime? ExpiredAt { get; set; } // Utilisé pour l'étape 6 (état expired)

        // Custom name support for home code groups
        public string? CustomName { get; set; }
        public int? HomeCode { get; set; }

        // pas stocké sur la DB
        
        // Property to get the display name (custom name if available, otherwise original name)
        [Ignore] // Not stored in SQLite
        public string DisplayName => !string.IsNullOrWhiteSpace(CustomName) ? CustomName : Name;
        
        public int DaysRemaining => (Dlc.Date - DateTime.Today).Days;

        
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
        
        public double DaysRemainingFontSize
        {
            get
            {
                // La propriété DaysRemaining est déjà disponible
                if (DaysRemaining >= 1000 && DaysRemaining <= 9999)
                {
                    return 20; // 4 chiffres, on réduit la taille
                }
        
                return 24; // Taille de police par défaut
            }
        }

        
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

         // pas stocké sur DB
         //stockage des Produits en ligne neon db
        public string ProductUniqueId { get; set; } = Guid.NewGuid().ToString();
        
        //en local avec SQlite
        public int ProductId { get; set; } 
    }
}
