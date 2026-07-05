using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using perimapp.Models;
using perimapp.Services;
using Xunit;

namespace perimapp.Tests
{
    public class LocalServicesTests : IDisposable
    {
        private readonly string _tempDirectory;

        public LocalServicesTests()
        {
            _tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestStorage_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, true);
                }
            }
            catch { }
        }

        [Fact]
        public async Task LocalProductService_ShouldSaveAndLoadProducts()
        {
            // Arrange
            var service = new LocalProductService(_tempDirectory);
            var products = new List<ProductInfos>
            {
                new ProductInfos { Barcode = 123456, Name = "Apple", State = "Active" },
                new ProductInfos { Barcode = 789012, Name = "Milk", State = "Active" }
            };

            // Act
            await service.SaveProductsAsync(products);
            var loaded = await service.LoadProductsAsync();

            // Assert
            Assert.Equal(2, loaded.Count);
            Assert.Equal("Apple", loaded[0].Name);
            Assert.Equal("Milk", loaded[1].Name);
        }

        [Fact]
        public async Task LocalProductService_LoadProducts_ShouldReturnEmpty_WhenFileDoesNotExist()
        {
            // Arrange
            var service = new LocalProductService(_tempDirectory);

            // Act
            var loaded = await service.LoadProductsAsync();

            // Assert
            Assert.Empty(loaded);
        }

        [Fact]
        public async Task LocalProductService_AddProduct_ShouldAddUniqueProduct()
        {
            // Arrange
            var service = new LocalProductService(_tempDirectory);
            var product = new ProductInfos { ProductUniqueId = "p1", Name = "Orange", Barcode = 111 };

            // Act
            await service.AddProductAsync(product);
            var loaded = await service.LoadProductsAsync();

            // Assert
            Assert.Single(loaded);
            Assert.Equal("Orange", loaded[0].Name);
            Assert.Equal(SyncState.PendingCreate, loaded[0].SyncState);

            // Try adding same product
            await service.AddProductAsync(product);
            var loaded2 = await service.LoadProductsAsync();
            Assert.Single(loaded2);
        }

        [Fact]
        public async Task LocalProductService_GetPendingSyncProducts_ShouldReturnNonSynced()
        {
            // Arrange
            var service = new LocalProductService(_tempDirectory);
            var p1 = new ProductInfos { ProductUniqueId = "p1", Name = "P1", SyncState = SyncState.PendingCreate };
            var p2 = new ProductInfos { ProductUniqueId = "p2", Name = "P2", SyncState = SyncState.Synced };
            var p3 = new ProductInfos { ProductUniqueId = "p3", Name = "P3", SyncState = SyncState.PendingUpdate };

            await service.SaveProductsAsync(new List<ProductInfos> { p1, p2, p3 });

            // Act
            var pending = await service.GetPendingSyncProductsAsync();

            // Assert
            Assert.Equal(2, pending.Count);
            Assert.Contains(pending, p => p.ProductUniqueId == "p1");
            Assert.Contains(pending, p => p.ProductUniqueId == "p3");
        }

        [Fact]
        public async Task LocalProductService_HardDeleteProduct_ShouldRemoveProduct()
        {
            // Arrange
            var service = new LocalProductService(_tempDirectory);
            var p1 = new ProductInfos { ProductUniqueId = "p1", Name = "P1" };
            var p2 = new ProductInfos { ProductUniqueId = "p2", Name = "P2" };
            await service.SaveProductsAsync(new List<ProductInfos> { p1, p2 });

            // Act
            await service.HardDeleteProductAsync(p1);
            var loaded = await service.LoadProductsAsync();

            // Assert
            Assert.Single(loaded);
            Assert.Equal("p2", loaded[0].ProductUniqueId);
        }

        [Fact]
        public async Task LocalProductService_UpdateProductLocal_ShouldModifyProductAndSetPendingUpdate()
        {
            // Arrange
            var service = new LocalProductService(_tempDirectory);
            var p1 = new ProductInfos { ProductUniqueId = "p1", Name = "P1", SyncState = SyncState.Synced };
            await service.SaveProductsAsync(new List<ProductInfos> { p1 });

            // Act
            p1.Name = "Updated P1";
            await service.UpdateProductLocalAsync(p1);
            var loaded = await service.LoadProductsAsync();

            // Assert
            Assert.Single(loaded);
            Assert.Equal("Updated P1", loaded[0].Name);
            Assert.Equal(SyncState.PendingUpdate, loaded[0].SyncState);
        }

        [Fact]
        public async Task LocalProductService_UpdateProductState_ShouldUpdateStateAndDates()
        {
            // Arrange
            var service = new LocalProductService(_tempDirectory);
            var p1 = new ProductInfos { ProductUniqueId = "p1", Name = "P1", State = "Active", SyncState = SyncState.Synced };
            await service.SaveProductsAsync(new List<ProductInfos> { p1 });

            // Act
            var result = await service.UpdateProductStateAsync("p1", "Deleted");
            var loaded = await service.LoadProductsAsync();

            // Assert
            Assert.True(result);
            Assert.Equal("Deleted", loaded[0].State);
            Assert.NotNull(loaded[0].DeletedAt);
            Assert.Equal(SyncState.PendingUpdate, loaded[0].SyncState);

            // Attempt to update non-existing product
            var result2 = await service.UpdateProductStateAsync("non-existing", "Deleted");
            Assert.False(result2);
        }

        [Fact]
        public async Task LocalProductService_EmptyTrashLocally_ShouldRemoveOrMarkDeleted()
        {
            // Arrange
            var service = new LocalProductService(_tempDirectory);
            // Case 1: PendingCreate and Deleted -> Should be hard deleted
            var p1 = new ProductInfos { ProductUniqueId = "p1", Name = "P1", State = "Deleted", SyncState = SyncState.PendingCreate };
            // Case 2: Synced and Deleted -> Should be set to HardDeleted / PendingDelete
            var p2 = new ProductInfos { ProductUniqueId = "p2", Name = "P2", State = "Deleted", SyncState = SyncState.Synced };

            await service.SaveProductsAsync(new List<ProductInfos> { p1, p2 });

            // Act
            await service.EmptyTrashLocallyAsync();
            var loaded = await service.LoadProductsAsync();

            // Assert
            Assert.Single(loaded);
            Assert.Equal("p2", loaded[0].ProductUniqueId);
            Assert.Equal("HardDeleted", loaded[0].State);
            Assert.Equal(SyncState.PendingDelete, loaded[0].SyncState);
        }

        [Fact]
        public async Task LocalProductService_CustomProductNames_ShouldSaveAndGet()
        {
            // Arrange
            var service = new LocalProductService(_tempDirectory);

            // Act
            await service.SaveCustomProductNameAsync(12345, "home_1", "Yogurt");
            var customName = await service.GetCustomProductNameAsync(12345, "home_1");
            var missingName = await service.GetCustomProductNameAsync(999, "home_1");

            // Assert
            Assert.Equal("Yogurt", customName);
            Assert.Null(missingName);
        }

        [Fact]
        public async Task LocalProductService_ClearAllProducts_ShouldDeleteFiles()
        {
            // Arrange
            var service = new LocalProductService(_tempDirectory);
            await service.SaveProductsAsync(new List<ProductInfos> { new ProductInfos { Name = "P1" } });
            await service.SaveCustomProductNameAsync(123, "H", "C");

            // Act
            service.ClearAllProducts();
            var loaded = await service.LoadProductsAsync();

            // Assert
            Assert.Empty(loaded);
            Assert.Null(await service.GetCustomProductNameAsync(123, "H"));
        }

        // ================= LOCAL USER SERVICE TESTS =================

        [Fact]
        public async Task LocalUserService_ShouldSaveAndLoadUser()
        {
            // Arrange
            var service = new LocalUserService(_tempDirectory);
            var user = new UserProfileDetails { Id = 10, FirstName = "John", LastName = "Doe", LostProductCount = 5, Email = "john@test.com", HomeCode = "H1" };

            // Act
            await service.SaveUserAsync(user);
            var loaded = await service.LoadUserAsync();

            // Assert
            Assert.NotNull(loaded);
            Assert.Equal("John", loaded.FirstName);
            Assert.Equal("Doe", loaded.LastName);
            Assert.Equal(5, loaded.LostProductCount);
        }

        [Fact]
        public async Task LocalUserService_LoadUser_ShouldReturnNull_WhenFileDoesNotExist()
        {
            // Arrange
            var service = new LocalUserService(_tempDirectory);

            // Act
            var loaded = await service.LoadUserAsync();

            // Assert
            Assert.Null(loaded);
        }

        [Fact]
        public async Task LocalUserService_ClearUser_ShouldDeleteFile()
        {
            // Arrange
            var service = new LocalUserService(_tempDirectory);
            await service.SaveUserAsync(new UserProfileDetails { FirstName = "Bob", Email = "bob@test.com", HomeCode = "H1" });

            // Act
            service.ClearUser();
            var loaded = await service.LoadUserAsync();

            // Assert
            Assert.Null(loaded);
        }

        [Fact]
        public async Task LocalUserService_IncrementDecrementLostProductCount_ShouldUpdateCount()
        {
            // Arrange
            var service = new LocalUserService(_tempDirectory);
            var user = new UserProfileDetails { Id = 1, FirstName = "Bob", LostProductCount = 2, Email = "bob@test.com", HomeCode = "H1" };
            await service.SaveUserAsync(user);

            // Act & Assert (Increment)
            var incResult = await service.IncrementLostProductCountAsync();
            var loadedInc = await service.LoadUserAsync();
            Assert.True(incResult);
            Assert.Equal(3, loadedInc?.LostProductCount);

            // Act & Assert (Decrement)
            var decResult = await service.DecrementLostProductCountAsync();
            var loadedDec = await service.LoadUserAsync();
            Assert.True(decResult);
            Assert.Equal(2, loadedDec?.LostProductCount);

            // Decrement to zero and further
            await service.DecrementLostProductCountAsync(); // 1
            await service.DecrementLostProductCountAsync(); // 0
            var decResult2 = await service.DecrementLostProductCountAsync(); // stays 0
            var loadedDec2 = await service.LoadUserAsync();
            Assert.True(decResult2);
            Assert.Equal(0, loadedDec2?.LostProductCount);
        }

        [Fact]
        public async Task LocalUserService_IncrementDecrementLostProductCount_ShouldReturnFalse_WhenNoUser()
        {
            // Arrange
            var service = new LocalUserService(_tempDirectory);

            // Act
            var inc = await service.IncrementLostProductCountAsync();
            var dec = await service.DecrementLostProductCountAsync();

            // Assert
            Assert.False(inc);
            Assert.False(dec);
        }
    }
}
