using System;
using System.Threading.Tasks;
using Npgsql;
using perimapp.Models;

namespace perimapp.Services
{
    public class NeonUserService
    {
        private const string ConnectionString =
            "Host=ep-little-bread-abqvwscs-pooler.eu-west-2.aws.neon.tech;Username=perimapp_owner;Password=npg_5KTFGrlNZ0Ao;Database=perimapp;SSL Mode=Require;Trust Server Certificate=true";

        /*public async Task<int> RegisterUserAsync(UserProfile user)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                string insertQuery =
                    @"
                    INSERT INTO users (email, password, home_code)
                    VALUES (@Email, @Password, @HomeCode)
                    RETURNING id;
                ";

                await using var cmd = new NpgsqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("Email", user.Email);
                cmd.Parameters.AddWithValue("Password", PasswordHasher.HashPassword(user.Password));
                cmd.Parameters.AddWithValue("HomeCode", user.HomeCode); // Générer un homecode de 6 digits

                object? result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : -1;
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "23505") // Unique violation
            {
                Console.WriteLine("Email déjà utilisé.");
                return -2;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'inscription : {ex.Message}");
                return -1;
            }
        }*/

        public async Task<int> RegisterUserAsync(UserProfile user)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                // Génération d'un code unique à 6 chiffres
                int uniqueCode;
                do
                {
                    uniqueCode = new Random().Next(100000, 999999);
                } while (await HomeCodeExistsAsync(uniqueCode, conn));

                user.HomeCode = uniqueCode;

                string insertQuery =
                    @"
            INSERT INTO users (email, password, home_code)
            VALUES (@Email, @Password, @HomeCode)
            RETURNING id;
        ";

                await using var cmd = new NpgsqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("Email", user.Email);
                cmd.Parameters.AddWithValue("Password", PasswordHasher.HashPassword(user.Password));
                cmd.Parameters.AddWithValue("HomeCode", user.HomeCode);

                object? result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : -1;
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "23505") // unique violation
            {
                Console.WriteLine("Email déjà utilisé.");
                return -2;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'inscription : {ex.Message}");
                return -1;
            }
        }

        // 🔍 Méthode privée pour vérifier l'existence du code foyer
        private async Task<bool> HomeCodeExistsAsync(int code, NpgsqlConnection conn)
        {
            const string checkQuery = "SELECT COUNT(*) FROM users WHERE home_code = @code";

            await using var cmd = new NpgsqlCommand(checkQuery, conn);
            cmd.Parameters.AddWithValue("code", code);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<int> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                string query =
                    @"
            SELECT id, password
            FROM users
            WHERE email = @Email;
        ";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("Email", email);

                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    int userId = reader.GetInt32(0);
                    string hashedPassword = reader.GetString(1);

                    bool isPasswordValid = PasswordHasher.VerifyPassword(hashedPassword, password);
                    return isPasswordValid ? userId : -1;
                }
                Console.WriteLine("[DEBUG]: Email non trouvé ou mot de passe incorrect");
                return -1; // Email non trouvé
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la connexion : {ex.Message}");
                return -1;
            }
        }
    }
}
