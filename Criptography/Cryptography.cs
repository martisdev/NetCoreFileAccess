using System;
using System.Security.Cryptography;

namespace NetCoreFileAccess.Criptography
{
    public static class Cryptography
    {
        #region CONSTS
        // IV is still 16 bytes or 128 bits for AES 128, 192 and 256. 
        public const int IV_LENGTH = 16;

        public const int RAMDOM_LENGTH = 10;

        public const int USER_LENGTH = 32;
        #endregion

        #region FIELDS

        // Change the declaration of KEY_IV to be non-nullable and initialize it directly.
        public static byte[] KEY_IV = new byte[IV_LENGTH];

        #endregion


        #region METHODS Publics

        /// <summary>
        /// AES-CBC encrypt with PKCS7 padding. Uses the static KEY_IV as IV.
        /// Returns ciphertext (IV is NOT prefixed).
        /// Returns null on error.
        /// </summary>
        public static byte[]? AESEncrypt(byte[] plainBytes, byte[] keyBytes)
        {
            try
            {
                using Aes aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Ensure key length is valid (16/24/32). If not, expand/truncate deterministically.
                aes.Key = NormalizeKey(keyBytes, aes.KeySize / 8);

                // Use existing KEY_IV (expect GenerateIV called before). If empty, create one for this call.
                if (KEY_IV == null || KEY_IV.Length != IV_LENGTH)
                {
                    return null; // IV must be set before encryption, cannot generate a new one here as it won't match the decryption IV    
                }
                aes.IV = KEY_IV;

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// AES-CBC decrypt with PKCS7 padding. Uses the static KEY_IV as IV.
        /// Returns plaintext bytes or null on error.
        /// </summary>
        public static byte[]? AESDecrypt(byte[] cipherBytes, byte[] keyBytes)
        {
            try
            {
                using Aes aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = NormalizeKey(keyBytes, aes.KeySize / 8);
                if (KEY_IV == null ||  KEY_IV.Length != IV_LENGTH)
                {
                    return null; // IV must be set before decryption, cannot generate a new one here as it won't match the encryption IV                    
                }
                aes.IV = KEY_IV;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                return decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            }
            catch
            {
                return null;
            }
        }
        private static byte[] NormalizeKey(byte[] keyBytes, int requiredLength)
        {
            if (keyBytes == null) throw new ArgumentNullException(nameof(keyBytes));
            if (keyBytes.Length == requiredLength) return keyBytes;

            var normalized = new byte[requiredLength];
            if (keyBytes.Length >= requiredLength)
            {
                Array.Copy(keyBytes, normalized, requiredLength);
            }
            else
            {
                // If shorter, copy and fill the rest with repeated bytes from the key (simple deterministic expansion)
                Array.Copy(keyBytes, normalized, keyBytes.Length);
                for (int i = keyBytes.Length; i < requiredLength; i++)
                {
                    normalized[i] = keyBytes[i % keyBytes.Length];
                }
            }
            return normalized;
        }

        public static void GenerateIV()
        {
            //Set a new IV for this session
            KEY_IV = GererateRamdomKEY(IV_LENGTH);            
        }

        public static byte[] GererateRamdomKEY(int mLength)
        {
            using (var random = RandomNumberGenerator.Create())
            {
                var key = new byte[mLength];
                random.GetBytes(key);
                return key;
            }            
        }

        #endregion
    }
}