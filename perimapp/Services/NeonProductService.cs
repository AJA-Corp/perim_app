using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using perimapp.Models;

namespace perimapp.Services
{
    public class NeonProductService
    {
        private const string ConnectionString =
            "Host=ep-little-bread-abqvwscs-pooler.eu-west-2.aws.neon.tech;Username=perimapp_owner;Password=npg_5KTFGrlNZ0Ao;Database=perimapp;SSL Mode=Require;Trust Server Certificate=true";

        // MODIFICATION : Utiliser ProductUniqueId comme clé de recherche, même si on ne le lit pas encore depuis la DB Neon
        public async Task<List<ProductInfos>> GetUserProductsAsync(int userId)
        {
            var products = new List<ProductInfos>();

            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                // First get the user's home code
                string homeCodeQuery = "SELECT home_code FROM users WHERE id = @userId";
                await using var homeCodeCmd = new NpgsqlCommand(homeCodeQuery, conn);
                homeCodeCmd.Parameters.AddWithValue("userId", userId);
                object? homeCodeResult = await homeCodeCmd.ExecuteScalarAsync();
                
                if (homeCodeResult == null)
                {
                    Console.WriteLine($"[DEBUG] - User {userId} not found");
                    return products;
                }

                int homeCode = Convert.ToInt32(homeCodeResult);

                // Check if custom_product_names table exists
                bool customNamesTableExists = await TableExistsAsync(conn, "custom_product_names");

                string query;
                if (customNamesTableExists)
                {
                    // query = @"
                    //     SELECT pu.id, pu.barcode, pd.name, pd.url_image, pd.category, conservation,
                    //            pu.dlc, pu.quantity, pu.added_at, cpn.custom_name
                    //     FROM products_users pu
                    //     JOIN products_data pd ON pu.barcode = pd.barcode
                    //     LEFT JOIN custom_product_names cpn ON pd.barcode = cpn.barcode AND cpn.home_code = @homeCode
                    //     WHERE pu.user_id = @userId;
                    // ";

                    query = @"
                            SELECT pu.id, pu.barcode, pd.name, pd.url_image, pd.category, pd.conservation,
                                   pu.dlc, pu.quantity, pu.added_at, pu.state, cpn.custom_name, pu.deleted_at
                            FROM products_users pu
                            JOIN products_data pd ON pu.barcode = pd.barcode
                            LEFT JOIN custom_product_names cpn ON pd.barcode = cpn.barcode AND cpn.home_code = @homeCode
                            WHERE pu.user_id = @userId;
                    ";
                }
                else
                {
                    query = @"
                        SELECT pu.id, pu.barcode, pd.name, pd.url_image, pd.category, conservation,
                               pu.dlc, pu.quantity, pu.added_at, pu.state, NULL as custom_name, pu.deleted_at
                        FROM products_users pu
                        JOIN products_data pd ON pu.barcode = pd.barcode
                        WHERE pu.user_id = @userId;
                    ";
                }

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("userId", userId);
                if (customNamesTableExists)
                {
                    cmd.Parameters.AddWithValue("homeCode", homeCode);
                }

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    // LECTURE SÉCURISÉE JUSQU'À L'INDEX 10 (custom_name)
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
                        State = reader.GetString(9),
                        CustomName = reader.IsDBNull(10) ? null : reader.GetString(10),
                        DeletedAt = reader.IsDBNull(11) ? null : reader.GetDateTime(11)
                    };

                    products.Add(product);
                }

