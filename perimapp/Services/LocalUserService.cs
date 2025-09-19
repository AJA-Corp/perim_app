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

        // Sauvegarde le profil utilisateur localement.
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

        // Charge le profil utilisateur depuis le stockage local.
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

        
        // Supprime le profil utilisateur local.
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
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
