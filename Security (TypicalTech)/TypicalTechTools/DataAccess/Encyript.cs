using System;
using System.IO;
using System.Text;
using CryptoNet;

namespace TypicalTechTools.DataAccess
{
    public class Encrypt
    {
        private static readonly CryptoNetAes cryptoNet = new CryptoNetAes();

        public static byte[] EncryptString(string plainText)
        {
            return cryptoNet.EncryptFromString(plainText);
        }

        public static string DecryptString(byte[] encryptedText)
        {
            return cryptoNet.DecryptToString(encryptedText);
        }
    }
}
