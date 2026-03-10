using System.Text.Json.Serialization;

namespace perimapp.Models
{
    public class UserProfileDetails
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("home_code")]
        public int HomeCode { get; set; }

        [JsonPropertyName("lost_product_count")]
        public int LostProductCount { get; set; }

        // AJOUT DE CETTE PROPRIÉTÉ
        [JsonIgnore] // Cet attribut est utile : il indique au sérialiseur JSON d'ignorer ce champ.
        public int RegisteredProductsCount { get; set; }
    }
}