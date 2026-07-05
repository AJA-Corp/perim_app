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
        private readonly ISecureStorage? _secureStorage;
        private const string BaseApiUrl = "https://perimapp-web-api.onrender.com/api/";

        public ApiProfileService(HttpClient? httpClient = null, ISecureStorage? secureStorage = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(BaseApiUrl);
            }
            _secureStorage = secureStorage;
        }

        private async Task AttachAuthenticationHeaderAsync()
        {
            string? token = null;
            if (_secureStorage != null)
            {
                token = await _secureStorage.GetAsync("auth_token");
            }
            else
            {
                try
                {
                    token = await SecureStorage.GetAsync("auth_token");
                }
                catch (Exception)
                {
                    // Fallback
                }
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public virtual async Task<UserProfileDetails?> GetOrCreateMyProfileAsync(string? providedHomeCode = null)
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

        public virtual async Task<bool> UpdateNameAsync(string firstName, string lastName)
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

        public virtual async Task<bool> DeleteMyAccountAsync()
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

        public virtual async Task<bool> CheckHomeCodeExistsAsync(string code)
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

        public virtual async Task<bool> ValidateHomeJoinCodeAsync(string code)
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