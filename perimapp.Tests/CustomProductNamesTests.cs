using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using perimapp.Models;
using perimapp.Services;

namespace perimapp.Tests
{
    [TestClass]
    public class CustomProductNamesTests
    {
        private LocalProductService _localService;
        private string _testDataPath;

        [TestInitialize]
        public void Setup()
        {
            _localService = new LocalProductService();
            _testDataPath = Path.Combine(Path.GetTempPath(), "test_custom_names.json");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_testDataPath))
            {
                File.Delete(_testDataPath);
            }
        }

        [TestMethod]
        public void ProductInfos_DisplayName_ShowsCustomNameWhenAvailable()
        {
            // Arrange
            var product = new ProductInfos
            {
                Name = "Original Product Name",
                CustomName = "Custom Family Name"
            };

            // Act & Assert
            Assert.AreEqual("Custom Family Name", product.DisplayName);
        }

        [TestMethod]
        public void ProductInfos_DisplayName_ShowsOriginalNameWhenNoCustomName()
        {
            // Arrange
            var product = new ProductInfos
            {
                Name = "Original Product Name",
                CustomName = null
            };

            // Act & Assert
            Assert.AreEqual("Original Product Name", product.DisplayName);
        }

        [TestMethod]
        public void ProductInfos_DisplayName_ShowsOriginalNameWhenCustomNameIsEmpty()
        {
            // Arrange
            var product = new ProductInfos
            {
                Name = "Original Product Name",
                CustomName = ""
            };

            // Act & Assert
            Assert.AreEqual("Original Product Name", product.DisplayName);
        }

        [TestMethod]
        public void ProductInfos_DisplayName_ShowsOriginalNameWhenCustomNameIsWhitespace()
        {
            // Arrange
            var product = new ProductInfos
            {
                Name = "Original Product Name",
                CustomName = "   "
            };

            // Act & Assert
            Assert.AreEqual("Original Product Name", product.DisplayName);
        }

        [TestMethod]
        public async Task LocalProductService_SaveAndGetCustomName_WorksCorrectly()
        {
            // Arrange
            long barcode = 123456789;
            int homeCode = 555123;
            string customName = "Family Custom Name";

            // Act
            await _localService.SaveCustomProductNameAsync(barcode, homeCode, customName);
            string? retrievedName = await _localService.GetCustomProductNameAsync(barcode, homeCode);

            // Assert
            Assert.AreEqual(customName, retrievedName);
        }

        [TestMethod]
        public async Task LocalProductService_GetCustomName_ReturnsNullForNonExistentName()
        {
            // Arrange
            long barcode = 987654321;
            int homeCode = 555123;

            // Act
            string? retrievedName = await _localService.GetCustomProductNameAsync(barcode, homeCode);

            // Assert
            Assert.IsNull(retrievedName);
        }

        [TestMethod]
        public async Task LocalProductService_SaveCustomName_OverwritesExistingName()
        {
            // Arrange
            long barcode = 123456789;
            int homeCode = 555123;
            string originalName = "Original Custom Name";
            string updatedName = "Updated Custom Name";

            // Act
            await _localService.SaveCustomProductNameAsync(barcode, homeCode, originalName);
            await _localService.SaveCustomProductNameAsync(barcode, homeCode, updatedName);
            string? retrievedName = await _localService.GetCustomProductNameAsync(barcode, homeCode);

            // Assert
            Assert.AreEqual(updatedName, retrievedName);
        }

        [TestMethod]
        public async Task LocalProductService_CustomNamesAreIsolatedByHomeCode()
        {
            // Arrange
            long barcode = 123456789;
            int homeCode1 = 555123;
            int homeCode2 = 555456;
            string customName1 = "Family 1 Name";
            string customName2 = "Family 2 Name";

            // Act
            await _localService.SaveCustomProductNameAsync(barcode, homeCode1, customName1);
            await _localService.SaveCustomProductNameAsync(barcode, homeCode2, customName2);
            
            string? retrievedName1 = await _localService.GetCustomProductNameAsync(barcode, homeCode1);
            string? retrievedName2 = await _localService.GetCustomProductNameAsync(barcode, homeCode2);

            // Assert
            Assert.AreEqual(customName1, retrievedName1);
            Assert.AreEqual(customName2, retrievedName2);
            Assert.AreNotEqual(retrievedName1, retrievedName2);
        }
    }
}