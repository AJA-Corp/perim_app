using Microsoft.Maui.Storage;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace perimapp.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ISecureStorage? _secureStorage;

        private const string NeonAuthBaseUrl = "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech/perimapp/auth/"; 

        public AuthService(HttpClient? httpClient = null, ISecureStorage? secureStorage = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(NeonAuthBaseUrl);
            }

            _httpClient.DefaultRequestHeaders.Add("Origin", "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech");
            _secureStorage = secureStorage;
        }

        private async Task SetTokenAsync(string token)
        {
            if (_secureStorage != null)
                await _secureStorage.SetAsync("auth_token", token);
            else
            {
                try { await SecureStorage.SetAsync("auth_token", token); } catch (Exception) {}
            }
        }

        private async Task<string?> GetTokenAsync()
        {
            if (_secureStorage != null)
                return await _secureStorage.GetAsync("auth_token");
            try { return await SecureStorage.GetAsync("auth_token"); } catch (Exception) { return null; }
        }

        private void RemoveToken()
        {
            if (_secureStorage != null)
                _secureStorage.Remove("auth_token");
            else
            {
                try { SecureStorage.Remove("auth_token"); } catch (Exception) {}
            }
        }

        public virtual async Task<bool> SignUpAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                var signUpData = new
                {
                    email = email,
                    password = password,
                    name = $"{firstName} {lastName}"
                };

                var response = await _httpClient.PostAsJsonAsync("sign-up/email", signUpData);

                if (response.IsSuccessStatusCode)
                {
                    string tokenTrouve = null;

                    if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                    {
                        var sessionCookie = cookies.FirstOrDefault(c => c.Contains("session_token"));
                        if (sessionCookie != null)
                        {
                            tokenTrouve = sessionCookie.Split(';')[0].Split('=')[1];
                        }
                    }

                    if (string.IsNullOrEmpty(tokenTrouve))
                    {
                        var authResult = await response.Content.ReadFromJsonAsync<BetterAuthResponse>();
                        tokenTrouve = authResult?.Token;
                    }

                    if (!string.IsNullOrEmpty(tokenTrouve))
                    {
                        await SetTokenAsync(tokenTrouve);
                        System.Diagnostics.Debug.WriteLine("[SUCCÈS] Utilisateur créé et Token sauvegardé.");
                        return true;
                    }

                    System.Diagnostics.Debug.WriteLine("[ALERTE] Compte créé mais aucun Token n'a été trouvé.");
                    return false;
                }
                else
                {
                    string errorDetail = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ERREUR NEON] Code: {response.StatusCode}, Détails: {errorDetail}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CRASH] Erreur lors de l'inscription : {ex.Message}");
                return false;
            }
        }

        public virtual async Task<bool> SignInAsync(string email, string password)
        {
            try
            {
                var signInData = new { email = email, password = password };

                var response = await _httpClient.PostAsJsonAsync("sign-in/email", signInData);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                    {
                        var neonCookie = cookies.FirstOrDefault(c => c.StartsWith("__Secure-neon-auth.session_token="));

                        if (neonCookie != null)
                        {
                            string secureTokenValue = neonCookie.Split(';')[0].Split('=')[1];

                            await SetTokenAsync(secureTokenValue);

                            Console.WriteLine("[SUCCÈS] Jeton crypté sauvegardé");
                            return true;
                        }
                    }

                    var authResult = await response.Content.ReadFromJsonAsync<BetterAuthResponse>();
                    if (!string.IsNullOrWhiteSpace(authResult?.Token))
                    {
                        await SetTokenAsync(authResult.Token);
                        return true;
                    }
                }
                else
                {
                    string errorDetail = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ERREUR NEON] Code: {response.StatusCode}, Détails: {errorDetail}");
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Crash Total Sign-In] : {ex.Message}");
                return false;
            }
        }

        public virtual void SignOut()
        {
            RemoveToken();
            _httpClient.PostAsync("sign-out", null);
        }

        public virtual async Task<string?> GetCurrentTokenAsync()
        {
            return await GetTokenAsync();
        }

        public virtual async Task<bool> DeleteNeonAccountAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token)) return false;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Remove("Cookie");
                _httpClient.DefaultRequestHeaders.Add("Cookie", $"__Secure-neon-auth.session_token={token}");

                var response = await _httpClient.PostAsync("user/delete", null);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("[SUCCÈS] Compte Neon Auth supprimé");
                    return true;
                }
                else
                {
                    string errorDetail = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ERREUR NEON DELETE] Code: {response.StatusCode}, Détails: {errorDetail}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CRASH DELETE NEON] : {ex.Message}");
                return false;
            }
        }
    }

    public class BetterAuthResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("user")]
        public BetterAuthUser? User { get; set; }
    }

    public class BetterAuthUser
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}