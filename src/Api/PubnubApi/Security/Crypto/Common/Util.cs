using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PubnubApi.Security.Crypto.Common
{
    public class Util
    {
        internal static byte[] InitializationVector(bool useDynamicRandomIV, int ivSize)
        {
            if (!useDynamicRandomIV)
            {
                return Encoding.UTF8.GetBytes("0123456789012345");
            }
            
            byte[] iv = new byte[ivSize];
            var rng = RandomNumberGenerator.Create();
            try
            {
                rng.GetBytes(iv);
            }
            finally 
            {
                var disposable = rng as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return iv;
        }

        internal static byte[] ComputeSha256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                return sha256.ComputeHash(inputBytes);
            }
        }

        internal static byte[] GetLegacyEncryptionKey(string input)
        {
            //Compute Hash using the SHA256 
            string strKeySHA256HashRaw = ComputeHashRaw(input);
            //delete the "-" that appear after every 2 chars
            string strKeySHA256Hash = strKeySHA256HashRaw.Replace("-", "").Substring(0, 32);
            //convert to lower case
            return Encoding.UTF8.GetBytes(strKeySHA256Hash.ToLowerInvariant());
        }
        internal static byte[] GetEncryptionKeyBytes(string input)
        {
            return ComputeHash(input);
        }

        private static byte[] ComputeHash(string input)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using(SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(inputBytes);
            }
        }
        private static string ComputeHashRaw(string input)
        {
            HashAlgorithm algorithm = SHA256.Create();
            Byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes);
        }

        internal static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4", CultureInfo.InvariantCulture);
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
