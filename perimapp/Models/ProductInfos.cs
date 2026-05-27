using SQLite;
using System;
using System.Text.Json.Serialization;

namespace perimapp.Models
{
    public class ProductInfos
    {
        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

        [JsonPropertyName("id")]
        public int ProductId { get; set; }

        [JsonPropertyName("productUniqueId")]
        public string ProductUniqueId { get; set; } = Guid.NewGuid().ToString();

        [JsonIgnore]
        public SyncState SyncState { get; set; } = SyncState.PendingCreate;

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        [Indexed]
        [JsonPropertyName("barcode")]
        public long Barcode { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("urlImage")]
        public string UrlImage { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("conservation")]
        public string Conservation { get; set; }

        [JsonPropertyName("addedAt")]
        public DateTime AddedAt { get; set; }

        [JsonPropertyName("dlc")]
        public DateTime Dlc { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("deletedAt")]
        public DateTime? DeletedAt { get; set; }

        [JsonPropertyName("expiredAt")]
        public DateTime? ExpiredAt { get; set; }

        [JsonPropertyName("customName")]
        public string? CustomName { get; set; }

        [JsonPropertyName("homeCode")]
        public string? HomeCode { get; set; }

        [Ignore, JsonIgnore]
        public string DisplayName => !string.IsNullOrWhiteSpace(CustomName) ? CustomName : Name;

        [Ignore, JsonIgnore]
        public int DaysRemaining => (Dlc.Date - DateTime.Today).Days;

        [Ignore, JsonIgnore]
        public string DaysRemainingTextMainView
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

        [Ignore, JsonIgnore]
        public double DaysRemainingFontSize => (DaysRemaining >= 1000 && DaysRemaining <= 9999) ? 20 : 24;

        [Ignore, JsonIgnore]
        public string DaysRemainingTextDetailsView
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
    }
}

//Variable, Type, Qui le génère ?, À quoi ça sert ?
//Id,int, Le téléphone (SQLite),"C'est l'ordre d'arrivée dans la base locale du téléphone (1, 2, 3...). Il ne veut absolument rien dire pour le serveur ou pour le téléphone de ta copine."
//ProductId,int, Le serveur (PostgreSQL),"C'est l'ID officiel dans la base de données de l'API. Il est indispensable pour dire au serveur : ""Mets à jour le produit n°45""."
//ProductUniqueId,string (Guid), Le téléphone, Le sauveur du hors-ligne. Une suite de caractères unique au monde (ex: 123e4567 - e89b - 12d3...).