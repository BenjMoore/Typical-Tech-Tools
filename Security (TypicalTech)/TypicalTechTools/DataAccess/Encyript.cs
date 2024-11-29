using AngleSharp.Dom;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

   namespace TypicalTechTools.DataAccess
   {
    public class Encrypt
    {
        string _secretKey;
      public Encrypt(IConfiguration config)
        {
            _secretKey = config["SecretKey"];
        }
        public byte[] EncryptByteArray(byte[] fileData)
        {
            // Create an AES algorithm class (This will generate our Initialization Vector on creation)
            using (var aesAlg = Aes.Create())
            {
                // Convert our secret key to a byte array
                aesAlg.Key = System.Text.Encoding.UTF8.GetBytes(_secretKey);

                // Create an encryptor using our key and initialization vector (IV) to encrypt the data
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Final memory stream to hold the new file data
                using (var memStream = new MemoryStream())
                {
                    // Add the IV to the start of the byte array before we add the file data
                    memStream.Write(aesAlg.IV, 0, 16);

                    // Create a crypto stream which will pass the data back to the memory stream using our encryptor
                    using (var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                    {
                        // Pass our file data through the crypto stream
                        cryptoStream.Write(fileData, 0, fileData.Length);
                        cryptoStream.FlushFinalBlock();

                        // Return the final memory stream data
                        return memStream.ToArray();
                    }
                }
            }
        }


        public byte[] DecryptByteArray(byte[] encryptedData)
        {
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = System.Text.Encoding.UTF8.GetBytes(_secretKey);
            byte[] IV = new byte[16];
                Array.Copy(encryptedData, IV, IV.Length);
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, IV);
                using (var memStream = new MemoryStream())
                {
                    using(var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(encryptedData, IV.Length, encryptedData.Length - IV.Length);
                        cryptoStream.FlushFinalBlock();
                        return memStream.ToArray();
                    }
                }
        }
        }
    }
}
