using OtpNet;
using PasswordManager.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Services
{
        public class SecurityService : ISecurityService
        {
                private const int SALT_SIZE = 16; // 128 bits
                private const int HASH_SIZE = 32; // 256 bits
                private const int ITERATIONS = 10000;

                public string HashPassword(string password)
                {
                        // Generate a random salt
                        byte[] salt = new byte[SALT_SIZE];
                        using (var rng = new RNGCryptoServiceProvider())
                        {
                                rng.GetBytes(salt);
                        }

                        // Create the Rfc2898DeriveBytes and get the hash value
                        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
                        {
                                byte[] hash = pbkdf2.GetBytes(HASH_SIZE);

                                // Combine the salt and password bytes for storage
                                byte[] hashBytes = new byte[SALT_SIZE + HASH_SIZE];
                                Array.Copy(salt, 0, hashBytes, 0, SALT_SIZE);
                                Array.Copy(hash, 0, hashBytes, SALT_SIZE, HASH_SIZE);

                                // Convert to base64 for storage
                                return Convert.ToBase64String(hashBytes);
                        }
                }

                public bool VerifyPassword(string password, string hashedPassword)
                {
                        // Convert base64 string to byte array
                        byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                        // Extract the salt and hash
                        byte[] salt = new byte[SALT_SIZE];
                        byte[] hash = new byte[HASH_SIZE];
                        Array.Copy(hashBytes, 0, salt, 0, SALT_SIZE);
                        Array.Copy(hashBytes, SALT_SIZE, hash, 0, HASH_SIZE);

                        // Compute the hash for the provided password
                        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
                        {
                                byte[] testHash = pbkdf2.GetBytes(HASH_SIZE);

                                // Compare the hashes
                                for (int i = 0; i < HASH_SIZE; i++)
                                {
                                        if (hash[i] != testHash[i])
                                                return false;
                                }
                                return true;
                        }
                }

                public string GenerateStrongPassword(int length = 16)
                {
                        const string upperCase = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
                        const string lowerCase = "abcdefghijkmnopqrstuvwxyz";
                        const string digits = "23456789";
                        const string special = "@#$%^&*";

                        StringBuilder password = new StringBuilder();
                        using (var rng = new RNGCryptoServiceProvider())
                        {
                                // Ensure at least one character from each category
                                password.Append(GetRandomChar(upperCase, rng));
                                password.Append(GetRandomChar(lowerCase, rng));
                                password.Append(GetRandomChar(digits, rng));
                                password.Append(GetRandomChar(special, rng));

                                // Fill the rest with random characters from all categories
                                string allChars = upperCase + lowerCase + digits + special;
                                while (password.Length < length)
                                {
                                        password.Append(GetRandomChar(allChars, rng));
                                }

                                // Shuffle the password
                                return new string(password.ToString().ToCharArray()
                                    .OrderBy(x => GetNextInt32(rng))
                                    .ToArray());
                        }
                }

                public string GenerateTwoFactorKey()
                {
                        byte[] key = new byte[20]; // 160 bits
                        using (var rng = new RNGCryptoServiceProvider())
                        {
                                rng.GetBytes(key);
                        }
                        return Base32Encoding.ToString(key);
                }

                public bool ValidateTwoFactorCode(string secretKey, string code)
                {
                        try
                        {
                                var totp = new Totp(Base32Encoding.ToBytes(secretKey));
                                return totp.VerifyTotp(code, out long timeStepMatched);
                        }
                        catch
                        {
                                return false;
                        }
                }

                private char GetRandomChar(string characters, RNGCryptoServiceProvider rng)
                {
                        return characters[GetNextInt32(rng) % characters.Length];
                }

                private int GetNextInt32(RNGCryptoServiceProvider rng)
                {
                        byte[] buffer = new byte[4];
                        rng.GetBytes(buffer);
                        return BitConverter.ToInt32(buffer, 0) & int.MaxValue;
                }
        }
}
