using System.Text.Json.Serialization;

namespace perimapp.Models
{
    public class UserProfileDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("homeCode")]
        public string HomeCode { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("lostProductCount")]
        public int LostProductCount { get; set; }

        [JsonPropertyName("isValidated")]
        public bool IsValidated { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        [JsonIgnore]
        public int RegisteredProductsCount { get; set; }

        [JsonIgnore]
        public string FirstName { get; set; }

        [JsonIgnore]
        public string LastName { get; set; }
    }
}