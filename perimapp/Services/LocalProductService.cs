using System;
using System.Collections.Generic;
using System.IO;
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
            // Le fichier sera stocké dans le dossier local de l'app
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "products.json");
        }

        /// <summary>
        /// Charge les produits depuis le fichier local JSON
        /// </summary>
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

        /// <summary>
        /// Sauvegarde la liste complète des produits dans le fichier JSON local
        /// </summary>
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

        /// <summary>
        /// Ajoute un produit dans le fichier local
        /// </summary>
        public async Task AddProductAsync(ProductInfos product)
        {
            var products = await LoadProductsAsync();

            // Vérifie si le produit existe déjà pour éviter les doublons
            if (!products.Exists(p => p.ProductUniqueId == product.ProductUniqueId))
            {
                products.Add(product);
                await SaveProductsAsync(products);
            }
        }

        /// <summary>
        /// Supprime un produit par son ID unique (ProductUniqueId en string)
        /// </summary>
        public async Task RemoveProductAsync(string productUniqueId)
        {
            var products = await LoadProductsAsync();
            products.RemoveAll(p => p.ProductUniqueId == productUniqueId);
            await SaveProductsAsync(products);
        }

        /// <summary>
        /// Met à jour un produit existant
        /// </summary>
        public async Task UpdateProductAsync(ProductInfos updatedProduct)
        {
            var products = await LoadProductsAsync();
            var index = products.FindIndex(p => p.ProductUniqueId == updatedProduct.ProductUniqueId);
            if (index >= 0)
            {
                products[index] = updatedProduct;
                await SaveProductsAsync(products);
            }
        }
    }
}
