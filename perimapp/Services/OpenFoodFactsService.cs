using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using perimapp.Models;

namespace perimapp.Services
{
    public class OpenFoodFactsService
    {
        private readonly HttpClient _httpClient;

        public OpenFoodFactsService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public virtual async Task<ProductInfos?> GetProductFromApiAsync(long barcode)
        {
            try
            {
                string url = $"https://world.openfoodfacts.net/api/v2/product/{barcode}.json";
                var jsonDoc = await _httpClient.GetFromJsonAsync<JsonDocument>(url);

                if (jsonDoc is null)
                    return null;

                var productNode = jsonDoc.RootElement.GetProperty("product");

                return new ProductInfos
                {
                    Barcode = barcode,
                    Name = productNode.GetProperty("product_name").GetString() ?? "",
                    UrlImage = productNode.TryGetProperty("image_url", out var img)
                        ? img.GetString() ?? ""
                        : "",
                    Category = productNode.TryGetProperty("categories", out var cat)
                        ? cat.GetString() ?? ""
                        : "",
                    Conservation = "",
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
