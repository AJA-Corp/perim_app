using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace perimapp.Services
{
    public class LoginVerificationService
    {
        private static readonly Dictionary<string, VerificationSession> _verificationSessions = new();

        public class VerificationSession
        {
            public int UserId { get; set; }
            public string Email { get; set; }
            public string VerificationCode { get; set; }
            public DateTime ExpiresAt { get; set; }
            public string LoginMethod { get; set; } // "email" or "homecode"
        }

        public string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public async Task<string> CreateVerificationSessionAsync(int userId, string email, string loginMethod)
        {
            var verificationCode = GenerateVerificationCode();
            var sessionId = Guid.NewGuid().ToString();

            var session = new VerificationSession
            {
                UserId = userId,
                Email = email,
                VerificationCode = verificationCode,
                ExpiresAt = DateTime.Now.AddMinutes(5), // 5 minutes expiration
                LoginMethod = loginMethod
            };

            _verificationSessions[sessionId] = session;
            
            // Cleanup expired sessions
            CleanupExpiredSessions();

            return sessionId;
        }

        public bool VerifyCode(string sessionId, string enteredCode)
        {
            if (!_verificationSessions.TryGetValue(sessionId, out var session))
                return false;

            if (DateTime.Now > session.ExpiresAt)
            {
                _verificationSessions.Remove(sessionId);
                return false;
            }

            return session.VerificationCode == enteredCode;
        }

        public VerificationSession GetSession(string sessionId)
        {
            _verificationSessions.TryGetValue(sessionId, out var session);
            return session;
        }

        public void CompleteVerification(string sessionId)
        {
            _verificationSessions.Remove(sessionId);
        }

        private void CleanupExpiredSessions()
        {
            var expiredSessions = new List<string>();
            foreach (var kvp in _verificationSessions)
            {
                if (DateTime.Now > kvp.Value.ExpiresAt)
                {
                    expiredSessions.Add(kvp.Key);
                }
            }

            foreach (var sessionId in expiredSessions)
            {
                _verificationSessions.Remove(sessionId);
            }
        }
    }
}