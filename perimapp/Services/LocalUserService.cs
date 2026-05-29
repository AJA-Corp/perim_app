using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using perimapp.Models;

namespace perimapp.Services
{
    public class LocalUserService
    {
        private readonly string _filePath;

        public LocalUserService()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "userProfile.json");
        }

        public async Task SaveUserAsync(UserProfileDetails user)
        {
            try
            {
                using var stream = File.Create(_filePath);
                await JsonSerializer.SerializeAsync(stream, user, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalUserService] Erreur écriture JSON : {ex.Message}");
            }
        }

        public async Task<UserProfileDetails?> LoadUserAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return null;

                using var stream = File.OpenRead(_filePath);
                var user = await JsonSerializer.DeserializeAsync<UserProfileDetails>(stream);
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalUserService] Erreur lecture JSON : {ex.Message}");
                return null;
            }
        }

        public void ClearUser()
        {
            try
            {
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalUserService] Erreur suppression JSON : {ex.Message}");
            }
        }

        public void DeleteUser()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IncrementLostProductCountAsync()
        {
            try
            {
                var user = await LoadUserAsync();
                if (user == null)
                {
                    Console.WriteLine("[LocalUserService] Impossible d'incrémenter: utilisateur non trouvé localement.");
                    return false;
                }

                user.LostProductCount++;
                await SaveUserAsync(user);
                Console.WriteLine($"[LocalUserService] lost_product_count incrémenté localement: {user.LostProductCount}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalUserService] Erreur lors de l'incrémentation: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DecrementLostProductCountAsync()
        {
            try
            {
                var user = await LoadUserAsync();
                if (user == null)
                {
                    Console.WriteLine("[LocalUserService] Impossible de décrémenter: utilisateur non trouvé localement.");
                    return false;
                }

                user.LostProductCount = Math.Max(0, user.LostProductCount - 1);
                await SaveUserAsync(user);
                Console.WriteLine($"[LocalUserService] lost_product_count décrémenté localement: {user.LostProductCount}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalUserService] Erreur lors de la décrémentation: {ex.Message}");
                return false;
            }
        }
    }
}
