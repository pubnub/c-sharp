using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PubnubApi.Security.Crypto.Common
{
    public class Util
    {
        internal static byte[] InitializationVector(bool useRandomIV, int aesBlockSize)
        {
            if (useRandomIV)
            {
                #if NET35
                byte[] iv = new byte[aesBlockSize];
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
                #else
                using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
                {
                    byte[] iv = new byte[aesBlockSize];
                    rngCsp.GetBytes(iv);
                    return iv;
                }
                #endif

            }
            else
            {
                return Encoding.UTF8.GetBytes("0123456789012345");
            }
        }

        internal static byte[] ComputeSha256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                return sha256.ComputeHash(inputBytes);
            }
        }

        internal static string GetLegacyEncryptionKey(string input)
        {
            //Compute Hash using the SHA256 
            string strKeySHA256HashRaw = ComputeHashRaw(input);
            //delete the "-" that appear after every 2 chars
            string strKeySHA256Hash = strKeySHA256HashRaw.Replace("-", "").Substring(0, 32);
            //convert to lower case
            return strKeySHA256Hash.ToLowerInvariant();
        }
        internal static string GetEncryptionKey(string input)
        {
            //Compute Hash using the SHA256 
            string strKeySHA256HashRaw = ComputeHashRaw(input);
            //delete the "-" that appear after every 2 chars
            string strKeySHA256Hash = strKeySHA256HashRaw.Replace("-", "");
            //convert to lower case
            return strKeySHA256Hash.ToLowerInvariant();
        }

        private static string ComputeHashRaw(string input)
        {
            HashAlgorithm algorithm = SHA256.Create();
            Byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes);
        }
    }
}
