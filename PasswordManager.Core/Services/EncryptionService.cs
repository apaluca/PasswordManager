using PasswordManager.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Services
{
        public class EncryptionService : IEncryptionService
        {
                private readonly string _key;
                private readonly byte[] _iv = new byte[16] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

                public EncryptionService(string key)
                {
                        _key = key;
                }

                public string Encrypt(string plainText)
                {
                        using (Aes aes = Aes.Create())
                        {
                                aes.Key = Convert.FromBase64String(_key);
                                aes.IV = _iv;

                                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                                using (MemoryStream msEncrypt = new MemoryStream())
                                {
                                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                                        {
                                                swEncrypt.Write(plainText);
                                        }

                                        return Convert.ToBase64String(msEncrypt.ToArray());
                                }
                        }
                }

                public string Decrypt(string cipherText)
                {
                        using (Aes aes = Aes.Create())
                        {
                                aes.Key = Convert.FromBase64String(_key);
                                aes.IV = _iv;

                                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
                                        return srDecrypt.ReadToEnd();
                                }
                        }
                }

                public string GenerateKey()
                {
                        using (var aes = Aes.Create())
                        {
                                aes.GenerateKey();
                                return Convert.ToBase64String(aes.Key);
                        }
                }
        }
}
