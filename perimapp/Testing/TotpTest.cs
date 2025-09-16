using System;
using perimapp.Services;

namespace perimapp.Testing
{
    public static class TotpTest
    {
        public static void RunBasicTest()
        {
            try
            {
                Console.WriteLine("[TOTP Test] Starting basic TOTP functionality test...");
                
                var totpService = new TotpService();
                
                // Test 1: Generate a secret
                string secret = totpService.GenerateSecret();
                Console.WriteLine($"[TOTP Test] Generated secret: {secret}");
                
                if (string.IsNullOrEmpty(secret) || secret.Length < 16)
                {
                    Console.WriteLine("[TOTP Test] ERROR: Secret generation failed");
                    return;
                }
                
                // Test 2: Generate a code
                string code = totpService.GenerateCode(secret);
                Console.WriteLine($"[TOTP Test] Generated code: {code}");
                
                if (string.IsNullOrEmpty(code) || code.Length != 6)
                {
                    Console.WriteLine("[TOTP Test] ERROR: Code generation failed");
                    return;
                }
                
                // Test 3: Validate the same code
                bool isValid = totpService.ValidateCode(secret, code);
                Console.WriteLine($"[TOTP Test] Code validation: {isValid}");
                
                if (!isValid)
                {
                    Console.WriteLine("[TOTP Test] ERROR: Code validation failed");
                    return;
                }
                
                // Test 4: Try with invalid code
                bool isInvalidCodeValid = totpService.ValidateCode(secret, "123456");
                Console.WriteLine($"[TOTP Test] Invalid code validation: {isInvalidCodeValid}");
                
                // Test 5: QR Code generation test
                try
                {
                    var qrBytes = totpService.GenerateQrCode("test@example.com", secret);
                    Console.WriteLine($"[TOTP Test] QR Code generated successfully: {qrBytes.Length} bytes");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TOTP Test] QR Code generation failed: {ex.Message}");
                }
                
                Console.WriteLine("[TOTP Test] All basic tests completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TOTP Test] Test failed with exception: {ex.Message}");
            }
        }
    }
}