                await conn.CloseAsync();
            }
            catch (Exception ex)
            {
                // Ceci est l'exception que vous avez attrapée précédemment
                Console.WriteLine($"[DEBUG] - Erreur Neon (GetUserProductsAsync) : {ex.Message}");
            }

            return products;
        }

        // MODIFICATION CRITIQUE : Utiliser l'ID entier (pu.id) pour la mise à jour si ProductUniqueId n'est pas fiable.
        public async Task<bool> UpdateProductStateAsync(string productUniqueId, string newState)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();
                
                // Si la colonne 'deleted_at' n'existe pas, la DB va générer une erreur.
                // Nous la laissons pour que le code soit prêt une fois la colonne ajoutée.
                string setDeletedAtClause = 
                    newState == "Deleted"
                    ? ", deleted_at = @deletedAtValue"
                    : ", deleted_at = NULL";

                string query;
                
                // Nous supposons que productUniqueId contient maintenant l'ID entier (`ProductDetail.Id.ToString()`).
                if (!int.TryParse(productUniqueId, out int intId))
                {
                    // Si ce n'est pas un entier (ni un GUID), c'est une erreur de formatage.
                    Console.WriteLine($"[NeonProductService] Erreur: ID de produit '{productUniqueId}' n'est pas un entier valide pour la suppression.");
                    return false;
                }
                
                // Utilisation de l'ID entier (pu.id) pour la mise à jour
                query =
                    $@"
                    UPDATE products_users
                    SET state = @state {setDeletedAtClause}
                    WHERE id = @intId; 
                ";
                
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("state", newState);
                cmd.Parameters.AddWithValue("intId", intId); // Utilisation de l'ID entier
                
                if (newState == "Deleted")
                {
                    cmd.Parameters.AddWithValue("deletedAtValue", DateTime.UtcNow);
                }
                
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour de l'état du produit : {ex.Message}");
                // Si la colonne 'deleted_at' n'existe pas, cette méthode échouera ici.
                return false;
            }
        }
        
        // --- Les autres méthodes ne sont pas modifiées ---

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

        public async Task<ProductInfos?> GetProductDataWithCustomNameAsync(long barcode, int homeCode)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                // Check if custom_product_names table exists
                bool customNamesTableExists = await TableExistsAsync(conn, "custom_product_names");

                string query;
                if (customNamesTableExists)
                {
                    query = @"
                SELECT pd.barcode, pd.name, pd.url_image, pd.category, pd.conservation, cpn.custom_name
                FROM products_data pd
                LEFT JOIN custom_product_names cpn ON pd.barcode = cpn.barcode AND cpn.home_code = @homeCode
                WHERE pd.barcode = @barcode;
            ";
                }
                else
                {
                    query = @"
                SELECT barcode, name, url_image, category, conservation, NULL as custom_name
                FROM products_data
                WHERE barcode = @barcode;
            ";
                }

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("barcode", barcode);
                if (customNamesTableExists)
                {
                    cmd.Parameters.AddWithValue("homeCode", homeCode);
                }

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
                        CustomName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        HomeCode = homeCode
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[DEBUG] - Erreur lors de la récupération du produit avec nom personnalisé : {ex.Message}"
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

        // Custom name management methods
        public async Task<string?> GetCustomProductNameAsync(long barcode, int homeCode)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                // Check if table exists first
                if (!await TableExistsAsync(conn, "custom_product_names"))
                {
                    Console.WriteLine("[DEBUG] - custom_product_names table does not exist");
                    return null;
                }

                string query = @"
                    SELECT custom_name
                    FROM custom_product_names
                    WHERE barcode = @barcode AND home_code = @homeCode;
                ";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("barcode", barcode);
                cmd.Parameters.AddWithValue("homeCode", homeCode);

                object? result = await cmd.ExecuteScalarAsync();
                return result?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération du nom personnalisé : {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SetCustomProductNameAsync(long barcode, int homeCode, string customName)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                // Check if table exists first
                if (!await TableExistsAsync(conn, "custom_product_names"))
                {
                    Console.WriteLine("[DEBUG] - custom_product_names table does not exist, skipping custom name save");
                    return false;
                }

                // First check if a custom name already exists
                string checkQuery = @"
                    SELECT COUNT(*) 
                    FROM custom_product_names 
                    WHERE barcode = @barcode AND home_code = @homeCode;
                ";

                await using var checkCmd = new NpgsqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("barcode", barcode);
                checkCmd.Parameters.AddWithValue("homeCode", homeCode);

                int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                string query;
                if (count > 0)
                {
                    // Update existing custom name
                    query = @"
                        UPDATE custom_product_names
                        SET custom_name = @customName, last_modified = @lastModified
                        WHERE barcode = @barcode AND home_code = @homeCode;
                    ";
                }
                else
                {
                    // Insert new custom name
                    query = @"
                        INSERT INTO custom_product_names (barcode, home_code, custom_name, last_modified)
                        VALUES (@barcode, @homeCode, @customName, @lastModified);
                    ";
                }

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("barcode", barcode);
                cmd.Parameters.AddWithValue("homeCode", homeCode);
                cmd.Parameters.AddWithValue("customName", customName);
                cmd.Parameters.AddWithValue("lastModified", DateTime.UtcNow);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la définition du nom personnalisé : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAllDeletedProductsAsync(int userId)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                string query =
                    @"DELETE FROM products_users
                      WHERE user_id = @userId
                      AND state = 'Deleted';";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("userId", userId);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur suppression définitive : {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Supprime définitivement de la base de données distante les produits de l'utilisateur qui sont marqués comme supprimés.
        /// </summary>
        public async Task<bool> EmptyTrashOnlineAsync(int userId)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                // On supprime physiquement les produits qui ont l'état 'Deleted' ou 'HardDeleted' pour cet utilisateur
                string query = @"
            DELETE FROM products_users 
            WHERE user_id = @userId AND (state = 'Deleted' OR state = 'HardDeleted');
        ";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("userId", userId);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NeonProductService] Erreur lors de la suppression définitive : {ex.Message}");
                return false;
            }
        }     

        private async Task<bool> TableExistsAsync(NpgsqlConnection conn, string tableName)
        {
            try
            {
                string query = @"
                    SELECT EXISTS (
                        SELECT 1 
                        FROM information_schema.tables 
                        WHERE table_name = @tableName
                    );
                ";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("tableName", tableName);

                object? result = await cmd.ExecuteScalarAsync();
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la vérification de l'existence de la table : {ex.Message}");
                return false;
            }
        }
    }
}