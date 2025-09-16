using System;
using System.Text;
using OtpNet;
using QRCoder;

namespace perimapp.Services
{
    public class TotpService
    {
        private const string Issuer = "PerimApp";

        // Génère une nouvelle clé secrète TOTP
        public string GenerateSecret()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        // Génère un QR code pour configurer l'authentificateur
        public byte[] GenerateQrCode(string userEmail, string secret)
        {
            var otpUri = $"otpauth://totp/{Uri.EscapeDataString(Issuer)}:{Uri.EscapeDataString(userEmail)}?secret={secret}&issuer={Uri.EscapeDataString(Issuer)}";
            
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(otpUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        // Valide un code TOTP
        public bool ValidateCode(string secret, string code)
        {
            try
            {
                var secretBytes = Base32Encoding.ToBytes(secret);
                var totp = new Totp(secretBytes);
                
                // Valide le code avec une fenêtre de tolérance (±1 step, soit ±30 secondes)
                return totp.VerifyTotp(code, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TotpService] Erreur validation TOTP : {ex.Message}");
                return false;
            }
        }

        // Génère un code TOTP pour test/debug
        public string GenerateCode(string secret)
        {
            try
            {
                var secretBytes = Base32Encoding.ToBytes(secret);
                var totp = new Totp(secretBytes);
                return totp.ComputeTotp();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TotpService] Erreur génération TOTP : {ex.Message}");
                return "";
            }
        }
    }
}