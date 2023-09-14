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

        public string Encrypt(string data)
        {
            return _algorithm.Encrypt(data);
        }
        public byte[] Encrypt(byte[] data)
        {
            return _algorithm.Encrypt(data);
        }

        public string Decrypt(string data)
        {
            if (data == null)
            {
                throw new ArgumentException("Input is null");
            }
            if (_algorithm is LegacyCryptoAlgorithm)
            {
                return _algorithm.Decrypt(data);
            }
            else
            {
                CryptorHeader header = CryptorHeader.FromBytes(Convert.FromBase64String(data));
                if (header == null || !header.Identifier.SequenceEqual(_algorithm.Identifier))
                {
                    throw new PNException("CryptorHeader mismatch");
                }
                string actualData = Convert.ToBase64String(Convert.FromBase64String(data).Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray());
                return _algorithm.Decrypt(actualData);
            }
        }
        public byte[] Decrypt(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentException("Input is null");
            }
            if (_algorithm is LegacyCryptoAlgorithm)
            {
                return _algorithm.Decrypt(data);
            }
            else
            {
                CryptorHeader header = CryptorHeader.FromBytes(data);
                if (header == null || !header.Identifier.SequenceEqual(_algorithm.Identifier))
                {
                    throw new PNException("CryptorHeader mismatch");
                }
                byte[] actualBytes = data.Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray();
                return _algorithm.Decrypt(actualBytes);
            }
        }
    }
}
