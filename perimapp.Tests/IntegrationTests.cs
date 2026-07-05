using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using perimapp.Models;
using perimapp.Services;
using Xunit;

namespace perimapp.Tests
{
    public class IntegrationTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly LocalProductService _localProductService;
        private readonly LocalUserService _localUserService;

        public IntegrationTests()
        {
            _tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IntegrationTestStorage_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);

            _localProductService = new LocalProductService(_tempDirectory);
            _localUserService = new LocalUserService(_tempDirectory);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                    Directory.Delete(_tempDirectory, true);
            }
            catch { }
        }

        // ================= 1. LOCAL INTEGRATION TESTS (MOCKED NETWORK) =================

        [Fact]
        public async Task LocalIntegration_OfflineOnlineSyncFlow()
        {
            // Arrange - Configure User and Local Product
            var user = new UserProfileDetails { Id = 999, FirstName = "Integration", LastName = "Test", HomeCode = "INT_HOME", Email = "integration@test.com" };
            await _localUserService.SaveUserAsync(user);

            var product = new ProductInfos
            {
                Barcode = 1234567890123,
                Name = "Local Product",
                HomeCode = user.HomeCode,
                Quantity = 2,
                Dlc = DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                State = "Active",
                SyncState = SyncState.PendingCreate
            };
            await _localProductService.AddProductAsync(product);

            // Mock API responding with success when online
            var mockHttpHandler = new MockHttpMessageHandler();
            mockHttpHandler.Handler = req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                if (req.RequestUri!.AbsolutePath.Contains("Inventory"))
                {
                    // Return the inventory list containing our product
                    var inventory = new List<ProductInfos> { product };
                    response.Content = JsonContent.Create(inventory);
                }
                return Task.FromResult(response);
            };

            var httpClient = new HttpClient(mockHttpHandler);
            var mockSecureStorage = new MockSecureStorage();
            await mockSecureStorage.SetAsync("auth_token", "mock_integration_token");

            var apiProductService = new ApiProductService(httpClient, mockSecureStorage);
            var mockConnectivity = new MockConnectivity();
            var syncService = new SyncService(apiProductService, _localProductService, mockConnectivity);

            // Act - Sync while OFFLINE
            mockConnectivity.NetworkAccess = NetworkAccess.None;
            await syncService.ProcessSyncAsync();

            // Assert - Product should still be PendingCreate
            var productsAfterOfflineSync = await _localProductService.LoadProductsAsync();
            Assert.Single(productsAfterOfflineSync);
            Assert.Equal(SyncState.PendingCreate, productsAfterOfflineSync[0].SyncState);

            // Act - Sync while ONLINE
            mockConnectivity.NetworkAccess = NetworkAccess.Internet;
            await syncService.ProcessSyncAsync();

            // Assert - Product should now be Synced
            var productsAfterOnlineSync = await _localProductService.LoadProductsAsync();
            Assert.Single(productsAfterOnlineSync);
            Assert.Equal(SyncState.Synced, productsAfterOnlineSync[0].SyncState);
        }

        // ================= 2. E2E INTEGRATION TESTS (REAL REMOTE SERVER) =================

        [Fact]
        public async Task RemoteIntegration_RealServerEndToEndFlow()
        {
            // 1. Warm-up and Wake-up Render Server (Free tier sleeps after 15 mins)
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(80) };
            bool isServerUp = false;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var response = await httpClient.GetAsync("https://perimapp-web-api.onrender.com/api/Health");
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
                    {
                        isServerUp = true;
                        break;
                    }
                }
                catch
                {
                    // Wait 15 seconds before retry
                    await Task.Delay(15000);
                }
            }

            if (!isServerUp)
            {
                // If Render is down or slow, skip the test gracefully rather than failing the build
                Console.WriteLine("[WARN] Remote Render API is currently unreachable. Skipping real server integration test.");
                return;
            }

            // 2. Setup Real Services with temporary secure storage and clean HTTP clients
            var mockSecureStorage = new MockSecureStorage();
            var authService = new AuthService(new HttpClient(), mockSecureStorage);
            var apiProfileService = new ApiProfileService(new HttpClient(), mockSecureStorage);
            var apiProductService = new ApiProductService(new HttpClient(), mockSecureStorage);
            var syncService = new SyncService(apiProductService, _localProductService, new MockConnectivity());

            string tempEmail = $"integration.{Guid.NewGuid().ToString("N")}@test.com";
            string password = "StrongPassword123!";

            try
            {
                // 3. Register user on real server
                bool registerSuccess = await authService.SignUpAsync(tempEmail, password, "Integration", "User");
                Assert.True(registerSuccess, "Registration failed on real server.");

                // 4. Sign in to retrieve auth token
                bool loginSuccess = await authService.SignInAsync(tempEmail, password);
                Assert.True(loginSuccess, "Login failed on real server.");

                // 5. Retrieve/Create profile
                var profile = await apiProfileService.GetOrCreateMyProfileAsync();
                Assert.NotNull(profile);
                Assert.NotEmpty(profile.HomeCode);

                // Save profile locally
                await _localUserService.SaveUserAsync(profile);

                // 6. Create product locally (PendingCreate)
                var localProduct = new ProductInfos
                {
                    Barcode = 9999999999999,
                    Name = "Integration Test Product",
                    HomeCode = profile.HomeCode,
                    Quantity = 5,
                    Dlc = DateOnly.FromDateTime(DateTime.Today.AddDays(15)),
                    State = "Active",
                    SyncState = SyncState.PendingCreate
                };
                await _localProductService.AddProductAsync(localProduct);

                // 7. Perform real synchronization
                await syncService.ProcessSyncAsync();

                // 8. Verify local state has been updated to Synced
                var localProducts = await _localProductService.LoadProductsAsync();
                var syncedProduct = localProducts.FirstOrDefault(p => p.Barcode == 9999999999999);
                Assert.NotNull(syncedProduct);
                Assert.Equal(SyncState.Synced, syncedProduct.SyncState);

                // 9. Fetch remote inventory to prove it's on the server
                var remoteInventory = await apiProductService.GetMyInventoryAsync();
                Assert.NotNull(remoteInventory);
                var remoteProduct = remoteInventory.FirstOrDefault(p => p.Barcode == 9999999999999);
                Assert.NotNull(remoteProduct);
                Assert.Equal("Integration Test Product", remoteProduct.Name);
                Assert.Equal(5, remoteProduct.Quantity);
            }
            finally
            {
                // 10. Clean up - Delete the remote account to keep Render/Neon DBs perfectly clean!
                try
                {
                    await apiProfileService.DeleteMyAccountAsync();
                    await authService.DeleteNeonAccountAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TEARDOWN ERROR] Échec lors de la suppression du compte de test : {ex.Message}");
                }
            }
        }
    }
}
