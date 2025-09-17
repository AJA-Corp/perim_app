using Microsoft.VisualStudio.TestTools.UnitTesting;
using perimapp.Services;
using perimapp.Models;
using System.Threading.Tasks;

namespace perimapp.Tests
{
    [TestClass]
    public class EmailServiceTests
    {
        private EmailService _emailService;

        [TestInitialize]
        public void Setup()
        {
            _emailService = new EmailService();
        }

        [TestMethod]
        public async Task SendLoginConfirmationEmailAsync_DevelopmentMode_ReturnsTrue()
        {
            // Arrange
            string testEmail = "test@example.com";
            string testCode = "123456";

            // Act - should use development mode by default
            var result = await _emailService.SendLoginConfirmationEmailAsync(testEmail, testCode);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SendLoginConfirmationEmailAsync_InvalidConfig_FallsBackToDevelopmentMode()
        {
            // Arrange
            string testEmail = "test@example.com";
            string testCode = "123456";
            
            // Configure with invalid credentials
            _emailService.ConfigureEmail("invalid@email.com", "invalidpassword", EmailProvider.Gmail);

            // Act - should fall back to development mode
            var result = await _emailService.SendLoginConfirmationEmailAsync(testEmail, testCode);

            // Assert
            Assert.IsTrue(result); // Should return true due to fallback
        }

        [TestMethod]
        public void EmailConfig_DefaultConfiguration_IsDevelopmentMode()
        {
            // Arrange & Act
            var config = new EmailConfig();

            // Assert
            Assert.IsTrue(config.DevelopmentMode);
            Assert.AreEqual(EmailProvider.Development, config.Provider);
            Assert.IsFalse(config.IsConfigured);
        }

        [TestMethod]
        public void EmailConfig_WithCredentials_IsConfigured()
        {
            // Arrange & Act
            var config = new EmailConfig
            {
                SenderEmail = "test@example.com",
                SenderPassword = "password",
                DevelopmentMode = false
            };

            // Assert
            Assert.IsTrue(config.IsConfigured);
        }
    }
}