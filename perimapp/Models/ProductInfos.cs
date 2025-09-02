// Models/ProductMainPage.cs
/*
using System;
using System.Text.Json.Serialization; // Ajouté pour JsonPropertyName et JsonIgnore

namespace perimapp.Models
{
    public class ProductInfos
    {
        // Rend les propriétés 'string' nullables pour éviter l'avertissement CS8618
        [JsonPropertyName("url_image")]
        public string? url_image { get; set; }

        [JsonPropertyName("product_name")]
        public string? product_name { get; set; }

        [JsonPropertyName("product_quantity")]
        public string? product_quantity { get; set; }

        [JsonPropertyName("product_dlc")]
        public DateTime product_dlc { get; set; }

        [JsonPropertyName("product_category")] // Nouveau champ du JSON
        public string? product_category { get; set; }

        [JsonPropertyName("product_barcode")] // Nouveau champ du JSON
        public string? product_barcode { get; set; }

        [JsonPropertyName("added_at")] // Nouveau champ du JSON
        public DateTime added_at { get; set; }

        // Propriété calculée pour les jours restants
        [JsonIgnore] // Ne pas inclure lors de la sérialisation/désérialisation du JSON
        public int DaysRemaining => (int)(product_dlc.Date - DateTime.Today.Date).TotalDays;

        // Propriété calculée pour le texte des jours restants
        [JsonIgnore] // Ne pas inclure lors de la sérialisation/désérialisation du JSON
        public string DaysRemainingTextMainPage
        {
            get
            {
                int days = DaysRemaining;
                if (days < 0) return "Exp.";
                if (days == 0) return "Auj.";
                if (days == 1) return "1j"; // Modifié de "1 jour" à "1j" pour correspondre au format "Xj"
                return $"{days}j";
            }
        }
        
        public string DaysRemainingTextDetailsPage
        {
            get
            {
                int days = DaysRemaining;
                if (days < 0) return "Expiré";
                if (days == 0) return "Aujourd'hui";
                if (days == 1) return "1 jour"; // Modifié de "1 jour" à "1j" pour correspondre au format "Xj"
                return $"{days} jours";
            }
        }

        // Ajout d'une propriété d'ID pour la navigation, basée sur le code-barres
        // Ceci remplace la nécessité d'un champ "id" explicite dans votre JSON
        [JsonIgnore] // Ne pas inclure lors de la sérialisation/désérialisation du JSON
        public string ProductUniqueId => product_barcode ?? Guid.NewGuid().ToString(); // Utilise le code-barres comme ID unique. Si null, génère un GUID.
    }
}
*/

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

        // pas stocké sur la DB
        
        public int DaysRemaining => (Dlc - DateTime.Today).Days;

        
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
        //public int ProductId { get; set; } 
    }
}
