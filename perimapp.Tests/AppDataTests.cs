using perimapp.Data;
using perimapp.Models;
using System.Collections.ObjectModel;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace perimapp.Tests
{
    public class AppDataTests
    {
        [Fact]
        public void AppData_Clear_ShouldResetAllFieldsToDefaults()
        {
            // Arrange
            AppData.CurrentUserId = 42;
            AppData.CurrentProducts = new ObservableCollection<ProductInfos> { new ProductInfos { Name = "Test Product" } };
            AppData.CurrentUser = new UserProfileDetails 
            { 
                FirstName = "Alice", 
                LastName = "Smith",
                Email = "alice@test.com",
                HomeCode = "H1"
            };
            AppData.NeedsAutoRefresh = false;

            // Act
            AppData.Clear();

            // Assert
            Assert.Equal(-1, AppData.CurrentUserId);
            Assert.Empty(AppData.CurrentProducts);
            Assert.Null(AppData.CurrentUser);
            Assert.True(AppData.NeedsAutoRefresh);
        }
    }
}
