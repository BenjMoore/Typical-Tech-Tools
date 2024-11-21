using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TypicalTechTools.DataAccess
{
    public class Encrypt
    {
        private static readonly string KeyFilePath = "keyfile.bin"; // Path to store the encrypted key and IV
        private static readonly string Password = "StrongPassword123!"; // Use a secure password or passphrase

        /// <summary>
        /// Encrypts a file using AES encryption.
        /// </summary>
        public static void EncryptFile(string inputFilePath, string outputFilePath)
        {
            EnsureKeyAndIV();

            (byte[] key, byte[] iv) = LoadKeyAndIV();

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create))
                using (CryptoStream cryptoStream = new CryptoStream(outputFileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (FileStream inputFileStream = new FileStream(inputFilePath, FileMode.Open))
                {
                    inputFileStream.CopyTo(cryptoStream);
                }
            }
        }

        /// <summary>
        /// Decrypts an encrypted file using AES encryption.
        /// </summary>
        public static void DecryptFile(string inputFilePath, string outputFilePath)
        {
            EnsureKeyAndIV();

            (byte[] key, byte[] iv) = LoadKeyAndIV();

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (FileStream inputFileStream = new FileStream(inputFilePath, FileMode.Open))
                using (CryptoStream cryptoStream = new CryptoStream(inputFileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create))
                {
                    cryptoStream.CopyTo(outputFileStream);
                }
            }
        }

        /// <summary>
        /// Ensures the key and IV exist, generating and saving them if not already present.
        /// </summary>
        private static void EnsureKeyAndIV()
        {
            if (!File.Exists(KeyFilePath))
            {
                using (Aes aes = Aes.Create())
                {
                    SaveKeyAndIV(aes.Key, aes.IV);
                }
            }
        }

        /// <summary>
        /// Loads the key and IV from the key file.
        /// </summary>
        private static (byte[] key, byte[] iv) LoadKeyAndIV()
        {
            byte[] encryptedKeyAndIV = File.ReadAllBytes(KeyFilePath);

            using (Aes aes = Aes.Create())
            {
                using (ICryptoTransform decryptor = aes.CreateDecryptor(DeriveKey(), new byte[16])) // 16-byte zero IV for simplicity
                using (MemoryStream ms = new MemoryStream(encryptedKeyAndIV))
                using (CryptoStream cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (BinaryReader reader = new BinaryReader(cryptoStream))
                {
                    byte[] key = reader.ReadBytes(32); // 256-bit key
                    byte[] iv = reader.ReadBytes(16);  // 128-bit IV
                    return (key, iv);
                }
            }
        }

        /// <summary>
        /// Saves the key and IV to a secure key file.
        /// </summary>
        private static void SaveKeyAndIV(byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            using (ICryptoTransform encryptor = aes.CreateEncryptor(DeriveKey(), new byte[16])) // 16-byte zero IV for simplicity
            {
                // Create the MemoryStream outside the using block so it can be accessed later
                using (var ms = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var writer = new BinaryWriter(cryptoStream))
                    {
                        writer.Write(key);
                        writer.Write(iv);
                    }

                    // Write the encrypted key and IV to the file after closing the streams
                    File.WriteAllBytes(KeyFilePath, ms.ToArray());
                }
            }
        }


        /// <summary>
        /// Derives a key from the password for encrypting/decrypting the key file.
        /// </summary>
        private static byte[] DeriveKey()
        {
            using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(Password, Encoding.UTF8.GetBytes("SaltValue"), 100000))
            {
                return pdb.GetBytes(32); // 256-bit key
            }
        }
    }
}
