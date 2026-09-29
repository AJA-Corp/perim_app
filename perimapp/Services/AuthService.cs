using Microsoft.Maui.Storage;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace perimapp.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        private const string NeonAuthBaseUrl = "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech/perimapp/auth/";
        private const int RequestTimeoutSeconds = 10;

        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(NeonAuthBaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds);

            _httpClient.DefaultRequestHeaders.Add("Origin", "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PerimApp/1.0");
        }

        /// <summary>
        /// Valide le format d'une adresse email
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var emailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
                return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, emailPattern);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valide la force du mot de passe (minimum 6 caractères)
        /// </summary>
        private bool IsValidPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }

        /// <summary>
        /// Nettoie les données d'entrée
        /// </summary>
        private string SanitizeInput(string input)
        {
            return input?.Trim() ?? string.Empty;
        }

        public async Task<bool> SignUpAsync(string email, string password, string firstName, string lastName)
        {
            email = SanitizeInput(email);
            password = SanitizeInput(password);
            firstName = SanitizeInput(firstName);
            lastName = SanitizeInput(lastName);

            // Validation client
            if (!IsValidEmail(email))
            {
                System.Diagnostics.Debug.WriteLine("[VALIDATION] Format email invalide");
                return false;
            }

            if (!IsValidPassword(password))
            {
                System.Diagnostics.Debug.WriteLine("[VALIDATION] Mot de passe trop court (min. 6 caractères)");
                return false;
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                System.Diagnostics.Debug.WriteLine("[VALIDATION] Prénom ou nom manquant");
                return false;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[SIGNUP] Tentative d'inscription: {email}");

                var handler = new HttpClientHandler { UseCookies = false };
                using var client = new HttpClient(handler) { BaseAddress = new Uri(NeonAuthBaseUrl) };
                client.Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds);

                client.DefaultRequestHeaders.Add("Origin", "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech");
                client.DefaultRequestHeaders.Add("User-Agent", "PerimApp/1.0");

                var signUpData = new
                {
                    email = email,
                    password = password,
                    name = $"{firstName} {lastName}"
                };

                System.Diagnostics.Debug.WriteLine("[SIGNUP] Envoi du payload: email=" + email + ", name=" + $"{firstName} {lastName}");

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
                            System.Diagnostics.Debug.WriteLine("[SIGNUP] Token du cookie récupéré");
                        }
                    }

                    if (string.IsNullOrEmpty(tokenTrouve))
                    {
                        var authResult = await response.Content.ReadFromJsonAsync<BetterAuthResponse>();
                        tokenTrouve = authResult?.Token;
                        System.Diagnostics.Debug.WriteLine("[SIGNUP] Token de la réponse JSON récupéré");
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
                    System.Diagnostics.Debug.WriteLine($"[ERREUR NEON SIGNUP] Code: {response.StatusCode}, Détails: {errorDetail}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG SIGNUP] Headers: {string.Join(", ", response.Headers.Select(h => h.Key + "=" + string.Join(";", h.Value)))}");
                    return false;
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[ERREUR RÉSEAU SIGNUP] {httpEx.Message}");
                return false;
            }
            catch (TaskCanceledException timeEx)
            {
                System.Diagnostics.Debug.WriteLine($"[ERREUR TIMEOUT SIGNUP] Requête dépassée après {RequestTimeoutSeconds}s: {timeEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CRASH SIGNUP] Erreur lors de l'inscription: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> SignInAsync(string email, string password)
        {
            email = SanitizeInput(email);
            password = SanitizeInput(password);

            // Validation client
            if (!IsValidEmail(email))
            {
                System.Diagnostics.Debug.WriteLine("[VALIDATION] Format email invalide");
                return false;
            }

            if (!IsValidPassword(password))
            {
                System.Diagnostics.Debug.WriteLine("[VALIDATION] Mot de passe trop court (min. 6 caractères)");
                return false;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[SIGNIN] Tentative de connexion: {email}");

                var handler = new HttpClientHandler { UseCookies = false };
                using var authClient = new HttpClient(handler) { BaseAddress = new Uri(NeonAuthBaseUrl) };
                authClient.Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds);

                authClient.DefaultRequestHeaders.Add("Origin", "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech");
                authClient.DefaultRequestHeaders.Add("User-Agent", "PerimApp/1.0");

                var signInData = new { email = email, password = password };

                System.Diagnostics.Debug.WriteLine("[SIGNIN] Envoi du payload: email=" + email);

                var response = await authClient.PostAsJsonAsync("sign-in/email", signInData);

                System.Diagnostics.Debug.WriteLine($"[SIGNIN] Réponse reçue: StatusCode={response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[SIGNIN] Content-Type: {response.Content?.Headers?.ContentType}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("[SIGNIN] Réponse 2xx - Succès");

                    if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                    {
                        System.Diagnostics.Debug.WriteLine($"[SIGNIN] Cookies reçus: {cookies.Count()}");
                        var neonCookie = cookies.FirstOrDefault(c => c.StartsWith("__Secure-neon-auth.session_token="));

                        if (neonCookie != null)
                        {
                            string secureTokenValue = neonCookie.Split(';')[0].Split('=')[1];
                            await SecureStorage.SetAsync("auth_token", secureTokenValue);
                            System.Diagnostics.Debug.WriteLine("[SUCCÈS] Jeton sécurisé (cookie) sauvegardé");
                            return true;
                        }
                    }

                    var authResult = await response.Content.ReadFromJsonAsync<BetterAuthResponse>();
                    if (!string.IsNullOrWhiteSpace(authResult?.Token))
                    {
                        await SecureStorage.SetAsync("auth_token", authResult.Token);
                        System.Diagnostics.Debug.WriteLine("[SUCCÈS] Jeton (réponse JSON) sauvegardé");
                        return true;
                    }

                    System.Diagnostics.Debug.WriteLine("[ALERTE] Succès 2xx reçu mais aucun token trouvé");
                    return false;
                }
                else
                {
                    string errorDetail = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ERREUR NEON SIGNIN] Code HTTP: {(int)response.StatusCode} {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"[ERREUR NEON SIGNIN] Détails: {errorDetail}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG SIGNIN] Content-Type: {response.Content?.Headers?.ContentType}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG SIGNIN] Headers: {string.Join(", ", response.Headers.Select(h => h.Key + "=" + string.Join(";", h.Value)))}");

                    return false;
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[ERREUR RÉSEAU SIGNIN] {httpEx.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERREUR RÉSEAU SIGNIN] InnerException: {httpEx.InnerException?.Message}");
                return false;
            }
            catch (TaskCanceledException timeEx)
            {
                System.Diagnostics.Debug.WriteLine($"[ERREUR TIMEOUT SIGNIN] Requête dépassée après {RequestTimeoutSeconds}s: {timeEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CRASH SIGNIN] Exception: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CRASH SIGNIN] StackTrace: {ex.StackTrace}");
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
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("[DELETE] Aucun token trouvé");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine("[DELETE] Tentative de suppression du compte");

                var handler = new HttpClientHandler { UseCookies = false };
                using var client = new HttpClient(handler) { BaseAddress = new Uri(NeonAuthBaseUrl) };
                client.Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds);

                client.DefaultRequestHeaders.Add("Origin", "https://ep-lingering-dream-abu6v3ac.neonauth.eu-west-2.aws.neon.tech");
                client.DefaultRequestHeaders.Add("User-Agent", "PerimApp/1.0");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("Cookie", $"__Secure-neon-auth.session_token={token}");

                var response = await client.PostAsync("user/delete", null);

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
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[ERREUR RÉSEAU DELETE] {httpEx.Message}");
                return false;
            }
            catch (TaskCanceledException timeEx)
            {
                System.Diagnostics.Debug.WriteLine($"[ERREUR TIMEOUT DELETE] Requête dépassée après {RequestTimeoutSeconds}s: {timeEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CRASH DELETE NEON] Exception: {ex.GetType().Name}: {ex.Message}");
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