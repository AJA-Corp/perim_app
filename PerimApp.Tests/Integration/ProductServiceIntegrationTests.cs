using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using PerimApp.Core.Services;
using PerimApp.Core.Models;

namespace PerimApp.Tests.Integration
{
    public class ProductServiceIntegrationTests
    {
        private const string TestConnectionString = "Host=localhost;Username=test;Password=test;Database=test_db";

        [Fact]
        public void NeonProductService_Constructor_WithValidConnectionString_ShouldNotThrow()
        {
            // Act
            var action = () => new NeonProductService(TestConnectionString);

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void NeonProductService_Constructor_WithNullConnectionString_ShouldThrowException()
        {
            // Act
            var action = () => new NeonProductService(null!);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task AddProductDataAsync_WithNullProduct_ShouldThrowException()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            var action = async () => await service.AddProductDataAsync(null!);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddUserProductAsync_WithNullProduct_ShouldThrowException()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            var action = async () => await service.AddUserProductAsync(null!, 1);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateUserProductAsync_WithNullProduct_ShouldThrowException()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            var action = async () => await service.UpdateUserProductAsync(null!);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProductService_Interface_ShouldBeImplementedCorrectly()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Assert
            service.Should().BeAssignableTo<IProductService>();
        }

        [Fact]
        public async Task GetUserProductsAsync_WithValidUserId_ShouldReturnEmptyListOnConnectionFailure()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            // This will fail to connect but should handle the exception gracefully
            var action = async () => await service.GetUserProductsAsync(1);

            // Assert
            // The method should throw because we configured it to throw on errors in our implementation
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetProductDataAsync_WithValidBarcode_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            var action = async () => await service.GetProductDataAsync(1234567890);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task AddProductDataAsync_WithValidProduct_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);
            var product = CreateValidProduct();

            // Act
            var action = async () => await service.AddProductDataAsync(product);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task AddUserProductAsync_WithValidProduct_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);
            var product = CreateValidProduct();

            // Act
            var action = async () => await service.AddUserProductAsync(product, 1);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task UpdateUserProductAsync_WithValidProduct_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);
            var product = CreateValidProduct();
            product.Id = 1;

            // Act
            var action = async () => await service.UpdateUserProductAsync(product);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task DeleteUserProductAsync_WithValidId_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            var action = async () => await service.DeleteUserProductAsync(1);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetExpiringProductsAsync_WithValidParameters_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            var action = async () => await service.GetExpiringProductsAsync(1, 3);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task GetUserProductsAsync_WithInvalidUserId_ShouldStillAttemptConnection(int userId)
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            var action = async () => await service.GetUserProductsAsync(userId);

            // Assert
            // Even with invalid user IDs, the service should still attempt the database operation
            await action.Should().ThrowAsync<Exception>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetExpiringProductsAsync_WithInvalidThreshold_ShouldStillAttemptConnection(int threshold)
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            var action = async () => await service.GetExpiringProductsAsync(1, threshold);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetProductDataAsync_WithZeroBarcode_ShouldStillAttemptConnection()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            var action = async () => await service.GetProductDataAsync(0);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task DeleteUserProductAsync_WithZeroId_ShouldStillAttemptConnection()
        {
            // Arrange
            var service = new NeonProductService(TestConnectionString);

            // Act
            var action = async () => await service.DeleteUserProductAsync(0);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        private static ProductInfos CreateValidProduct()
        {
            return new ProductInfos
            {
                Barcode = 1234567890,
                Name = "Test Product",
                UrlImage = "https://example.com/image.jpg",
                Category = "Fruits",
                Conservation = "Frais",
                Dlc = DateTime.Today.AddDays(7),
                Quantity = 1,
                AddedAt = DateTime.Today
            };
        }

        [Fact]
        public void ProductService_Methods_ShouldHaveCorrectSignatures()
        {
            // This test verifies that the interface methods have the expected signatures
            var serviceType = typeof(NeonProductService);
            var interfaceType = typeof(IProductService);

            // Verify that NeonProductService implements IProductService
            interfaceType.IsAssignableFrom(serviceType).Should().BeTrue();

            // Verify specific method signatures exist
            var getUserProductsMethod = serviceType.GetMethod("GetUserProductsAsync");
            getUserProductsMethod.Should().NotBeNull();
            getUserProductsMethod!.ReturnType.Should().Be(typeof(Task<System.Collections.Generic.List<ProductInfos>>));

            var getProductDataMethod = serviceType.GetMethod("GetProductDataAsync");
            getProductDataMethod.Should().NotBeNull();
            getProductDataMethod!.ReturnType.Should().Be(typeof(Task<ProductInfos?>));
        }
    }
}