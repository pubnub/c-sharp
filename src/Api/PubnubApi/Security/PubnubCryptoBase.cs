using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

#if NET35
using System.Security.Cryptography;
#else
using System.Security.Cryptography;
#endif

namespace PubnubApi
{
    public abstract class PubnubCryptoBase
    {
        private readonly string cipherKey;
        private readonly PNConfiguration config;
        protected PubnubCryptoBase(string cipher_key, PNConfiguration pubnubConfig)
        {
            this.cipherKey = cipher_key;
            this.config = pubnubConfig;
        }
        protected PubnubCryptoBase(string cipher_key)
        {
            this.cipherKey = cipher_key;
            this.config = null;
        }

        /// <summary>
        /// Computes the hash using the specified algo
        /// </summary>
        /// <returns>
        /// The hash.
        /// </returns>
        /// <param name='input'>
        /// Input string
        /// </param>
        /// <param name='algorithm'>
        /// Algorithm to use for Hashing
        /// </param>
        //private string ComputeHash(string input, HashAlgorithm algorithm)
        protected abstract string ComputeHashRaw(string input);


        protected string GetEncryptionKey()
        {
            //Compute Hash using the SHA256 
            string strKeySHA256HashRaw = ComputeHashRaw(this.cipherKey);
            //delete the "-" that appear after every 2 chars
            string strKeySHA256Hash = strKeySHA256HashRaw.Replace("-", "").Substring(0, 32);
            //convert to lower case
            return strKeySHA256Hash.ToLowerInvariant();
        }

        /**
         * EncryptOrDecrypt
         * 
         * Basic function for encrypt or decrypt a string
         * for encrypt type = true
         * for decrypt type = false
         */
        //private string EncryptOrDecrypt(bool type, string plainStr)
        protected abstract string EncryptOrDecrypt(bool type, string dataStr, bool dynamicIV);
        protected abstract byte[] EncryptOrDecrypt(bool type, byte[] dataBytes, bool dynamicIV);


        // encrypt string
        public string Encrypt(string plainText)
        {
            if (plainText == null || plainText.Length <= 0) { throw new ArgumentNullException("plainText"); }
            bool dynamicIV = (config != null && config.UseRandomInitializationVector);
            return EncryptOrDecrypt(true, plainText, dynamicIV);
        }

        public byte[] Encrypt(byte[] plainBytes)
        {
            if (plainBytes == null || plainBytes.Length <= 0) { throw new ArgumentNullException("plainBytes"); }
            bool dynamicIV = (config != null && config.UseRandomInitializationVector);
            return EncryptOrDecrypt(true, plainBytes, dynamicIV);
        }

        public byte[] Encrypt(byte[] plainBytes, bool file)
        {
            if (plainBytes == null || plainBytes.Length <= 0) { throw new ArgumentNullException("plainBytes"); }
            bool dynamicIV = file || (config != null && config.UseRandomInitializationVector);
            return EncryptOrDecrypt(true, plainBytes, dynamicIV);
        }

        // decrypt string
        public string Decrypt(string cipherText)
        {
            if (cipherText == null) { throw new ArgumentNullException("cipherText"); }
            bool dynamicIV = (config != null && config.UseRandomInitializationVector);
            return EncryptOrDecrypt(false, cipherText, dynamicIV);
        }

        public byte[] Decrypt(byte[] cipherBytes)
        {
            if (cipherBytes == null) { throw new ArgumentNullException("cipherBytes"); }
            bool dynamicIV = (config != null && config.UseRandomInitializationVector);
            return EncryptOrDecrypt(false, cipherBytes, dynamicIV);
        }

        public byte[] Decrypt(byte[] cipherBytes, bool file)
        {
            if (cipherBytes == null) { throw new ArgumentNullException("cipherBytes"); }
            bool dynamicIV = file || (config != null && config.UseRandomInitializationVector);
            return EncryptOrDecrypt(false, cipherBytes, dynamicIV);
        }

        /// <summary>
        /// Converts the upper case hex to lower case hex.
        /// </summary>
        /// <returns>The lower case hex.</returns>
        /// <param name="value">Hex Value.</param>
        public static string ConvertHexToUnicodeChars(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                }
            );
        }

        /// <summary>
        /// Encodes the non ASCII characters.
        /// </summary>
        /// <returns>
        /// The non ASCII characters.
        /// </returns>
        /// <param name='value'>
        /// Value.
        /// </param>
        protected static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public string PubnubAccessManagerSign(string key, string data)
        {
            string secret = key;
            string message = data;

            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage).Replace('+', '-').Replace('/', '_');
            }
        }

        public static byte[] PubnubAccessManagerSign(string key, byte[] dataBytes)
        {
            string secret = key;

            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = dataBytes;

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return hashmessage;
            }
        }

        public string GetHashRaw(string input)
        {
            return ComputeHashRaw(input);
        }
    }
}
