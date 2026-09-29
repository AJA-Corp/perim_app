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
        private const string BaseApiUrl = "https://perimapp-web-api.onrender.com/api/";

        private async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var client = new HttpClient { BaseAddress = new Uri(BaseApiUrl) };

            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<ProductInfos?> SearchProductAsync(long barcode)
        {
            try
            {
                using var client = await CreateAuthenticatedClientAsync();

                var response = await client.GetAsync($"Products/search/{barcode}");

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
                using var client = await CreateAuthenticatedClientAsync();

                var response = await client.PostAsJsonAsync("Inventory/add", product);

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
                using var client = await CreateAuthenticatedClientAsync();

                var response = await client.GetAsync("Inventory/my-inventory");

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
                using var client = await CreateAuthenticatedClientAsync();
                var response = await client.PutAsJsonAsync($"Inventory/state/{productUniqueId}", newState);
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
                using var client = await CreateAuthenticatedClientAsync();
                var response = await client.PostAsJsonAsync("Inventory/sync", pendingProducts);
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
                using var client = await CreateAuthenticatedClientAsync();
                var response = await client.DeleteAsync("Inventory/trash/empty");
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
                using var client = await CreateAuthenticatedClientAsync();

                var customNameObj = new
                {
                    barcode = barcode,
                    homeCode = homeCode,
                    customName = customName
                };

                var response = await client.PostAsJsonAsync("CustomProductNames", customNameObj);
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
                using var client = new HttpClient { BaseAddress = new Uri(BaseApiUrl) };

                var response = await client.GetAsync($"api/CustomProductNames/{barcode}/{homeCode}");

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