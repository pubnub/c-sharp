using System;
using System.Text;
using System.Globalization;

using System.Security.Cryptography;

namespace PubnubApi
{
    [Obsolete("Use CryptoModule instead.", false)]
    public class PubnubCrypto : PubnubCryptoBase
    {
        private readonly PNConfiguration config;
        private readonly IPubnubLog pubnubLog;
        private readonly IPubnubUnitTest pnUnit;

        public PubnubCrypto(string cipherKey, PNConfiguration pubnubConfig, IPubnubLog log, IPubnubUnitTest pubnubUnit)
            : base(cipherKey, pubnubConfig)
        {
            this.config = pubnubConfig;
            this.pubnubLog = log;
            this.pnUnit = pubnubUnit;
        }

        public PubnubCrypto(string cipherKey)
            : base(cipherKey)
        {
        }

        protected override string ComputeHashRaw(string input)
        {
            HashAlgorithm algorithm = SHA256.Create();
            Byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes);
        }

        protected override string EncryptOrDecrypt(bool type, string dataStr, bool dynamicIV)
        {
            byte[] dataBytes = null;
            if (type)
            {
                //encrypt
                string input = EncodeNonAsciiCharacters(dataStr);
                dataBytes = Encoding.UTF8.GetBytes(input);
            }
            else
            {
                //decrypt
                dataBytes = Convert.FromBase64CharArray(dataStr.ToCharArray(), 0, dataStr.Length);
            }

            byte[] retBytes = EncryptOrDecrypt(type, dataBytes, dynamicIV);

            if (type)
            {
                //encrypt
                return Convert.ToBase64String(retBytes); 
            }
            else
            {
                //decrypt
                return Encoding.UTF8.GetString(retBytes, 0, retBytes.Length);
            }
        }

        protected override byte[] EncryptOrDecrypt(bool type, byte[] dataBytes, bool dynamicIV)
        {
            string keyString = GetEncryptionKey();
            byte[] ivBytes = new byte[16];
            if (type)
            {
                //encrypt
                if (dynamicIV)
                {
#if NETSTANDARD10 || NETSTANDARD11
                    new Random().NextBytes(ivBytes);
#else
                    var rng = RandomNumberGenerator.Create();
                    
                    try
                    {
                        rng.GetBytes(ivBytes);
                    }
                    finally 
                    {
                        var disposable = rng as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
#endif
                }
                else
                {
                    ivBytes = Encoding.UTF8.GetBytes("0123456789012345");
                }
            }
            else
            {
                //decrypt - check if random IV is part of the message
                if (dynamicIV)
                {
                    Array.Copy(dataBytes, 0, ivBytes, 0, 16);
                    byte[] receivedBytes = new byte[dataBytes.Length - 16];
                    Array.Copy(dataBytes, 16, receivedBytes, 0, dataBytes.Length-16);
                    dataBytes = receivedBytes;
                }
                else
                {
                    ivBytes = System.Text.Encoding.UTF8.GetBytes("0123456789012345");
                }
            }
            if (pnUnit != null && pnUnit.IV != null && pnUnit.IV.Length == 16)
            {
                ivBytes = pnUnit.IV;
            }
            if (config != null)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} IV = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), GetDisplayableBytes(ivBytes)), config.LogVerbosity);
            }

            Aes aesAlg = Aes.Create();
            aesAlg.KeySize = 256;
            aesAlg.BlockSize = 128;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.IV = ivBytes;
            aesAlg.Key = System.Text.Encoding.UTF8.GetBytes(keyString);

            if (type)
            {
                // Encrypt
                byte[] outputBytes = null;
                ICryptoTransform crypto = aesAlg.CreateEncryptor();
                byte[] plainBytes = dataBytes;
                outputBytes = crypto.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                byte[] newOutputBytes = null;
                if (dynamicIV)
                {
                    newOutputBytes = new byte[ivBytes.Length + outputBytes.Length];
                    Buffer.BlockCopy(ivBytes, 0, newOutputBytes, 0, ivBytes.Length);
                    Buffer.BlockCopy(outputBytes, 0, newOutputBytes, ivBytes.Length, outputBytes.Length);
                }
                else
                {
                    newOutputBytes = outputBytes;
                }

                return newOutputBytes;
            }
            else
            {
                byte[] newOutputBytes = null;
                try
                {
                    //Decrypt
                    byte[] decryptedBytes = dataBytes;
                    ICryptoTransform decrypto = aesAlg.CreateDecryptor();

                    var data = decrypto.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
                    newOutputBytes = data;

                    return newOutputBytes;
                }
                catch (Exception ex)
                {
                    if (config != null)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), config.LogVerbosity);
                    }
                    throw;
                }
            }
        }

        private static string GetDisplayableBytes(byte[] currentBytes)
        {
            StringBuilder outBuilder = new StringBuilder("{ ");
            for (int di = 0; di < currentBytes.Length; di++)
            {
                outBuilder.Append(currentBytes[di]);
                if (di < currentBytes.Length - 1)
                {
                    outBuilder.Append(", ");
                }
            }
            outBuilder.Append(" }");
            return outBuilder.ToString();
        }
    }
}
