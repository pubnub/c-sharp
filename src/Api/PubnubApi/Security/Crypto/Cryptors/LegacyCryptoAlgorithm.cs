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
    public class LegacyCryptoAlgorithm : ICryptoAlgorithm
    {
        /// AES cipher block size.
        private const int AES_BLOCK_SIZE = 128;

        private static readonly byte[] _identifier = new byte[] { };

        private readonly string _cipherKey;
        private readonly bool _useRandomIV;
        private readonly IPubnubLog _log;
        private readonly IPubnubUnitTest _unitTest;
        public LegacyCryptoAlgorithm(string cipherKey, bool useRandomIV, IPubnubLog log, IPubnubUnitTest unitTest)
        {
            _cipherKey = cipherKey;
            _useRandomIV = useRandomIV;
            _log = log;
            _unitTest = unitTest;
        }
        public LegacyCryptoAlgorithm(string cipherKey, bool useRandomIV, IPubnubLog log): this(cipherKey, useRandomIV, log, null)
        {
        }
        public LegacyCryptoAlgorithm(string cipherKey, bool useRandomIV): this(cipherKey, useRandomIV, null, null)
        {
        }
        public LegacyCryptoAlgorithm(string cipherKey): this(cipherKey, true, null, null)
        {
        }
        public byte[] Identifier => _identifier;
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

            int dataOffset = _useRandomIV ? AES_BLOCK_SIZE : 0;
            byte[] ivBytes = Util.InitializationVector(_useRandomIV, dataOffset);
            if (_unitTest != null && _unitTest.IV != null && _unitTest.IV.Length == 16)
            {
                ivBytes = _unitTest.IV;
            }

            string keyString = Util.GetLegacyEncryptionKey(_cipherKey);
            try
            {
                EncryptedBytes encryptedBytes = InternalEncrypt(dataBytes, ivBytes, keyString);
                if (encryptedBytes.Data != null)
                {
                    return new EncryptedData
                    {
                        Metadata = null,
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

            string keyString = Util.GetLegacyEncryptionKey(_cipherKey);
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
                byte[] buffer;
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
                        byte[] outputBytes = crypto.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                        if (_useRandomIV)
                        {
                            buffer = new byte[ivBytes.Length + outputBytes.Length];
                            Buffer.BlockCopy(ivBytes, 0, buffer, 0, ivBytes.Length);
                            Buffer.BlockCopy(outputBytes, 0, buffer, ivBytes.Length, outputBytes.Length);
                            return new EncryptedBytes
                            {
                                Data = buffer,
                                Status = null
                            };
                        }
                        else
                        {
                            return new EncryptedBytes
                            {
                                Data = outputBytes,
                                Status = null
                            };
                        }
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
            try
            {
                dataBytes = Convert.FromBase64String(encData.Data);
            }
            catch(Exception ex)
            {
                return new DecryptedData
                {
                    Data = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Base64 conversion error", ex), StatusCode = 400 }
                };
            }
            byte[] ivBytes = _useRandomIV ? dataBytes.Take(16).ToArray() : Encoding.UTF8.GetBytes("0123456789012345");
            dataBytes = _useRandomIV ? dataBytes.Skip(16).ToArray() : dataBytes;

            
            string keyString = Util.GetLegacyEncryptionKey(_cipherKey);

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

            byte[] ivBytes = _useRandomIV ? encryptedBytes.Data.Take(16).ToArray() : Encoding.UTF8.GetBytes("0123456789012345");
            byte[] dataBytes = _useRandomIV ? encryptedBytes.Data.Skip(16).ToArray() : encryptedBytes.Data;
            string keyString = Util.GetLegacyEncryptionKey(_cipherKey);
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
                    aesAlg.Key = System.Text.Encoding.UTF8.GetBytes(keyString);

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
