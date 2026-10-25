using System;
using System.Security.Cryptography;
using PubnubApi.Security.Crypto.Common;

namespace PubnubApi.Security.Crypto.Cryptors
{
    public abstract class CryptorBase : ICryptor
    {
        protected readonly PubnubLogModule logger;
        public string CipherKey { get; }
        
        protected CryptorBase(string cipherKey, PubnubLogModule logger)
        {
            this.logger = logger;
            CipherKey = cipherKey;
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
