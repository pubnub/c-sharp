using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubnubApi.Security.Crypto
{
    public class CryptoModule
    {
        private readonly AesCbcCryptor _cryptor;
        private readonly LegacyCryptor _fallbackCryptor;
        public CryptoModule(AesCbcCryptor cryptor, LegacyCryptor fallbackCryptor)
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
        public string Decrypt(string data)
        {
            if (data == null)
            {
                throw new ArgumentException("Input is null");
            }
            CryptorHeader header = CryptorHeader.FromBytes(Convert.FromBase64String(data));
            if (header == null)
            {
                return _fallbackCryptor?.Decrypt(data);
            }
            if (!header.Identifier.SequenceEqual(_cryptor?.Identifier))
            {
                throw new PNException("CryptorHeader mismatch");
            }
            return _cryptor?.Decrypt(data);
        }
        public byte[] Decrypt(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentException("Input is null");
            }
            CryptorHeader header = CryptorHeader.FromBytes(data);
            if (header == null)
            {
                return _fallbackCryptor?.Decrypt(data);
            }
            if (!header.Identifier.SequenceEqual(_cryptor?.Identifier))
            {
                throw new PNException("CryptorHeader mismatch");
            }
            return _cryptor?.Decrypt(data);
        }
    }
}
