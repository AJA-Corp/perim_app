using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using perimapp.Models;
using perimapp.Services;
using Xunit;

namespace perimapp.Tests
{
    public class MockSecureStorage : ISecureStorage
    {
        public Dictionary<string, string> Storage { get; } = new();

        public Task SetAsync(string key, string value)
        {
            Storage[key] = value;
            return Task.CompletedTask;
        }

        public Task<string?> GetAsync(string key)
        {
            Storage.TryGetValue(key, out string? val);
            return Task.FromResult(val);
        }

        public bool Remove(string key)
        {
            return Storage.Remove(key);
        }

        public void RemoveAll()
        {
            Storage.Clear();
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, Task<HttpResponseMessage>> Handler { get; set; } = req => 
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Handler(request);
        }
    }

    public class MockConnectivity : IConnectivity
    {
        public NetworkAccess NetworkAccess { get; set; } = NetworkAccess.Internet;
        public IEnumerable<ConnectionProfile> ConnectionProfiles => Array.Empty<ConnectionProfile>();
        public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

        public void TriggerConnectivityChanged(NetworkAccess access)
        {
            NetworkAccess = access;
            ConnectivityChanged?.Invoke(this, new ConnectivityChangedEventArgs(access, ConnectionProfiles));
        }
    }

    public class ApiServicesTests
    {
        // ================= OPEN FOOD FACTS SERVICE TESTS =================

        [Fact]
        public async Task OpenFoodFactsService_GetProductFromApi_ReturnsProduct_WhenResponseIsSuccessful()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.Handler = req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var json = @"
                {
                    ""product"": {
                        ""product_name"": ""Chocolate Cookie"",
                        ""image_url"": ""http://images.com/cookie.jpg"",
                        ""categories"": ""Snacks, Sweet snacks""
                    }
                }";
                response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                return Task.FromResult(response);
            };

            var client = new HttpClient(mockHandler);
            var service = new OpenFoodFactsService(client);

