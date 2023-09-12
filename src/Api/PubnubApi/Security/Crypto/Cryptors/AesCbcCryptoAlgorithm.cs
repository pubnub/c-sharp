using PubnubApi.Security.Crypto.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PubnubApi.Security.Crypto.Cryptors
{
    public class AesCbcCryptoAlgorithm : ICryptoAlgorithm
    {
        /// AES cipher block size.
        private const int AES_BLOCK_SIZE = 16;

        private readonly string _cipherKey;
        private readonly bool _useRandomIV = true;
        private readonly IPubnubLog _log;
        private readonly IPubnubUnitTest _unitTest;
        public AesCbcCryptoAlgorithm(string cipherKey, IPubnubLog log, IPubnubUnitTest unitTest)
        {
            _cipherKey = cipherKey;
            _log = log;
            _unitTest = unitTest;
        }
        public AesCbcCryptoAlgorithm(string cipherKey): this(cipherKey, null, null)
        {
        }
        public byte[] Identifier { get; } = Encoding.ASCII.GetBytes("CRIV");

        private static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4", CultureInfo.InvariantCulture);
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public EncryptedData Encrypt(string data)
        {
            if (data == null)
            {
                return new EncryptedData
                {
                    Metadata = null,
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Input is null", new Exception("Input is null")), StatusCode = 400 }
                };
            }
            string input = EncodeNonAsciiCharacters(data);
            byte[] dataBytes = Encoding.UTF8.GetBytes(input);

            byte[] ivBytes = Util.InitializationVector(_useRandomIV, AES_BLOCK_SIZE);
            if (_unitTest != null && _unitTest.IV != null)
            {
                ivBytes = _unitTest.IV;
            }

            string keyString = Util.GetEncryptionKey(_cipherKey);
            try
            {
                EncryptedBytes encryptedBytes = InternalEncrypt(dataBytes, ivBytes, keyString);
                if (encryptedBytes.Data != null)
                {
                    return new EncryptedData
                    {
                        Metadata = Convert.ToBase64String(ivBytes),
                        Data = Convert.ToBase64String(encryptedBytes.Data)
                    };
                }
                else
                {
                    return new EncryptedData
                    {
                        Metadata = null,
                        Data = null,
                        Status = encryptedBytes.Status
                    };
                }
            }
            catch(Exception ex)
            {
                return new EncryptedData
                {
                    Metadata = null,
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Encryption error", ex), StatusCode = 400 }
                };
            }
        }
        public EncryptedBytes Encrypt(byte[] data)
        {
            if (data == null)
            {
                return new EncryptedBytes
                {
                    Metadata = null,
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Input is null", new Exception("Input is null")), StatusCode = 400 }
                };
            }

            int dataOffset = _useRandomIV ? AES_BLOCK_SIZE : 0;
            byte[] ivBytes = Util.InitializationVector(_useRandomIV, dataOffset);
            if (_unitTest != null && _unitTest.IV != null && _unitTest.IV.Length == 16)
            {
                ivBytes = _unitTest.IV;
            }

            string keyString = Util.GetEncryptionKey(_cipherKey);
            try
            {
                return InternalEncrypt(data, ivBytes, keyString);
            }
            catch(Exception ex)
            {
                return new EncryptedBytes
                {
                    Metadata = null,
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Encryption error", ex), StatusCode = 400 }
                };
            }
        }
        private EncryptedBytes InternalEncrypt(byte[] dataBytes, byte[] ivBytes, string keyString)
        {
            try
            {
                
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = AES_BLOCK_SIZE;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.IV = ivBytes;
                    aesAlg.Key = Encoding.UTF8.GetBytes(keyString);

                    using (ICryptoTransform crypto = aesAlg.CreateEncryptor())
                    {
                        byte[] buffer = crypto.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                        return new EncryptedBytes
                        {
                            Metadata = _useRandomIV ? ivBytes : null,
                            Data = buffer,
                            Status = null
                        };
                    }
                }
            }
            catch(Exception ex)
            {
                return new EncryptedBytes
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Encryption error", ex), StatusCode = 400 }
                };
            }
        }
        public DecryptedData Decrypt(DataEnvelope encryptedData)
        {
            if (encryptedData == null)
            {
                return new DecryptedData
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Input is null", new Exception("Input is null")), StatusCode = 400 }
                };
            }
            
            if (!(encryptedData is EncryptedData encData))
            {
                return new DecryptedData
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Input is not EncryptedData", new Exception("Input is not EncryptedData")), StatusCode = 400 }
                };
            }

            byte[] dataBytes;
            byte[] ivBytes;
            try
            {
                dataBytes = Convert.FromBase64String(encData.Data);
                ivBytes = Convert.FromBase64String(encData.Metadata);
            }
            catch(Exception ex)
            {
                return new DecryptedData
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Base64 conversion error", ex), StatusCode = 400 }
                };
            }
            string keyString = Util.GetEncryptionKey(_cipherKey);

            try
            {
                DecryptedBytes decryptedBytes = InternalDecrypt(dataBytes, ivBytes, keyString);
                if (decryptedBytes.Data != null)
                {
                    return new DecryptedData
                    {
                        Data = Encoding.UTF8.GetString(decryptedBytes.Data),
                        Status = null
                    };
                }
                else
                {
                    return new DecryptedData
                    {
                        Data = null,
                        Status = decryptedBytes.Status
                    };
                }
                
            }
            catch(Exception ex)
            {
                return new DecryptedData
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Decryption error", ex), StatusCode = 400 }
                };
            }
        }
        public DecryptedBytes Decrypt(BytesEnvelope encryptedBytes)
        {
            if (encryptedBytes == null || encryptedBytes.Data == null)
            {
                return new DecryptedBytes
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Input is null", new Exception("Input is null")), StatusCode = 400 }
                };
            }
            
            if (!(encryptedBytes is EncryptedBytes))
            {
                return new DecryptedBytes
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Input is not EncryptedData", new Exception("Input is not EncryptedData")), StatusCode = 400 }
                };
            }

            byte[] ivBytes = encryptedBytes.Metadata;
            byte[] dataBytes = encryptedBytes.Data;
            string keyString = Util.GetEncryptionKey(_cipherKey);
            try
            {
                return InternalDecrypt(dataBytes, ivBytes, keyString);
            }
            catch(Exception ex)
            {
                return new DecryptedBytes
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Decryption error", ex), StatusCode = 400 }
                };
            }
        }
        private DecryptedBytes InternalDecrypt(byte[] dataBytes, byte[] ivBytes, string keyString)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = AES_BLOCK_SIZE;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.IV = ivBytes;
                    aesAlg.Key = Encoding.UTF8.GetBytes(keyString);

                    using(ICryptoTransform decrypto = aesAlg.CreateDecryptor())
                    {
                        byte[] buffer = decrypto.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                        return new DecryptedBytes
                        {
                            Data = buffer,
                            Status = null
                        };
                    }
                }
                
            }
            catch(Exception ex)
            {
                return new DecryptedBytes
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Decryption error", ex), StatusCode = 400 }
                };
            }
        }
    }
}
