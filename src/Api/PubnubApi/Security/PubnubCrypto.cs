using System;
using System.Text;
using System.Globalization;

#if NET35
using System.Security.Cryptography;
#else
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
#endif

namespace PubnubApi
{
    public class PubnubCrypto : PubnubCryptoBase
    {
        private readonly PNConfiguration config;
        private readonly IPubnubLog pubnubLog;
        private readonly IPubnubUnitTest pnUnit;

        public PubnubCrypto(string cipher_key, PNConfiguration pubnubConfig, IPubnubLog log, IPubnubUnitTest pubnubUnit)
            : base(cipher_key, pubnubConfig)
        {
            this.config = pubnubConfig;
            this.pubnubLog = log;
            this.pnUnit = pubnubUnit;
        }

        public PubnubCrypto(string cipher_key)
            : base(cipher_key)
        {
        }

        protected override string ComputeHashRaw(string input)
        {
#if NET35
            HashAlgorithm algorithm = SHA256.Create();
            Byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes);
#else
            Sha256Digest algorithm = new Sha256Digest();
            Byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            Byte[] bufferBytes = new byte[algorithm.GetDigestSize()];
            algorithm.BlockUpdate(inputBytes, 0, inputBytes.Length);
            algorithm.DoFinal(bufferBytes, 0);
            return BitConverter.ToString(bufferBytes);
#endif
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
                    new Random().NextBytes(ivBytes);
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
                ivBytes = pnUnit.IV; //new byte[] { 76, 224, 5, 202, 19, 66, 254, 23, 44, 240, 122, 244, 252, 13, 74, 61 };// 
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine("IV = " + GetDisplayableBytes(ivBytes));
#endif
#if NET35
            Aes aesAlg = Aes.Create();
            aesAlg.KeySize = 256;
            aesAlg.BlockSize = 128;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.IV = ivBytes;
            aesAlg.Key = System.Text.Encoding.UTF8.GetBytes(keyString);
#else
            byte[] iv = ivBytes;
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(keyString);

            //Set up
            AesEngine engine = new AesEngine();
            CbcBlockCipher blockCipher = new CbcBlockCipher(engine); //CBC
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(blockCipher); //Default scheme is PKCS5/PKCS7
            KeyParameter keyParam = new KeyParameter(keyBytes);
            ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, iv, 0, iv.Length);
#endif


            if (type)
            {
                // Encrypt
                byte[] outputBytes = null;
#if NET35
                ICryptoTransform crypto = aesAlg.CreateEncryptor();
                byte[] plainBytes = dataBytes;
                outputBytes = crypto.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
#else
                byte[] inputBytes = dataBytes;
                cipher.Init(true, keyParamWithIV);
                outputBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
                int length = cipher.ProcessBytes(inputBytes, outputBytes, 0);
                cipher.DoFinal(outputBytes, length); //Do the final block
#endif
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
#if NET35
                    byte[] decryptedBytes = dataBytes;
                    ICryptoTransform decrypto = aesAlg.CreateDecryptor();

                    var data = decrypto.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
                    newOutputBytes = data;
#else
                    byte[] inputBytes = dataBytes;
                    cipher.Init(false, keyParamWithIV);
                    byte[] encryptedBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
                    var encryptLength = cipher.ProcessBytes(inputBytes, encryptedBytes, 0);
                    var lastBytesLength = cipher.DoFinal(encryptedBytes, encryptLength); //Do the final block
                    var totalBytesLength = encryptLength + lastBytesLength;
                    newOutputBytes = new byte[totalBytesLength];
                    Array.Copy(encryptedBytes, newOutputBytes, totalBytesLength);
#endif
                    return newOutputBytes;
                }
                catch (Exception ex)
                {
                    if (config != null)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), config.LogVerbosity);
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
