using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.Security.Crypto.Cryptors
{
    public class Cryptor
    {
        private readonly ICryptoAlgorithm _algorithm;

        public Cryptor(ICryptoAlgorithm algorithm)
        {
            this._algorithm = algorithm;
        }

        public byte[] Identifier()
        {
            return _algorithm.Identifier;
        }

        public EncryptedData Encrypt(string data)
        {
            return _algorithm.Encrypt(data);
        }
        public EncryptedBytes Encrypt(byte[] data)
        {
            return _algorithm.Encrypt(data);
        }

        public DecryptedData Decrypt(DataEnvelope data)
        {
            return _algorithm.Decrypt(data);
        }
        public DecryptedBytes Decrypt(BytesEnvelope data)
        {
            return _algorithm.Decrypt(data);
        }
    }
}
