using PubnubApi.Security.Crypto.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace PubnubApi.Security.Crypto.Cryptors
{
    public class AesCbcCryptoAlgorithm : ICryptoAlgorithm
    {
        /// AES cipher block size.
        private const int IV_SIZE = 16;

        private readonly string _cipherKey;
        private readonly bool _useRandomIV = true;
        private readonly IPubnubLog _log;
        private readonly IPubnubUnitTest _unitTest;
        public AesCbcCryptoAlgorithm(string cipherKey, IPubnubLog log, IPubnubUnitTest unitTest)
        {
            _cipherKey = cipherKey;
            _log = log;
            _unitTest = unitTest;
        }
        public AesCbcCryptoAlgorithm(string cipherKey): this(cipherKey, null, null)
        {
        }
        public byte[] Identifier { get; } = Encoding.ASCII.GetBytes("CRIV");

        private static string EncodeNonAsciiCharacters(string value)
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
        public string Encrypt(string data)
        {
            if (data == null)
            {
                throw new ArgumentException("Input is null");
            }
            string input = EncodeNonAsciiCharacters(data);
            byte[] dataBytes = Encoding.UTF8.GetBytes(input);

            byte[] ivBytes = Util.InitializationVector(_useRandomIV, IV_SIZE);
            if (_unitTest != null && _unitTest.IV != null)
            {
                ivBytes = _unitTest.IV;
            }

            byte[] keyBytes = Util.GetEncryptionKeyBytes(_cipherKey);
            try
            {
                byte[] encryptedBytes = InternalEncrypt(dataBytes, ivBytes, keyBytes);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch(Exception ex)
            {
                throw new PNException("Encryption error", ex);
            }
        }
        public byte[] Encrypt(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentException("Input is null");
            }

            int dataOffset = _useRandomIV ? IV_SIZE : 0;
            byte[] ivBytes = Util.InitializationVector(_useRandomIV, dataOffset);
            if (_unitTest != null && _unitTest.IV != null && _unitTest.IV.Length == 16)
            {
                ivBytes = _unitTest.IV;
            }

            byte[] keyBytes = Util.GetEncryptionKeyBytes(_cipherKey);
            try
            {
                return InternalEncrypt(data, ivBytes, keyBytes);
            }
            catch(Exception ex)
            {
                throw new PNException("Encryption error", ex);
            }
        }
        private byte[] InternalEncrypt(byte[] dataBytes, byte[] ivBytes, byte[] keyBytes)
        {
            try
            {
                if (_log != null)
                {
                    LoggingMethod.WriteToLog(_log, string.Format(CultureInfo.InvariantCulture, "DateTime {0} IV = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ivBytes.ToDisplayFormat()), PNLogVerbosity.BODY);
                }
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.IV = ivBytes;
                    aesAlg.Key = keyBytes;

                    using (ICryptoTransform crypto = aesAlg.CreateEncryptor())
                    {
                        byte[] outputBytes = crypto.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                        CryptorHeader header = new CryptorHeader(Identifier, outputBytes.Length);
                        byte[] headerBytes = header.ToBytes();
                        if (_log != null)
                        {
                            LoggingMethod.WriteToLog(_log, string.Format(CultureInfo.InvariantCulture, "DateTime {0} Header = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), headerBytes.ToDisplayFormat()), PNLogVerbosity.BODY);
                        }
                        byte[] buffer = new byte[headerBytes.Length + ivBytes.Length + outputBytes.Length];
                        Buffer.BlockCopy(headerBytes, 0, buffer, 0, headerBytes.Length);
                        Buffer.BlockCopy(ivBytes    , 0, buffer, headerBytes.Length, ivBytes.Length);
                        Buffer.BlockCopy(outputBytes, 0, buffer, headerBytes.Length + ivBytes.Length, outputBytes.Length);

                        return buffer;
                    }
                }
            }
            catch(Exception ex)
            {
                throw new PNException("Encryption error", ex);
            }
        }
        public string Decrypt(string encryptedData)
        {
            if (encryptedData == null)
            {
                throw new ArgumentException("Input is null");
            }
            
            byte[] dataBytes;
            byte[] ivBytes;
            try
            {
                dataBytes = Convert.FromBase64String(encryptedData);
                ivBytes = dataBytes.Take(16).ToArray();
                dataBytes = dataBytes.Skip(16).ToArray();
            }
            catch(Exception ex)
            {
                throw new PNException("Base64 conversion error", ex);
            }
            try
            {
                byte[] keyBytes = Util.GetEncryptionKeyBytes(_cipherKey);
                byte[] decryptedBytes = InternalDecrypt(dataBytes, ivBytes, keyBytes);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch(Exception ex)
            {
                throw new PNException("Decryption error", ex);
            }
        }
        public byte[] Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null)
            {
                throw new ArgumentException("Input is null");
            }
            
            try
            {
                byte[] ivBytes = encryptedData.Take(16).ToArray();
                byte[] dataBytes = encryptedData.Skip(16).ToArray();
                byte[] keyBytes = Util.GetEncryptionKeyBytes(_cipherKey);
                return InternalDecrypt(dataBytes, ivBytes, keyBytes);
            }
            catch(Exception ex)
            {
                throw new PNException("Decryption error", ex);
            }
        }
        private byte[] InternalDecrypt(byte[] dataBytes, byte[] ivBytes, byte[] keyBytes)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.IV = ivBytes;
                    aesAlg.Key = keyBytes;

                    using(ICryptoTransform decrypto = aesAlg.CreateDecryptor())
                    {
                        byte[] buffer = decrypto.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                        return buffer;
                    }
                }
                
            }
            catch(Exception ex)
            {
                throw new PNException("Decryption error", ex);
            }
        }
    }
}
