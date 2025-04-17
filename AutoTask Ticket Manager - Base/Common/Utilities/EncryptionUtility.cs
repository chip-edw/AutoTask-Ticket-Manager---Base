using System.Security.Cryptography;
using System.Text;

namespace AutoTaskTicketManager_Base.Common.Utilities
{
    public static class EncryptionUtility
    {
        // You should keep this secure! Maybe load from environment variable later.
        private static readonly string _encryptionKey = "SuperSecretEncryptionKey123!";

        public static string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                var key = GetKeyBytes(_encryptionKey);
                aesAlg.Key = key;
                aesAlg.GenerateIV(); // Random IV for every encryption
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using var msEncrypt = new MemoryStream();
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length); // Prepend IV to the ciphertext

                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }

        public static string Decrypt(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                var key = GetKeyBytes(_encryptionKey);
                aesAlg.Key = key;

                // Extract IV from the beginning
                byte[] iv = new byte[aesAlg.BlockSize / 8];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                var cipher = new byte[fullCipher.Length - iv.Length];
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using var msDecrypt = new MemoryStream(cipher);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
        }

        private static byte[] GetKeyBytes(string key)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(key)); // Always 256 bits
        }
    }
}
