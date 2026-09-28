using System.Security.Cryptography;

namespace FileGenerationAPIApp.Services
{
    public class PasswordHasher: IPasswordHasher
    {
        private const int SaltSize = 16;       // 128 bits
        private const int HashSize = 32;       // 256 bits
        private const int Iterations = 600_000;
        public string HashPassword(string password)
        {            
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            // Format: iterations.salt.hash
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }

            try
            {
                string[] parts = passwordHash.Split('.');

                if (parts.Length != 3)
                    return false;

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] storedHash = Convert.FromBase64String(parts[2]);

                byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    storedHash.Length);

                return CryptographicOperations.FixedTimeEquals(
                    computedHash,
                    storedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
