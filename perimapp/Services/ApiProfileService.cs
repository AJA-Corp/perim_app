using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using perimapp.Models;

namespace perimapp.Services
{
    public class ApiProfileService
    {
        private readonly HttpClient _httpClient;
        private const string BaseApiUrl = "https://perimapp-web-api.onrender.com/api/";

        public ApiProfileService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseApiUrl) };
        }

        private async Task AttachAuthenticationHeaderAsync()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<UserProfileDetails?> GetOrCreateMyProfileAsync(string? providedHomeCode = null)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();

                var response = await _httpClient.GetAsync($"FamilyInfos/me?homeCode={providedHomeCode}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserProfileDetails>();
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erreur Sync Profile] : {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateNameAsync(string firstName, string lastName)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();
                var dto = new { FirstName = firstName, LastName = lastName };
                var response = await _httpClient.PutAsJsonAsync("FamilyInfos/me", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erreur Update Name] : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteMyAccountAsync()
        {
            try
            {
                await AttachAuthenticationHeaderAsync();
                var response = await _httpClient.DeleteAsync("FamilyInfos/me");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erreur Delete Account] : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckHomeCodeExistsAsync(string code)
        {
            try
            {
                var response = await _httpClient.GetAsync($"FamilyInfos/check-code/{code}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erreur Check Code] : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ValidateHomeJoinCodeAsync(string code)
        {
            try
            {
                await AttachAuthenticationHeaderAsync();
                var response = await _httpClient.PostAsync($"FamilyInfos/validate-join?code={code}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur validation code : {ex.Message}");
                return false;
            }
        }
    }
}