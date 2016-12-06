using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

#if NET35
using System.Security.Cryptography;
#else
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
#endif

namespace PubnubApi
{
    public abstract class PubnubCryptoBase
    {
        private string cipherKey = "";
        public PubnubCryptoBase(string cipher_key)
        {
            this.cipherKey = cipher_key;
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
            return strKeySHA256Hash.ToLower();
        }

        /**
         * EncryptOrDecrypt
         * 
         * Basic function for encrypt or decrypt a string
         * for encrypt type = true
         * for decrypt type = false
         */
        //private string EncryptOrDecrypt(bool type, string plainStr)
        protected abstract string EncryptOrDecrypt(bool type, string plainStr);


        // encrypt string
        public string Encrypt(string plainText)
        {
            if (plainText == null || plainText.Length <= 0) throw new ArgumentNullException("plainText");

            return EncryptOrDecrypt(true, plainText);
        }

        // decrypt string
        public string Decrypt(string cipherText)
        {
            if (cipherText == null) throw new ArgumentNullException("cipherText");

            return EncryptOrDecrypt(false, cipherText);
        }

        /// <summary>
        /// Converts the upper case hex to lower case hex.
        /// </summary>
        /// <returns>The lower case hex.</returns>
        /// <param name="value">Hex Value.</param>
        public static string ConvertHexToUnicodeChars(string value)
        {
            //if(;
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
        protected string EncodeNonAsciiCharacters(string value)
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

#if NET35
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage).Replace('+', '-').Replace('/', '_');
            }
#else

            //http://mycsharp.de/wbb2/thread.php?postid=3550104
            KeyParameter paramKey = new KeyParameter(keyByte);
            IMac mac = MacUtilities.GetMac("HMac-SHA256");
            mac.Init(paramKey);
            mac.Reset();
            mac.BlockUpdate(messageBytes, 0, messageBytes.Length);
            byte[] hashmessage = new byte[mac.GetMacSize()];
            mac.DoFinal(hashmessage, 0);
            return Convert.ToBase64String(hashmessage).Replace('+', '-').Replace('/', '_');
#endif
        }


    }
}
