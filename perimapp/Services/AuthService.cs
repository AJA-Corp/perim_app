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

        private const string NeonAuthBaseUrl = "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech/perimapp/auth/"; 

        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(NeonAuthBaseUrl);

            _httpClient.DefaultRequestHeaders.Add("Origin", "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech");
        }

        public async Task<bool> SignUpAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                var handler = new HttpClientHandler { UseCookies = false };
                using var client = new HttpClient(handler) { BaseAddress = new Uri(NeonAuthBaseUrl) };

                client.DefaultRequestHeaders.Add("Origin", "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech");

                var signUpData = new
                {
                    email = email,
                    password = password,
                    name = $"{firstName} {lastName}"
                };

                var response = await client.PostAsJsonAsync("sign-up/email", signUpData);

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
                        await SecureStorage.SetAsync("auth_token", tokenTrouve);
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

        public async Task<bool> SignInAsync(string email, string password)
        {
            try
            {
                var handler = new HttpClientHandler { UseCookies = false };
                using var authClient = new HttpClient(handler) { BaseAddress = new Uri(NeonAuthBaseUrl) };

                authClient.DefaultRequestHeaders.Add("Origin", "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech");

                var signInData = new { email = email, password = password };

                var response = await authClient.PostAsJsonAsync("sign-in/email", signInData);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                    {
                        var neonCookie = cookies.FirstOrDefault(c => c.StartsWith("__Secure-neon-auth.session_token="));

                        if (neonCookie != null)
                        {
                            string secureTokenValue = neonCookie.Split(';')[0].Split('=')[1];

                            await SecureStorage.SetAsync("auth_token", secureTokenValue);

                            Console.WriteLine("[SUCCÈS] Jeton crypté sauvegardé !");
                            return true;
                        }
                    }

                    var authResult = await response.Content.ReadFromJsonAsync<BetterAuthResponse>();
                    if (!string.IsNullOrWhiteSpace(authResult?.Token))
                    {
                        await SecureStorage.SetAsync("auth_token", authResult.Token);
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

        public void SignOut()
        {
            SecureStorage.Remove("auth_token");
            _httpClient.PostAsync("sign-out", null);
        }

        public async Task<string?> GetCurrentTokenAsync()
        {
            return await SecureStorage.GetAsync("auth_token");
        }

        public async Task<bool> DeleteNeonAccountAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token)) return false;

                var handler = new HttpClientHandler { UseCookies = false };
                using var client = new HttpClient(handler) { BaseAddress = new Uri(NeonAuthBaseUrl) };

                client.DefaultRequestHeaders.Add("Origin", "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("Cookie", $"__Secure-neon-auth.session_token={token}");

                var response = await client.PostAsync("user/delete", null);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("[SUCCÈS] Compte Neon Auth pulvérisé !");
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