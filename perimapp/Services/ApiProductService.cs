using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using perimapp.Models;

namespace perimapp.Services
{
    public class ApiProductService
    {
        private readonly HttpClient _httpClient;

        private const string BaseApiUrl = "https://perimapp-web-api.onrender.com/api/";

        public ApiProductService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseApiUrl);
        }

        // --- CREATE (ApiProductService) START ---
        public async Task<bool> CreateProductAsync(ProductInfos product)
        {
            // Create = POST Inventory/add
            return await AddProductToInventoryAsync(product);
        }
        // --- CREATE (ApiProductService) END ---

        // --- READ (ApiProductService) START ---
        public async Task<ProductInfos?> ReadProductAsync(string productUniqueId)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();
                var response = await _httpClient.GetAsync($"Inventory/{productUniqueId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProductInfos>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiProductService] Erreur ReadProduct : {ex.Message}");
                return null;
            }
        }
        // --- READ (ApiProductService) END ---

        // --- UPDATE (ApiProductService) START ---
        public async Task<bool> UpdateProductAsync(string productUniqueId, ProductInfos updatedProduct)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();
                var response = await _httpClient.PutAsJsonAsync($"Inventory/{productUniqueId}", updatedProduct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiProductService] Erreur UpdateProduct : {ex.Message}");
                return false;
            }
        }
        // --- UPDATE (ApiProductService) END ---

        // --- DELETE (ApiProductService) START ---
        public async Task<bool> DeleteProductAsync(string productUniqueId)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();
                var response = await _httpClient.DeleteAsync($"Inventory/{productUniqueId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiProductService] Erreur DeleteProduct : {ex.Message}");
                return false;
            }
        }
        // --- DELETE (ApiProductService) END ---

        private async Task AttachAuthenticationHeaderAsync()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<ProductInfos?> SearchProductAsync(long barcode)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();

                var response = await _httpClient.GetAsync($"Products/search/{barcode}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProductInfos>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiProductService] Erreur SearchProduct : {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AddProductToInventoryAsync(ProductInfos product)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();

                var response = await _httpClient.PostAsJsonAsync("Inventory/add", product);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiProductService] Erreur AddProductToInventory : {ex.Message}");
                return false;
            }
        }

        public async Task<List<ProductInfos>> GetMyInventoryAsync()
        {
            try
            {
                await AttachAuthenticationHeaderAsync();

                var response = await _httpClient.GetAsync("Inventory/my-inventory");

                if (response.IsSuccessStatusCode)
                {
                    var products = await response.Content.ReadFromJsonAsync<List<ProductInfos>>();
                    return products ?? new List<ProductInfos>();
                }

                return new List<ProductInfos>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiProductService] Erreur GetMyInventory : {ex.Message}");
                return new List<ProductInfos>();
            }
        }

        public async Task<bool> UpdateProductStateAsync(string productUniqueId, string newState)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();
                var response = await _httpClient.PutAsJsonAsync($"Inventory/state/{productUniqueId}", newState);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiProductService] Erreur UpdateState : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SyncOfflineProductsAsync(List<ProductInfos> pendingProducts)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();
                var response = await _httpClient.PostAsJsonAsync("Inventory/sync", pendingProducts);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiProductService] Erreur SyncOffline : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EmptyTrashOnlineAsync()
        {
            try
            {
                await AttachAuthenticationHeaderAsync();
                var response = await _httpClient.DeleteAsync("Inventory/trash/empty");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiProductService] Erreur EmptyTrash : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetCustomProductNameAsync(long barcode, string homeCode, string customName)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();

                var customNameObj = new
                {
                    barcode = barcode,
                    homeCode = homeCode,
                    customName = customName
                };

                var response = await _httpClient.PostAsJsonAsync("CustomProductNames", customNameObj);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiProductService] Erreur SetCustomName : {ex.Message}");
                return false;
            }
        }

        public async Task<string?> GetCustomProductNameAsync(long barcode, string homeCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/CustomProductNames/{barcode}/{homeCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result.Trim('\"');
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Erreur API CustomName : {ex.Message}");
            }

            return null;
        }
    }
}