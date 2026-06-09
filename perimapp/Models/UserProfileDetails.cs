using System.Text.Json.Serialization;

namespace perimapp.Models
{
    public class UserProfileDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("homeCode")]
        public required string HomeCode { get; set; }

        [JsonPropertyName("email")]
        public required string Email { get; set; }

        [JsonPropertyName("lostProductCount")]
        public int LostProductCount { get; set; }

        [JsonPropertyName("isValidated")]
        public bool IsValidated { get; set; }

        [JsonIgnore]
        public required string Password { get; set; }

        [JsonPropertyName("registeredProductsCount")]
        public int RegisteredProductsCount { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
    }
}