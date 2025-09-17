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
        public async Task SendLoginConfirmationEmailAsync_UnconfiguredEmail_ReturnsFalse()
        {
            // Arrange
            string testEmail = "test@example.com";
            string testCode = "123456";

            // Act - should fail due to unconfigured email
            var result = await _emailService.SendLoginConfirmationEmailAsync(testEmail, testCode);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EmailConfig_DefaultConfiguration_IsNotConfigured()
        {
            // Arrange & Act
            var config = new EmailConfig();

            // Assert
            Assert.IsFalse(config.IsConfigured);
        }

        [TestMethod]
        public void EmailConfig_WithValidCredentials_IsConfigured()
        {
            // Arrange & Act
            var config = new EmailConfig
            {
                SenderEmail = "test@example.com",
                SenderPassword = "validpassword"
            };

            // Assert
            Assert.IsTrue(config.IsConfigured);
        }

        [TestMethod]
        public void EmailConfig_WithDefaultPlaceholders_IsNotConfigured()
        {
            // Arrange & Act - Default placeholders should not be considered configured
            var config = new EmailConfig();
            config.SenderEmail = "votre-email@gmail.com";
            config.SenderPassword = "votre-mot-de-passe-application";

            // Assert
            Assert.IsFalse(config.IsConfigured);
        }
    }
}