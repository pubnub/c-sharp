using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PubnubApi.Security.Crypto.Common;

namespace PubnubApi.Security.Crypto.Cryptors
{
    public abstract class CryptorBase : ICryptor
    {
        protected const int IV_SIZE = 16;

        private readonly bool _useDynamicRandomIV;
        private readonly IPubnubLog _log;
        private readonly IPubnubUnitTest _unitTest;
        protected CryptorBase(string cipherKey, bool useDynamicRandomIV, IPubnubLog log, IPubnubUnitTest unitTest)
        {
            _useDynamicRandomIV = useDynamicRandomIV;
            _log = log;
            _unitTest = unitTest;
            CipherKey = cipherKey;
        }
        public string CipherKey { get; }
        protected byte[] GenerateRandomIV(bool useDynamicRandomIV)
        {
            int dataOffset = useDynamicRandomIV ? IV_SIZE : 0;
            if (_unitTest != null && _unitTest.IV != null && _unitTest.IV.Length == 16)
            {
                return _unitTest.IV;
            }
            return Util.InitializationVector(useDynamicRandomIV, dataOffset);
        }
        protected void Log(string message)
        {
            _log?.WriteToLog(message);
        }
        protected byte[] InternalEncrypt(bool cryptoHeader, byte[] dataBytes, byte[] ivBytes, byte[] keyBytes)
        {
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
                    if (cryptoHeader)
                    {
                        CryptorHeader header = new CryptorHeader(Identifier, outputBytes.Length);
                        byte[] headerBytes = header.ToBytes();
                        Log(string.Format(CultureInfo.InvariantCulture, "DateTime {0} Header = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), headerBytes.ToDisplayFormat()));
                        byte[] buffer = new byte[headerBytes.Length + ivBytes.Length + outputBytes.Length];
                        Buffer.BlockCopy(headerBytes, 0, buffer, 0, headerBytes.Length);
                        Buffer.BlockCopy(ivBytes    , 0, buffer, headerBytes.Length, ivBytes.Length);
                        Buffer.BlockCopy(outputBytes, 0, buffer, headerBytes.Length + ivBytes.Length, outputBytes.Length);

                        return buffer;
                    }

                    if (_useDynamicRandomIV)
                    {
                        byte[] buffer = new byte[ivBytes.Length + outputBytes.Length];
                        Buffer.BlockCopy(ivBytes, 0, buffer, 0, ivBytes.Length);
                        Buffer.BlockCopy(outputBytes, 0, buffer, ivBytes.Length, outputBytes.Length);
                        return buffer;
                    }

                    return outputBytes;
                }
            }
        }
        protected byte[] InternalDecrypt(byte[] dataBytes, byte[] ivBytes, byte[] keyBytes)
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
        public abstract string Identifier { get; }
        public abstract string Encrypt(string data);
        public abstract byte[] Encrypt(byte[] data);
        public abstract string Decrypt(string encryptedData);
        public abstract byte[] Decrypt(byte[] encryptedData);

        public abstract void EncryptFile(string sourceFile, string destinationFile);

        public abstract void DecryptFile(string sourceFile, string destinationFile);
    }
}
