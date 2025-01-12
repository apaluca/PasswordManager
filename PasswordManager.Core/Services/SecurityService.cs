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
                private const int SALT_SIZE = 16;
                private const int HASH_SIZE = 32;
                private const int ITERATIONS = 10000;

                public string HashPassword(string password)
                {
                        byte[] salt = new byte[SALT_SIZE];
                        using (var rng = new RNGCryptoServiceProvider())
                        {
                                rng.GetBytes(salt);
                        }

                        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
                        {
                                byte[] hash = pbkdf2.GetBytes(HASH_SIZE);
                                byte[] hashBytes = new byte[SALT_SIZE + HASH_SIZE];
                                Array.Copy(salt, 0, hashBytes, 0, SALT_SIZE);
                                Array.Copy(hash, 0, hashBytes, SALT_SIZE, HASH_SIZE);

                                return Convert.ToBase64String(hashBytes);
                        }
                }

                public bool VerifyPassword(string password, string hashedPassword)
                {
                        try
                        {
                                byte[] hashBytes = Convert.FromBase64String(hashedPassword);
                                byte[] salt = new byte[SALT_SIZE];
                                Array.Copy(hashBytes, 0, salt, 0, SALT_SIZE);

                                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
                                {
                                        byte[] hash = pbkdf2.GetBytes(HASH_SIZE);
                                        for (int i = 0; i < HASH_SIZE; i++)
                                        {
                                                if (hashBytes[i + SALT_SIZE] != hash[i])
                                                        return false;
                                        }
                                        return true;
                                }
                        }
                        catch (Exception)
                        {
                                return false;
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
                                // Ensure at least one of each type
                                password.Append(GetRandomChar(upperCase, rng));
                                password.Append(GetRandomChar(lowerCase, rng));
                                password.Append(GetRandomChar(digits, rng));
                                password.Append(GetRandomChar(special, rng));

                                // Fill remaining length with random chars
                                string allChars = upperCase + lowerCase + digits + special;
                                while (password.Length < length)
                                {
                                        password.Append(GetRandomChar(allChars, rng));
                                }

                                // Shuffle the password
                                char[] array = password.ToString().ToCharArray();
                                int n = array.Length;
                                while (n > 1)
                                {
                                        byte[] box = new byte[1];
                                        do rng.GetBytes(box);
                                        while (!(box[0] < n * (Byte.MaxValue / n)));
                                        int k = (box[0] % n);
                                        n--;
                                        char temp = array[n];
                                        array[n] = array[k];
                                        array[k] = temp;
                                }
                                return new string(array);
                        }
                }

                private char GetRandomChar(string characters, RNGCryptoServiceProvider rng)
                {
                        byte[] randomNumber = new byte[1];
                        rng.GetBytes(randomNumber);
                        return characters[randomNumber[0] % characters.Length];
                }

                public string GenerateTwoFactorKey()
                {
                        // Generate a random 20-byte (160-bit) secret key
                        var secretKey = KeyGeneration.GenerateRandomKey(20);
                        // Convert to Base32 string for storage and QR code generation
                        return Base32Encoding.ToString(secretKey);
                }

                public bool ValidateTwoFactorCode(string secretKey, string code)
                {
                        try
                        {
                                // Convert the Base32 secret back to bytes
                                var keyBytes = Base32Encoding.ToBytes(secretKey);

                                // Create new TOTP instance
                                var totp = new Totp(keyBytes);

                                // Create a verification window that allows 1 step before and after
                                var window = new VerificationWindow(previous: 1, future: 1);

                                // Verify the code with the specified window
                                return totp.VerifyTotp(code, out long timeStepMatched, window);
                        }
                        catch (Exception)
                        {
                                // If any error occurs (invalid key format, etc.), return false
                                return false;
                        }
                }

                public string GetTwoFactorQrCodeUri(string secretKey, string username, string issuer = "PasswordManager")
                {
                        // Create an otpauth URI that can be used to generate QR codes
                        // Format: otpauth://totp/{issuer}:{username}?secret={secret}&issuer={issuer}
                        var normalizedIssuer = Uri.EscapeDataString(issuer);
                        var normalizedUsername = Uri.EscapeDataString(username);

                        return $"otpauth://totp/{normalizedIssuer}:{normalizedUsername}?secret={secretKey}&issuer={normalizedIssuer}";
                }

                public string GenerateCurrentTotpCode(string secretKey)
                {
                        try
                        {
                                var keyBytes = Base32Encoding.ToBytes(secretKey);
                                var totp = new Totp(keyBytes);
                                return totp.ComputeTotp();
                        }
                        catch (Exception)
                        {
                                return null;
                        }
                }

                public int GetRemainingTotpSeconds()
                {
                        // TOTP tokens typically change every 30 seconds
                        const int step = 30;
                        var delta = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                        return step - (int)(delta % step);
                }

                public string GenerateHashForTest(string password)
                {
                        var hash = HashPassword(password);
                        Console.WriteLine($"Generated hash for '{password}': {hash}");
                        return hash;
                }
        }
}
