using System;
using System.Threading.Tasks;
using Npgsql;
using PerimApp.Core.Models;

namespace PerimApp.Core.Services
{
    /// <summary>
    /// Interface pour le service de gestion des utilisateurs
    /// </summary>
    public interface IUserService
    {
        Task<int> RegisterUserAsync(UserProfileDetails user);
        Task<UserProfileDetails?> LoginUserAsync(string email, string password);
        Task<bool> UpdateUserAsync(UserProfileDetails user);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> HomeCodeExistsAsync(int homeCode);
    }

    /// <summary>
    /// Service de gestion des utilisateurs utilisant Neon PostgreSQL
    /// </summary>
    public class NeonUserService : IUserService
    {
        private readonly string _connectionString;

        public NeonUserService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur
        /// </summary>
        public async Task<int> RegisterUserAsync(UserProfileDetails user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (!user.IsValid()) throw new ArgumentException("Les données utilisateur ne sont pas valides");

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                // Génération d'un code unique à 6 chiffres
                int uniqueCode;
                do
                {
                    uniqueCode = new Random().Next(100000, 999999);
                } while (await HomeCodeExistsAsync(uniqueCode, conn));

                user.HomeCode = uniqueCode;

                string insertQuery = @"
                    INSERT INTO users (email, password, home_code)
                    VALUES (@Email, @Password, @HomeCode)
                    RETURNING id;";

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
                return -2; // Code spécifique pour email déjà existant
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de l'enregistrement : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Authentifie un utilisateur
        /// </summary>
        public async Task<UserProfileDetails?> LoginUserAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    SELECT id, email, password, home_code
                    FROM users
                    WHERE email = @Email;";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("Email", email);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string storedPassword = reader.GetString(2);
                    if (PasswordHasher.VerifyPassword(password, storedPassword))
                    {
                        return new UserProfileDetails
                        {
                            Email = reader.GetString(1),
                            HomeCode = reader.GetInt32(3)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de la connexion : {ex.Message}");
                throw;
            }

            return null;
        }

        /// <summary>
        /// Met à jour les informations d'un utilisateur
        /// </summary>
        public async Task<bool> UpdateUserAsync(UserProfileDetails user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"
                    UPDATE users 
                    SET email = @Email, password = @Password
                    WHERE home_code = @HomeCode;";

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("Email", user.Email);
                cmd.Parameters.AddWithValue("Password", PasswordHasher.HashPassword(user.Password));
                cmd.Parameters.AddWithValue("HomeCode", user.HomeCode);

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
        /// Vérifie si un email existe déjà
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = "SELECT COUNT(1) FROM users WHERE email = @Email;";
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("Email", email);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de la vérification de l'email : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Vérifie si un code famille existe déjà
        /// </summary>
        public async Task<bool> HomeCodeExistsAsync(int homeCode)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            return await HomeCodeExistsAsync(homeCode, conn);
        }

        /// <summary>
        /// Vérifie si un code famille existe déjà (avec connexion existante)
        /// </summary>
        private async Task<bool> HomeCodeExistsAsync(int homeCode, NpgsqlConnection conn)
        {
            try
            {
                string query = "SELECT COUNT(1) FROM users WHERE home_code = @HomeCode;";
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("HomeCode", homeCode);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] - Erreur lors de la vérification du code famille : {ex.Message}");
                throw;
            }
        }
    }
}