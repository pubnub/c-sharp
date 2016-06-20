using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
            string strKeySHA256Hash = (strKeySHA256HashRaw.Replace("-", "")).Substring(0, 32);
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

        //md5 used for AES encryption key
        /*private static byte[] Md5(string cipherKey)
		{
			MD5 obj = new MD5CryptoServiceProvider();
			#if (SILVERLIGHT || WINDOWS_PHONE)
			byte[] data = Encoding.UTF8.GetBytes(cipherKey);
			#else
			byte[] data = Encoding.Default.GetBytes(cipherKey);
			#endif
			return obj.ComputeHash(data);
		}*/

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
#if (USE_JSONFX || USE_JSONFX_FOR_UNITY)
			value = ConvertHexToUnicodeChars(value);
#endif

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

#if NETFX_CORE
            var hmacsha256 = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            IBuffer valueBuffer = CryptographicBuffer.ConvertStringToBinary(message, BinaryStringEncoding.Utf8);
            IBuffer buffKeyMaterial = CryptographicBuffer.ConvertStringToBinary(secret, BinaryStringEncoding.Utf8);

            CryptographicKey cryptographicKey = hmacsha256.CreateKey(buffKeyMaterial);

            // Sign the key and message together.
            IBuffer bufferProtected = CryptographicEngine.Sign(cryptographicKey, valueBuffer);

            DataReader dataReader = DataReader.FromBuffer(bufferProtected);
            byte[] hashmessage = new byte[bufferProtected.Length];
            dataReader.ReadBytes(hashmessage);

            return Convert.ToBase64String(hashmessage).Replace('+', '-').Replace('/', '_');
#else
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage).Replace('+', '-').Replace('/', '_');
            }
#endif
        }


    }
}
