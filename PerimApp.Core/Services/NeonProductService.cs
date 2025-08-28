using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using PerimApp.Core.Models;

namespace PerimApp.Core.Services
{
    /// <summary>
    /// Interface pour le service de gestion des produits
    /// </summary>
    public interface IProductService
    {
        Task<List<ProductInfos>> GetUserProductsAsync(int userId);
        Task<ProductInfos?> GetProductDataAsync(long barcode);
        Task<bool> AddProductDataAsync(ProductInfos product);
        Task<bool> AddUserProductAsync(ProductInfos product, int userId);
        Task<bool> UpdateUserProductAsync(ProductInfos product);
        Task<bool> DeleteUserProductAsync(int productId);
        Task<List<ProductInfos>> GetExpiringProductsAsync(int userId, int daysThreshold = 3);
    }

    /// <summary>
    /// Service de gestion des produits utilisant Neon PostgreSQL
    /// </summary>
    public class NeonProductService : IProductService
    {
        private readonly string _connectionString;

        public NeonProductService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Récupère tous les produits d'un utilisateur
        /// </summary>
        public async Task<List<ProductInfos>> GetUserProductsAsync(int userId)
        {
            var products = new List<ProductInfos>();

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    SELECT pu.id, pu.barcode, pd.name, pd.url_image, pd.category, conservation,
                           pu.dlc, pu.quantity, pu.added_at
                    FROM products_users pu
                    JOIN products_data pd ON pu.barcode = pd.barcode
                    WHERE pu.user_id = @userId
                    ORDER BY pu.dlc ASC;";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("userId", userId);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var product = new ProductInfos
                    {
                        Id = reader.GetInt32(0),
                        Barcode = reader.GetInt64(1),
                        Name = reader.GetString(2),
                        UrlImage = reader.GetString(3),
                        Category = reader.GetString(4),
                        Conservation = reader.GetString(5),
                        Dlc = reader.GetDateTime(6),
                        Quantity = reader.GetInt32(7),
                        AddedAt = reader.GetDateTime(8),
                    };

                    products.Add(product);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de la récupération des produits: {ex.Message}");
                throw;
            }

            return products;
        }

        /// <summary>
        /// Récupère les données d'un produit par son code-barres
        /// </summary>
        public async Task<ProductInfos?> GetProductDataAsync(long barcode)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    SELECT barcode, name, url_image, category, conservation
                    FROM products_data
                    WHERE barcode = @barcode;";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("barcode", barcode);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new ProductInfos
                    {
                        Barcode = reader.GetInt64(0),
                        Name = reader.GetString(1),
                        UrlImage = reader.GetString(2),
                        Category = reader.GetString(3),
                        Conservation = reader.GetString(4),
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de la récupération du produit : {ex.Message}");
                throw;
            }

            return null;
        }

        /// <summary>
        /// Ajoute un nouveau produit dans la base de données
        /// </summary>
        public async Task<bool> AddProductDataAsync(ProductInfos product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    INSERT INTO products_data (barcode, name, url_image, category, conservation)
                    VALUES (@Barcode, @Name, @UrlImage, @Category, @Conservation)
                    ON CONFLICT (barcode) DO NOTHING;";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("Barcode", product.Barcode);
                cmd.Parameters.AddWithValue("Name", product.Name ?? "");
                cmd.Parameters.AddWithValue("UrlImage", product.UrlImage ?? "");
                cmd.Parameters.AddWithValue("Category", product.Category ?? "");
                cmd.Parameters.AddWithValue("Conservation", product.Conservation ?? "");

                return await cmd.ExecuteNonQueryAsync() >= 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de l'insertion dans products_data : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ajoute un produit à la liste d'un utilisateur
        /// </summary>
        public async Task<bool> AddUserProductAsync(ProductInfos product, int userId)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    INSERT INTO products_users (user_id, barcode, dlc, quantity, added_at)
                    VALUES (@UserId, @Barcode, @Dlc, @Quantity, @AddedAt);";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("UserId", userId);
                cmd.Parameters.AddWithValue("Barcode", product.Barcode);
                cmd.Parameters.AddWithValue("Dlc", product.Dlc);
                cmd.Parameters.AddWithValue("Quantity", product.Quantity);
                cmd.Parameters.AddWithValue("AddedAt", product.AddedAt);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de l'ajout du produit utilisateur : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Met à jour un produit d'un utilisateur
        /// </summary>
        public async Task<bool> UpdateUserProductAsync(ProductInfos product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    UPDATE products_users
                    SET dlc = @dlc, quantity = @quantity
                    WHERE id = @id;";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("dlc", product.Dlc);
                cmd.Parameters.AddWithValue("quantity", product.Quantity);
                cmd.Parameters.AddWithValue("id", product.Id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de la mise à jour : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Supprime un produit d'un utilisateur
        /// </summary>
        public async Task<bool> DeleteUserProductAsync(int productId)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = "DELETE FROM products_users WHERE id = @id;";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("id", productId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de la suppression : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère les produits qui expirent bientôt
        /// </summary>
        public async Task<List<ProductInfos>> GetExpiringProductsAsync(int userId, int daysThreshold = 3)
        {
            var products = new List<ProductInfos>();

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    SELECT pu.id, pu.barcode, pd.name, pd.url_image, pd.category, conservation,
                           pu.dlc, pu.quantity, pu.added_at
                    FROM products_users pu
                    JOIN products_data pd ON pu.barcode = pd.barcode
                    WHERE pu.user_id = @userId 
                    AND pu.dlc <= @threshold
                    ORDER BY pu.dlc ASC;";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("userId", userId);
                cmd.Parameters.AddWithValue("threshold", DateTime.Today.AddDays(daysThreshold));

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var product = new ProductInfos
                    {
                        Id = reader.GetInt32(0),
                        Barcode = reader.GetInt64(1),
                        Name = reader.GetString(2),
                        UrlImage = reader.GetString(3),
                        Category = reader.GetString(4),
                        Conservation = reader.GetString(5),
                        Dlc = reader.GetDateTime(6),
                        Quantity = reader.GetInt32(7),
                        AddedAt = reader.GetDateTime(8),
                    };

                    products.Add(product);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de la récupération des produits expirants: {ex.Message}");
                throw;
            }

            return products;
        }
    }
}