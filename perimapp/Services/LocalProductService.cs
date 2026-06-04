using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using perimapp.Models;

namespace perimapp.Services
{
    public class LocalProductService
    {
        private readonly string _filePath;

        public LocalProductService()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "products.json");
        }

        public async Task<List<ProductInfos>> LoadProductsAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<ProductInfos>();

                using var stream = File.OpenRead(_filePath);
                var products = await JsonSerializer.DeserializeAsync<List<ProductInfos>>(stream);

                return products ?? new List<ProductInfos>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalProductService] Erreur lecture JSON : {ex.Message}");
                return new List<ProductInfos>();
            }
        }

        public async Task SaveProductsAsync(List<ProductInfos> products)
        {
            try
            {
                using var stream = File.Create(_filePath);
                await JsonSerializer.SerializeAsync(stream, products, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalProductService] Erreur écriture JSON : {ex.Message}");
            }
        }

        public async Task<List<ProductInfos>> GetPendingSyncProductsAsync()
        {
            var products = await LoadProductsAsync();
            return products.Where(p => p.SyncState != SyncState.Synced).ToList();
        }

        public async Task HardDeleteProductAsync(ProductInfos productToHardDelete)
        {
            var products = await LoadProductsAsync();
            products.RemoveAll(p => p.ProductUniqueId == productToHardDelete.ProductUniqueId);
            await SaveProductsAsync(products);
        }

        public async Task AddProductAsync(ProductInfos product)
        {
            var products = await LoadProductsAsync();

            if (!products.Exists(p => p.ProductUniqueId == product.ProductUniqueId))
            {
                product.SyncState = SyncState.PendingCreate;
                product.LastModified = DateTime.UtcNow;

                products.Add(product);
                await SaveProductsAsync(products);
            }
        }

        public async Task UpdateProductLocalAsync(ProductInfos updatedProduct)
        {
            var products = await LoadProductsAsync();
            var index = products.FindIndex(p => p.ProductUniqueId == updatedProduct.ProductUniqueId);

            if (index >= 0)
            {
                updatedProduct.LastModified = DateTime.UtcNow;

                if (updatedProduct.SyncState != SyncState.PendingCreate)
                {
                    updatedProduct.SyncState = SyncState.PendingUpdate;
                }

                products[index] = updatedProduct;
                await SaveProductsAsync(products);
            }
        }

        public async Task<bool> UpdateProductStateAsync(string productUniqueId, string newState)
        {
            try
            {
                var products = await LoadProductsAsync();
                var product = products.FirstOrDefault(p => p.ProductUniqueId == productUniqueId);

                if (product == null) return false;

                product.State = newState;
                product.LastModified = DateTime.UtcNow;
                product.DeletedAt = (newState == "Deleted") ? DateTime.UtcNow : null;

                if (product.SyncState != SyncState.PendingCreate)
                {
                    product.SyncState = SyncState.PendingUpdate;
                }

                await SaveProductsAsync(products);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalProductService] Erreur mise à jour état : {ex.Message}");
                return false;
            }
        }

        public async Task EmptyTrashLocallyAsync()
        {
            try
            {
                var products = await LoadProductsAsync();
                bool hasChanges = false;

                var itemsToTrash = products.Where(p => p.State == "Deleted").ToList();

                foreach (var product in itemsToTrash)
                {
                    if (product.SyncState == SyncState.PendingCreate)
                    {
                        products.Remove(product);
                    }
                    else
                    {
                        product.State = "HardDeleted";
                        product.SyncState = SyncState.PendingDelete;
                        product.LastModified = DateTime.UtcNow;
                    }
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await SaveProductsAsync(products);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalProductService] Erreur vidage corbeille : {ex.Message}");
            }
        }

        public async Task SaveCustomProductNameAsync(long barcode, string homeCode, string customName)
        {
            try
            {
                string customNamesPath = Path.Combine(FileSystem.AppDataDirectory, "custom_names.json"));
                var customNames = new Dictionary<string, string>();

                if (File.Exists(customNamesPath))
                {
                    using var stream = File.OpenRead(customNamesPath);
                    customNames = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream) ?? new Dictionary<string, string>();
                }

                string key = $"{barcode}_{homeCode}";
                customNames[key] = customName;

                using var writeStream = File.Create(customNamesPath);
                await JsonSerializer.SerializeAsync(writeStream, customNames, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalProductService] Erreur sauvegarde nom : {ex.Message}");
            }
        }

        public async Task<string?> GetCustomProductNameAsync(long barcode, string homeCode)
        {
            try
            {
                string customNamesPath = Path.Combine(FileSystem.AppDataDirectory, "custom_names.json");
                if (!File.Exists(customNamesPath)) return null;

                using var stream = File.OpenRead(customNamesPath);
                var customNames = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream);

                if (customNames == null) return null;

                string key = $"{barcode}_{homeCode}";
                return customNames.TryGetValue(key, out string? customName) ? customName : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void ClearAllProducts()
        {
            try
            {
                if (File.Exists(_filePath)) File.Delete(_filePath);

                string customNamesPath = Path.Combine(FileSystem.AppDataDirectory, "custom_names.json");
                if (File.Exists(customNamesPath)) File.Delete(customNamesPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalProductService] Erreur suppression des données : {ex.Message}");
            }
        }
    }
}