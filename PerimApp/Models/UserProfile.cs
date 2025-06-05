using System.Text.Json.Serialization;

namespace perimapp.Models
{
    public class UserProfile
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
        public string HomeCode { get; set; }

        [JsonPropertyName("lost_products")]
        public string LostProducts { get; set; } 
    }
}