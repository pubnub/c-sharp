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

        private static readonly byte[] _identifier = Encoding.ASCII.GetBytes("CRIV");

        private readonly byte[] _cipherKey;
        private readonly IPubnubLog _log;
        public AesCbcCryptoAlgorithm(string cipherKey, IPubnubLog log)
        {
            _cipherKey = Util.ComputeSha256(cipherKey);
            _log = log;
        }
        public AesCbcCryptoAlgorithm(string cipherKey): this(cipherKey, null)
        {
        }
        public byte[] Identifier => _identifier;

        public EncryptedData Encrypt(string data)
        {
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = _cipherKey;
                    aesAlg.IV = Util.InitializationVector(true, AES_BLOCK_SIZE);

                    byte[] encryptedData;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(ms, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(dataBytes, 0, data.Length);
                            csEncrypt.FlushFinalBlock();
                        }
                        encryptedData = ms.ToArray();
                    }
                    return new EncryptedData()
                    {
                        Data = Convert.ToBase64String(encryptedData), 
                        Metadata = Convert.ToBase64String(aesAlg.IV),
                        Status = new PNStatus { Error = false, ErrorData = null, StatusCode = 200 }
                    };
                }
            }
            catch (Exception ex)
            {
                _log?.WriteToLog(string.Format(CultureInfo.InvariantCulture, "DateTime {0} Encrypt Error. {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.ToString()));
                return new EncryptedData()
                {
                    Data = null, 
                    Metadata = null,
                    Status = new PNStatus { Error = true, ErrorData = new PNErrorData(ex.Message, ex), StatusCode = 400 }
                };
            }
        }
        
        public DecryptedData Decrypt(DataEnvelope encryptedData)
        {
            return null;
            //try
            //{
            //    using (Aes aesAlg = Aes.Create())
            //    {
            //        aesAlg.Key = _cipherKey;
            //        aesAlg.IV = encryptedData.Metadata.Take(AES_BLOCK_SIZE).ToArray(); // Extract IV

            //        byte[] decryptedData;
            //        using (MemoryStream ms = new MemoryStream())
            //        {
            //            using (CryptoStream csDecrypt = new CryptoStream(ms, aesAlg.CreateDecryptor(), CryptoStreamMode.Write))
            //            {
            //                csDecrypt.Write(encryptedData.Data, AES_BLOCK_SIZE, encryptedData.Metadata.Length - AES_BLOCK_SIZE);
            //                csDecrypt.FlushFinalBlock();
            //            }
            //            decryptedData = ms.ToArray();
            //        }
            //        return new DecryptedData
            //        {
            //            Data = decryptedData,
            //            Status = new PNStatus() { Error = false, ErrorData = null, StatusCode = 200 }
            //        };
            //    }
            //}
            //catch(Exception ex)
            //{
            //    _log?.WriteToLog(string.Format(CultureInfo.InvariantCulture, "DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.ToString()));
            //    return new DecryptedData
            //    {
            //        Data = null,
            //        Status = new PNStatus() { Error = true, ErrorData = new PNErrorData(ex.Message, ex), StatusCode = 400 }
            //    };
            //}
        }

        public EncryptedBytes Encrypt(byte[] data)
        {
            throw new NotImplementedException();
        }

        public DecryptedBytes Decrypt(BytesEnvelope encryptedBytes)
        {
            throw new NotImplementedException();
        }
    }
}
