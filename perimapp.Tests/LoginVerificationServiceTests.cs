using Microsoft.VisualStudio.TestTools.UnitTesting;
using perimapp.Core.Services;
using System;
using System.Threading.Tasks;

namespace perimapp.Tests
{
    [TestClass]
    public class LoginVerificationServiceTests
    {
        private LoginVerificationService _verificationService;

        [TestInitialize]
        public void Setup()
        {
            _verificationService = new LoginVerificationService();
        }

        [TestMethod]
        public void GenerateVerificationCode_ReturnsValidCode()
        {
            // Act
            var code = _verificationService.GenerateVerificationCode();

            // Assert
            Assert.IsNotNull(code);
            Assert.AreEqual(6, code.Length);
            Assert.IsTrue(int.TryParse(code, out int parsedCode));
            Assert.IsTrue(parsedCode >= 100000 && parsedCode <= 999999);
        }

        [TestMethod]
        public async Task CreateVerificationSession_ReturnsValidSessionId()
        {
            // Arrange
            int userId = 123;
            string email = "test@example.com";
            string loginMethod = "email";

            // Act
            var sessionId = await _verificationService.CreateVerificationSessionAsync(userId, email, loginMethod);

            // Assert
            Assert.IsNotNull(sessionId);
            Assert.IsTrue(Guid.TryParse(sessionId, out _));
        }

        [TestMethod]
        public async Task VerifyCode_ValidCode_ReturnsTrue()
        {
            // Arrange
            int userId = 123;
            string email = "test@example.com";
            string loginMethod = "email";
            var sessionId = await _verificationService.CreateVerificationSessionAsync(userId, email, loginMethod);
            var session = _verificationService.GetSession(sessionId);

            // Act
            var isValid = _verificationService.VerifyCode(sessionId, session.VerificationCode);

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void VerifyCode_InvalidCode_ReturnsFalse()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Act
            var isValid = _verificationService.VerifyCode(sessionId, "123456");

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public async Task VerifyCode_WrongCode_ReturnsFalse()
        {
            // Arrange
            int userId = 123;
            string email = "test@example.com";
            string loginMethod = "email";
            var sessionId = await _verificationService.CreateVerificationSessionAsync(userId, email, loginMethod);

            // Act
            var isValid = _verificationService.VerifyCode(sessionId, "000000");

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public async Task GetSession_ValidSessionId_ReturnsSession()
        {
            // Arrange
            int userId = 123;
            string email = "test@example.com";
            string loginMethod = "email";
            var sessionId = await _verificationService.CreateVerificationSessionAsync(userId, email, loginMethod);

            // Act
            var session = _verificationService.GetSession(sessionId);

            // Assert
            Assert.IsNotNull(session);
            Assert.AreEqual(userId, session.UserId);
            Assert.AreEqual(email, session.Email);
            Assert.AreEqual(loginMethod, session.LoginMethod);
            Assert.IsNotNull(session.VerificationCode);
            Assert.IsTrue(session.ExpiresAt > DateTime.Now);
        }

        [TestMethod]
        public void GetSession_InvalidSessionId_ReturnsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Act
            var session = _verificationService.GetSession(sessionId);

            // Assert
            Assert.IsNull(session);
        }

        [TestMethod]
        public async Task CompleteVerification_RemovesSession()
        {
            // Arrange
            int userId = 123;
            string email = "test@example.com";
            string loginMethod = "email";
            var sessionId = await _verificationService.CreateVerificationSessionAsync(userId, email, loginMethod);

            // Act
            _verificationService.CompleteVerification(sessionId);
            var session = _verificationService.GetSession(sessionId);

            // Assert
            Assert.IsNull(session);
        }
    }
}