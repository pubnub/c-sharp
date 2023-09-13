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
            if (data == null || data.Data == null)
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
                CryptorHeader header = CryptorHeader.FromBytes(Convert.FromBase64String(data.Data));
                if (header == null || !header.Identifier.SequenceEqual(_algorithm.Identifier))
                {
                    return new DecryptedData
                    {
                        Data = null,
                        Status = new PNStatus { Error = true, ErrorData = new PNErrorData("CryptorHeader mismatch", new Exception("CryptorHeader mismatch")), StatusCode = 400 }
                    };
                }
                DataEnvelope internalDataEnvelope = new EncryptedData 
                { 
                    Data = Convert.ToBase64String(Convert.FromBase64String(data.Data).Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray())
                };
                return _algorithm.Decrypt(internalDataEnvelope);
            }
        }
        public DecryptedBytes Decrypt(BytesEnvelope data)
        {
            if (data == null || data.Data == null)
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
                CryptorHeader header = CryptorHeader.FromBytes(data.Data);
                if (header == null || !header.Identifier.SequenceEqual(_algorithm.Identifier))
                {
                    return new DecryptedBytes
                    {
                        Data = null,
                        Status = new PNStatus { Error = true, ErrorData = new PNErrorData("CryptorHeader mismatch", new Exception("CryptorHeader mismatch")), StatusCode = 400 }
                    };
                }
                BytesEnvelope internalBytesEnvelope = new EncryptedBytes 
                { 
                    Data = data.Data.Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray()
                };

                return _algorithm.Decrypt(internalBytesEnvelope);
            }
        }
    }
}
