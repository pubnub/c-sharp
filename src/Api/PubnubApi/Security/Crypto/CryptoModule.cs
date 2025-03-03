using System;
using System.Collections.Generic;
using System.IO;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubnubApi.Security.Crypto
{
    public class CryptoModule
    {
        private readonly ICryptor _encryptor;
        private readonly Dictionary<string, ICryptor> _decryptors;
        public CryptoModule(ICryptor defaultCryptor, IEnumerable<ICryptor> decryptors)
        {
            _encryptor = defaultCryptor ?? throw new ArgumentNullException(nameof(defaultCryptor));;
            _decryptors = new Dictionary<string, ICryptor>();
            AddDecryptor(_encryptor);
            if (decryptors != null)
            {
                foreach (var decryptor in decryptors)
                {
                    AddDecryptor(decryptor);
                }
            }
        }

        private void AddDecryptor(ICryptor decryptor)
        {
            if (!_decryptors.TryGetValue(decryptor.Identifier, out var curentDecryptor))
            {
                _decryptors[decryptor.Identifier] = decryptor;
            }
        }
        public static LegacyCryptor CreateLegacyCryptor(string cipherKey, bool useDynamicIV, PubnubLogModule log)
        {
            return new LegacyCryptor(cipherKey, useDynamicIV, log);
        }
        public static LegacyCryptor CreateLegacyCryptor(string cipherKey, bool useDynamicIV)
        {
            return new LegacyCryptor(cipherKey, useDynamicIV);
        }
        public static LegacyCryptor CreateLegacyCryptor(string cipherKey)
        {
            return new LegacyCryptor(cipherKey, true);
        }
        public static AesCbcCryptor CreateAesCbcCryptor(string cipherKey, PubnubLogModule log)
        {
            return new AesCbcCryptor(cipherKey, log);
        }
        public static AesCbcCryptor CreateAesCbcCryptor(string cipherKey)
        {
            return new AesCbcCryptor(cipherKey);
        }
        public string Encrypt(string data)
        {
            return _encryptor.Encrypt(data);
        }
        public byte[] Encrypt(byte[] data)
        {
            return _encryptor.Encrypt(data);
        }
        public void EncryptFile(string sourceFile, string destinationFile)
        {
            _encryptor.EncryptFile(sourceFile, destinationFile);
        }
        public string Decrypt(string data)
        {
            if (data == null)
            {
                throw new ArgumentException("Input is null");
            }
            CryptorHeader header = CryptorHeader.FromBytes(Convert.FromBase64String(data));
            if (_decryptors.TryGetValue(header.Identifier, out var decryptor))
            {
                return decryptor.Decrypt(data);
            }
            else
            {
                throw new PNException("unknown cryptor error");
            }
        }
        public byte[] Decrypt(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentException("Input is null");
            }
            CryptorHeader header = CryptorHeader.FromBytes(data);
            if (_decryptors.TryGetValue(header.Identifier, out var decryptor))
            {
                return decryptor.Decrypt(data);
            }
            else
            {
                throw new PNException("unknown cryptor error");
            }
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
            #if !NETSTANDARD10 && !NETSTANDARD11
            if (new FileInfo(sourceFile).Length < 1)
            {
                throw new PNException("decryption error");
            }
            #else
                throw new NotSupportedException("FileStream not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
            #endif
            CryptorHeader header = CryptorHeader.FromFile(sourceFile);
            if (_decryptors.TryGetValue(header.Identifier, out var decryptor))
            {
                decryptor.DecryptFile(sourceFile, destinationFile);
            }
            else
            {
                throw new PNException("unknown cryptor error");
            }
        }
    }
}
