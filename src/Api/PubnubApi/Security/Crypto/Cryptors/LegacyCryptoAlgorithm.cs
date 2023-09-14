using PubnubApi.Security.Crypto.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PubnubApi.Security.Crypto.Cryptors
{
    public class LegacyCryptoAlgorithm : ICryptoAlgorithm
    {
        /// AES cipher block size.
        private const int IV_SIZE = 16;

        private static readonly byte[] _identifier = new byte[] { };

        private readonly string _cipherKey;
        private readonly bool _useRandomIV;
        private readonly IPubnubLog _log;
        private readonly IPubnubUnitTest _unitTest;
        public LegacyCryptoAlgorithm(string cipherKey, bool useRandomIV, IPubnubLog log, IPubnubUnitTest unitTest)
        {
            _cipherKey = cipherKey;
            _useRandomIV = useRandomIV;
            _log = log;
            _unitTest = unitTest;
        }
        public LegacyCryptoAlgorithm(string cipherKey, bool useRandomIV, IPubnubLog log): this(cipherKey, useRandomIV, log, null)
        {
        }
        public LegacyCryptoAlgorithm(string cipherKey, bool useRandomIV): this(cipherKey, useRandomIV, null, null)
        {
        }
        public LegacyCryptoAlgorithm(string cipherKey): this(cipherKey, true, null, null)
        {
        }
        public byte[] Identifier => _identifier;
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

            int dataOffset = _useRandomIV ? IV_SIZE : 0;
            byte[] ivBytes = Util.InitializationVector(_useRandomIV, dataOffset);
            if (_unitTest != null && _unitTest.IV != null && _unitTest.IV.Length == 16)
            {
                ivBytes = _unitTest.IV;
            }

            string keyString = Util.GetLegacyEncryptionKey(_cipherKey);
            byte[] encryptedBytes = InternalEncrypt(dataBytes, ivBytes, keyString);
            return Convert.ToBase64String(encryptedBytes);
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

            string keyString = Util.GetLegacyEncryptionKey(_cipherKey);
            try
            {
                return InternalEncrypt(data, ivBytes, keyString);
            }
            catch(Exception ex)
            {
                throw new PNException("Encryption error", ex);
            }
        }
        private byte[] InternalEncrypt(byte[] dataBytes, byte[] ivBytes, string keyString)
        {
            try
            {
                if (_log != null)
                {
                    LoggingMethod.WriteToLog(_log, string.Format(CultureInfo.InvariantCulture, "DateTime {0} IV = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ivBytes.ToDisplayFormat()), PNLogVerbosity.BODY);
                }
                byte[] buffer;
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.IV = ivBytes;
                    aesAlg.Key = Encoding.UTF8.GetBytes(keyString);

                    using (ICryptoTransform crypto = aesAlg.CreateEncryptor())
                    {
                        byte[] outputBytes = crypto.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                        if (_useRandomIV)
                        {
                            buffer = new byte[ivBytes.Length + outputBytes.Length];
                            Buffer.BlockCopy(ivBytes, 0, buffer, 0, ivBytes.Length);
                            Buffer.BlockCopy(outputBytes, 0, buffer, ivBytes.Length, outputBytes.Length);
                            return buffer;
                        }
                        else
                        {
                            return outputBytes;
                        }
                    }
                }
            }
            catch (Exception ex)
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
            try
            {
                dataBytes = Convert.FromBase64String(encryptedData);
            }
            catch(Exception ex)
            {
                throw new PNException("Base64 conversion error", ex);
            }
            byte[] ivBytes = _useRandomIV ? dataBytes.Take(16).ToArray() : Encoding.UTF8.GetBytes("0123456789012345");
            dataBytes = _useRandomIV ? dataBytes.Skip(16).ToArray() : dataBytes;

            
            string keyString = Util.GetLegacyEncryptionKey(_cipherKey);

            try
            {
                byte[] decryptedBytes = InternalDecrypt(dataBytes, ivBytes, keyString);
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
                byte[] ivBytes = _useRandomIV ? encryptedData.Take(16).ToArray() : Encoding.UTF8.GetBytes("0123456789012345");
                byte[] dataBytes = _useRandomIV ? encryptedData.Skip(16).ToArray() : encryptedData;
                string keyString = Util.GetLegacyEncryptionKey(_cipherKey);
                return InternalDecrypt(dataBytes, ivBytes, keyString);
            }
            catch(Exception ex)
            {
                throw new PNException("Decryption error", ex);
            }
        }
        private byte[] InternalDecrypt(byte[] dataBytes, byte[] ivBytes, string keyString)
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
                    aesAlg.Key = System.Text.Encoding.UTF8.GetBytes(keyString);

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
