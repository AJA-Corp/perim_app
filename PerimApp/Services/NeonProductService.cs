using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using perimapp.Models;

namespace perimapp.Services
{
    public class NeonProductService
    {
        private const string ConnectionString = "Host=ep-little-bread-abqvwscs-pooler.eu-west-2.aws.neon.tech;Username=perimapp_owner;Password=npg_5KTFGrlNZ0Ao;Database=perimapp;SSL Mode=Require;Trust Server Certificate=true";

        public async Task<List<ProductInfos>> GetUserProductsAsync(int userId)
        {
            var products = new List<ProductInfos>();

            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                string query = @"
                    SELECT pu.id, pu.barcode, pd.name, pd.url_image, pd.category, pd.conservation,
                           pu.dlc, pu.quantity
                    FROM product_users pu
                    JOIN products_data pd ON pu.barcode = pd.barcode
                    WHERE pu.user_id = @userId;
                ";

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
                        Quantity = reader.GetInt32(7)
                    };

                    products.Add(product);
                }
                
                await conn.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur Neon: {ex.Message}");
            }

            return products;
        }
    }
}
