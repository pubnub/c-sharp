using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.Security.Crypto
{
    public class Cryptor
    {
        private readonly ICryptor _cryptor;

        public Cryptor(ICryptor cryptor)
        {
            this._cryptor = cryptor;
        }

        public byte[] Identifier
        {
            get
            {
                return _cryptor.Identifier;
            }
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
            return _cryptor.Decrypt(data);
        }
        public byte[] Decrypt(byte[] data)
        {
            return _cryptor.Decrypt(data);
        }
        public void DecryptFile(string sourceFile, string destinationFile)
        {
            _cryptor.DecryptFile(sourceFile, destinationFile);
        }
    }
}