            // Act
            var result = await service.GetProductFromApiAsync(123456789);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(123456789, result.Barcode);
            Assert.Equal("Chocolate Cookie", result.Name);
            Assert.Equal("http://images.com/cookie.jpg", result.UrlImage);
            Assert.Equal("Snacks, Sweet snacks", result.Category);
        }

        [Fact]
        public async Task OpenFoodFactsService_GetProductFromApi_ReturnsNull_OnException()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.Handler = req => throw new HttpRequestException("Network failure");
            var client = new HttpClient(mockHandler);
            var service = new OpenFoodFactsService(client);

            // Act
            var result = await service.GetProductFromApiAsync(123);

            // Assert
            Assert.Null(result);
        }

        // ================= AUTH SERVICE TESTS =================

        [Fact]
        public async Task AuthService_SignUpAsync_ReturnsTrue_AndSavesToken_WhenSuccess()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.Handler = req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = JsonContent.Create(new BetterAuthResponse
                {
                    Token = "my_signup_token",
                    User = new BetterAuthUser { Email = "test@test.com" }
                });
                return Task.FromResult(response);
            };
            var secureStorage = new MockSecureStorage();
            var authService = new AuthService(new HttpClient(mockHandler), secureStorage);

            // Act
            var result = await authService.SignUpAsync("test@test.com", "pass", "John", "Doe");

            // Assert
            Assert.True(result);
            Assert.Equal("my_signup_token", secureStorage.Storage["auth_token"]);
        }

        [Fact]
        public async Task AuthService_SignInAsync_ReturnsTrue_AndSavesToken_WhenSuccess()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.Handler = req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = JsonContent.Create(new BetterAuthResponse
                {
                    Token = "my_signin_token"
                });
                return Task.FromResult(response);
            };
            var secureStorage = new MockSecureStorage();
            var authService = new AuthService(new HttpClient(mockHandler), secureStorage);

            // Act
            var result = await authService.SignInAsync("test@test.com", "pass");

            // Assert
            Assert.True(result);
            Assert.Equal("my_signin_token", secureStorage.Storage["auth_token"]);
        }

        [Fact]
        public async Task AuthService_SignOut_RemovesToken()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var secureStorage = new MockSecureStorage();
            secureStorage.Storage["auth_token"] = "some_token";
            var authService = new AuthService(new HttpClient(mockHandler), secureStorage);

            // Act
            authService.SignOut();

            // Assert
            Assert.False(secureStorage.Storage.ContainsKey("auth_token"));
        }

        [Fact]
        public async Task AuthService_DeleteNeonAccount_ReturnsTrue_WhenSuccess()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.Handler = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            var secureStorage = new MockSecureStorage();
            secureStorage.Storage["auth_token"] = "token";
            var authService = new AuthService(new HttpClient(mockHandler), secureStorage);

            // Act
            var result = await authService.DeleteNeonAccountAsync();

            // Assert
            Assert.True(result);
        }

        // ================= API PRODUCT SERVICE TESTS =================

        [Fact]
        public async Task ApiProductService_SearchProduct_ReturnsProduct()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.Handler = req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = JsonContent.Create(new ProductInfos { Name = "Server Product", Barcode = 123 });
                return Task.FromResult(response);
            };
            var secureStorage = new MockSecureStorage();
            secureStorage.Storage["auth_token"] = "my_token";
            var service = new ApiProductService(new HttpClient(mockHandler), secureStorage);

            // Act
            var product = await service.SearchProductAsync(123);

            // Assert
            Assert.NotNull(product);
            Assert.Equal("Server Product", product.Name);
        }

        [Fact]
        public async Task ApiProductService_AddProductToInventory_ReturnsTrue()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var secureStorage = new MockSecureStorage();
            var service = new ApiProductService(new HttpClient(mockHandler), secureStorage);

            // Act
            var success = await service.AddProductToInventoryAsync(new ProductInfos { Name = "New" });

            // Assert
            Assert.True(success);
        }

        [Fact]
        public async Task ApiProductService_GetMyInventory_ReturnsList()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.Handler = req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = JsonContent.Create(new List<ProductInfos>
                {
                    new ProductInfos { Name = "P1" },
                    new ProductInfos { Name = "P2" }
                });
                return Task.FromResult(response);
            };
            var secureStorage = new MockSecureStorage();
            var service = new ApiProductService(new HttpClient(mockHandler), secureStorage);

            // Act
            var list = await service.GetMyInventoryAsync();

            // Assert
            Assert.Equal(2, list.Count);
            Assert.Equal("P1", list[0].Name);
        }

        [Fact]
        public async Task ApiProductService_UpdateProductState_ReturnsTrue()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var secureStorage = new MockSecureStorage();
            var service = new ApiProductService(new HttpClient(mockHandler), secureStorage);

            // Act
            var success = await service.UpdateProductStateAsync("unique_id", "Active");

            // Assert
            Assert.True(success);
        }

        [Fact]
        public async Task ApiProductService_SyncOfflineProducts_ReturnsTrue()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var secureStorage = new MockSecureStorage();
            var service = new ApiProductService(new HttpClient(mockHandler), secureStorage);

            // Act
            var success = await service.SyncOfflineProductsAsync(new List<ProductInfos>());

            // Assert
            Assert.True(success);
        }

        [Fact]
        public async Task ApiProductService_EmptyTrashOnline_ReturnsTrue()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var secureStorage = new MockSecureStorage();
            var service = new ApiProductService(new HttpClient(mockHandler), secureStorage);

            // Act
            var success = await service.EmptyTrashOnlineAsync();

            // Assert
            Assert.True(success);
        }

        [Fact]
        public async Task ApiProductService_CustomProductNames_SaveAndGet()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.Handler = req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                if (req.Method == HttpMethod.Get)
                {
                    response.Content = new StringContent("\"Server Custom Name\"");
                }
                return Task.FromResult(response);
            };
            var secureStorage = new MockSecureStorage();
            var service = new ApiProductService(new HttpClient(mockHandler), secureStorage);

            // Act
            var setSuccess = await service.SetCustomProductNameAsync(123, "home1", "Server Custom Name");
            var getResult = await service.GetCustomProductNameAsync(123, "home1");

            // Assert
            Assert.True(setSuccess);
            Assert.Equal("Server Custom Name", getResult);
        }

        // ================= API PROFILE SERVICE TESTS =================

        [Fact]
        public async Task ApiProfileService_GetOrCreateMyProfile_ReturnsProfile()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.Handler = req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = JsonContent.Create(new UserProfileDetails { FirstName = "Bob", HomeCode = "XYZ", Email = "bob@test.com" });
                return Task.FromResult(response);
            };
            var secureStorage = new MockSecureStorage();
            var service = new ApiProfileService(new HttpClient(mockHandler), secureStorage);

            // Act
            var profile = await service.GetOrCreateMyProfileAsync();

            // Assert
            Assert.NotNull(profile);
            Assert.Equal("Bob", profile.FirstName);
            Assert.Equal("XYZ", profile.HomeCode);
        }

        [Fact]
        public async Task ApiProfileService_UpdateName_ReturnsTrue()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var secureStorage = new MockSecureStorage();
            var service = new ApiProfileService(new HttpClient(mockHandler), secureStorage);

            // Act
            var success = await service.UpdateNameAsync("Bob", "Sponge");

            // Assert
            Assert.True(success);
        }

        [Fact]
        public async Task ApiProfileService_DeleteMyAccount_ReturnsTrue()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var secureStorage = new MockSecureStorage();
            var service = new ApiProfileService(new HttpClient(mockHandler), secureStorage);

            // Act
            var success = await service.DeleteMyAccountAsync();

            // Assert
            Assert.True(success);
        }

        [Fact]
        public async Task ApiProfileService_CheckHomeCodeExists_ReturnsTrue()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var secureStorage = new MockSecureStorage();
            var service = new ApiProfileService(new HttpClient(mockHandler), secureStorage);

            // Act
            var success = await service.CheckHomeCodeExistsAsync("CODE");

            // Assert
            Assert.True(success);
        }

        [Fact]
        public async Task ApiProfileService_ValidateHomeJoinCode_ReturnsTrue()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            var secureStorage = new MockSecureStorage();
            var service = new ApiProfileService(new HttpClient(mockHandler), secureStorage);

            // Act
            var success = await service.ValidateHomeJoinCodeAsync("123456");

            // Assert
            Assert.True(success);
        }

        // ================= SYNC SERVICE TESTS =================

        [Fact]
        public async Task SyncService_ProcessSyncAsync_PerformsOfflineAndOnlineSynchronization()
        {
            // Arrange
            string tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SyncTestStorage_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                var localDb = new LocalProductService(tempDir);
                // Setup local offline items:
                // p1 is PendingCreate -> should be updated to Synced
                // p2 is PendingDelete -> should be hard deleted
                var p1 = new ProductInfos { ProductUniqueId = "p1", Name = "Local Product 1", SyncState = SyncState.PendingCreate };
                var p2 = new ProductInfos { ProductUniqueId = "p2", Name = "Local Product 2", SyncState = SyncState.PendingDelete };
                await localDb.SaveProductsAsync(new List<ProductInfos> { p1, p2 });

                var apiHandler = new MockHttpMessageHandler();
                apiHandler.Handler = req =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    if (req.RequestUri!.AbsolutePath.Contains("Inventory/my-inventory"))
                    {
                        // Server inventory has P1 and P3
                        response.Content = JsonContent.Create(new List<ProductInfos>
                        {
                            new ProductInfos { ProductUniqueId = "p1", Name = "Local Product 1" },
                            new ProductInfos { ProductUniqueId = "p3", Name = "Server Product 3" }
                        });
                    }
                    return Task.FromResult(response);
                };

                var secureStorage = new MockSecureStorage();
                var apiProductService = new ApiProductService(new HttpClient(apiHandler), secureStorage);
                var mockConnectivity = new MockConnectivity();

                var syncService = new SyncService(apiProductService, localDb, mockConnectivity);

                // Act
                await syncService.ProcessSyncAsync();

                // Assert
                var loadedLocal = await localDb.LoadProductsAsync();
                // p2 should be hard deleted. p1 should be updated (to Synced). p3 should be downloaded from server.
                Assert.Equal(2, loadedLocal.Count);
                
                var p1Loaded = loadedLocal.Find(p => p.ProductUniqueId == "p1");
                Assert.NotNull(p1Loaded);
                Assert.Equal(SyncState.Synced, p1Loaded.SyncState);

                var p3Loaded = loadedLocal.Find(p => p.ProductUniqueId == "p3");
                Assert.NotNull(p3Loaded);
                Assert.Equal("Server Product 3", p3Loaded.Name);
                Assert.Equal(SyncState.Synced, p3Loaded.SyncState);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}
