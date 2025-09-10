using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using perimapp.Models;
using System.Threading;

namespace perimapp.Services
{
    public class NeonProductService
    {
        private const string ConnectionString =
            "Host=ep-little-bread-abqvwscs-pooler.eu-west-2.aws.neon.tech;Username=perimapp_owner;Password=npg_5KTFGrlNZ0Ao;Database=perimapp;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;MinPoolSize=1;MaxPoolSize=10;Connection Idle Lifetime=300";
        
        private static readonly SemaphoreSlim ConnectionSemaphore = new(5, 5); // Limit concurrent connections

        public async Task<List<ProductInfos>> GetUserProductsAsync(int userId)
        {
            var products = new List<ProductInfos>();

            // Use semaphore to limit concurrent connections
            await ConnectionSemaphore.WaitAsync();
            
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                string query =
                    @"
                    SELECT pu.id, pu.barcode, pd.name, pd.url_image, pd.category, conservation,
                           pu.dlc, pu.quantity, pu.added_at
                    FROM products_users pu
                    JOIN products_data pd ON pu.barcode = pd.barcode
                    WHERE pu.user_id = @userId
                    ORDER BY pu.dlc ASC;
                ";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("userId", userId);
                
                // Set timeout for better performance on slow connections
                cmd.CommandTimeout = 30;

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
                Console.WriteLine($"[DEBUG] - Erreur Neon: {ex.Message}");
                throw; // Re-throw to allow caller to handle properly
            }
            finally
            {
                ConnectionSemaphore.Release();
            }

            return products;
        }

        public async Task<ProductInfos?> GetProductDataAsync(long barcode)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                string query =
                    @"
            SELECT barcode, name, url_image, category, conservation
            FROM products_data
            WHERE barcode = @barcode;
        ";

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
                Console.WriteLine(
                    $"[DEBUG] - Erreur lors de la récupération du produit : {ex.Message}"
                );
            }

            return null;
        }

        public async Task<bool> AddProductDataAsync(ProductInfos product)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                string query =
                    @"
            INSERT INTO products_data (barcode, name, url_image, category, conservation)
            VALUES (@Barcode, @Name, @UrlImage, @Category, @Conservation);
        ";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("Barcode", product.Barcode);
                cmd.Parameters.AddWithValue("Name", product.Name ?? "");
                cmd.Parameters.AddWithValue("UrlImage", product.UrlImage ?? "");
                cmd.Parameters.AddWithValue("Category", product.Category ?? "");
                cmd.Parameters.AddWithValue("Conservation", product.Conservation ?? "");

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[DEBUG] - Erreur lors de l'insertion dans products_data : {ex.Message}"
                );
                return false;
            }
        }

        public async Task<bool> AddUserProductAsync(ProductInfos product, int userId)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                string query =
                    @"
            INSERT INTO products_users (user_id, barcode, dlc, quantity, added_at)
            VALUES (@UserId, @Barcode, @Dlc, @Quantity, @AddedAt);
        ";

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
                Console.WriteLine(
                    $"[DEBUG] - Erreur lors de l'ajout du produit utilisateur : {ex.Message}"
                );
                return false;
            }
        }

        public async Task<bool> UpdateUserProductAsync(ProductInfos product)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                string query =
                    @"
                    UPDATE products_users
                    SET dlc = @dlc, quantity = @quantity
                    WHERE id = @id;
                ";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("dlc", product.Dlc);
                cmd.Parameters.AddWithValue("quantity", product.Quantity);
                cmd.Parameters.AddWithValue("id", product.Id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour : {ex.Message}");
                return false;
            }
        }
    }
}
