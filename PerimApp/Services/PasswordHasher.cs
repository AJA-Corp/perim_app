using System;
using Isopoh.Cryptography.Argon2;

namespace perimapp.Services
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            return Argon2.Hash(password);
        }

        /*
        public static bool VerifyPassword(string hashedPassword, string enteredPassword)
        {
            return Argon2.Verify(hashedPassword, enteredPassword);
        }
        */
    }
}
