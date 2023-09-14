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

        public DecryptedData Decrypt(string data)
        {
            if (data == null)
            {
                return new DecryptedData
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Input is null", new Exception("Input is null")), StatusCode = 400 }
                };
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
                    return new DecryptedData
                    {
                        Data = null,
                        Status = new PNStatus { Error = true, ErrorData = new PNErrorData("CryptorHeader mismatch", new Exception("CryptorHeader mismatch")), StatusCode = 400 }
                    };
                }
                string actualData = Convert.ToBase64String(Convert.FromBase64String(data).Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray());
                return _algorithm.Decrypt(actualData);
            }
        }
        public DecryptedBytes Decrypt(byte[] data)
        {
            if (data == null)
            {
                return new DecryptedBytes
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Input is null", new Exception("Input is null")), StatusCode = 400 }
                };
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
                    return new DecryptedBytes
                    {
                        Data = null,
                        Status = new PNStatus { Error = true, ErrorData = new PNErrorData("CryptorHeader mismatch", new Exception("CryptorHeader mismatch")), StatusCode = 400 }
                    };
                }
                byte[] actualBytes = data.Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray();
                return _algorithm.Decrypt(actualBytes);
            }
        }
    }
}
