using System.Security.Cryptography;
using System.Text;

namespace RaceDay.Api.Services
{
    /// <summary>
    /// Simple salted SHA256 password hasher.
    /// Passwords are never stored in plain text.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Salt = "RaceDayPoE2026_SecureSalt";

        /// <summary>
        /// Hashes a plain-text password using SHA256 with a fixed application salt.
        /// </summary>
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var combined = password + Salt;
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifies a plain-text password against a stored hash.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }
    }
}