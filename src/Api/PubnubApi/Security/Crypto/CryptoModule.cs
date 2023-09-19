using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubnubApi.Security.Crypto
{
    public class CryptoModule
    {
        private readonly ICryptor _cryptor;
        private readonly LegacyCryptor _fallbackCryptor;
        public CryptoModule(ICryptor cryptor, LegacyCryptor fallbackCryptor)
        {
            _cryptor = cryptor;
            _fallbackCryptor = fallbackCryptor;
        }
        public static LegacyCryptor CreateLegacyCryptor(string cipherKey, bool useDynamicIV, IPubnubLog log, IPubnubUnitTest unitTest)
        {
            return new LegacyCryptor(cipherKey, useDynamicIV, log, unitTest);
        }
        public static LegacyCryptor CreateLegacyCryptor(string cipherKey, bool useDynamicIV)
        {
            return new LegacyCryptor(cipherKey, useDynamicIV);
        }
        public static LegacyCryptor CreateLegacyCryptor(string cipherKey)
        {
            return new LegacyCryptor(cipherKey, true);
        }
        public static AesCbcCryptor CreateAesCbcCryptor(string cipherKey, IPubnubLog log, IPubnubUnitTest unitTest)
        {
            return new AesCbcCryptor(cipherKey, log, unitTest);
        }
        public static AesCbcCryptor CreateAesCbcCryptor(string cipherKey)
        {
            return new AesCbcCryptor(cipherKey);
        }
        public string Encrypt(string data)
        {
            return _cryptor.Encrypt(data);
        }
        public byte[] Encrypt(byte[] data)
        {
            return _cryptor.Encrypt(data);
        }
        public void EncryptFile(string sourceFile, string destinationFile)
        {
            _cryptor.EncryptFile(sourceFile, destinationFile);
        }
        public string Decrypt(string data)
        {
            if (data == null)
            {
                throw new ArgumentException("Input is null");
            }
            if (_cryptor is LegacyCryptor)
            {
                return _cryptor?.Decrypt(data);
            }
            CryptorHeader header = CryptorHeader.FromBytes(Convert.FromBase64String(data));
            if (header == null)
            {
                return _fallbackCryptor?.Decrypt(data);
            }
            if (!header.Identifier.SequenceEqual(_cryptor?.Identifier))
            {
                throw new PNException("unknown cryptor error");
            }
            return _cryptor?.Decrypt(data);
        }
        public byte[] Decrypt(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentException("Input is null");
            }
            if (_cryptor is LegacyCryptor)
            {
                return _cryptor?.Decrypt(data);
            }
            CryptorHeader header = CryptorHeader.FromBytes(data);
            if (header == null)
            {
                return _fallbackCryptor?.Decrypt(data);
            }
            if (!header.Identifier.SequenceEqual(_cryptor?.Identifier))
            {
                throw new PNException("unknown cryptor error");
            }
            return _cryptor?.Decrypt(data);
        }
        public void DecryptFile(string sourceFile, string destinationFile)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("sourceFile is not valid");
            }
            if (string.IsNullOrEmpty(destinationFile) || destinationFile.Length < 1)
            {
                throw new ArgumentException("destinationFile is not valid");
            }
            if (_cryptor is LegacyCryptor)
            {
                _cryptor?.DecryptFile(sourceFile, destinationFile);
                return;
            }
            CryptorHeader header = CryptorHeader.FromFile(sourceFile);
            if (header == null)
            {
                if (_fallbackCryptor == null)
                {
                    throw new PNException("unknown cryptor error");
                }
                _fallbackCryptor?.DecryptFile(sourceFile, destinationFile);
                return;
            }
            if (!header.Identifier.SequenceEqual(_cryptor?.Identifier))
            {
                throw new PNException("unknown cryptor error");
            }
            _cryptor?.DecryptFile(sourceFile, destinationFile);
        }
    }
}
