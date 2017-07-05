using System;
using System.Text;

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
        private PNConfiguration config = null;
        private IPubnubLog pubnubLog = null;

        public PubnubCrypto(string cipher_key, PNConfiguration pubnubConfig, IPubnubLog log)
            : base(cipher_key)
        {
            this.config = pubnubConfig;
            this.pubnubLog = log;
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

        protected override string EncryptOrDecrypt(bool type, string plainStr)
        {
            //Demo params
            string keyString = GetEncryptionKey();

#if NET35
            Aes aesAlg = Aes.Create();
            aesAlg.KeySize = 256;
            aesAlg.BlockSize = 128;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.IV = System.Text.Encoding.UTF8.GetBytes("0123456789012345");
            aesAlg.Key = System.Text.Encoding.UTF8.GetBytes(keyString);
#else
            string input = plainStr;
            byte[] inputBytes;
            byte[] iv = System.Text.Encoding.UTF8.GetBytes("0123456789012345");
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
#if NET35
                byte[] cipherText = null;
                plainStr = EncodeNonAsciiCharacters(plainStr);
                ICryptoTransform crypto = aesAlg.CreateEncryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(plainStr);

                cipherText = crypto.TransformFinalBlock(plainText, 0, plainText.Length);

                return Convert.ToBase64String(cipherText);
#else
                input = EncodeNonAsciiCharacters(input);
                inputBytes = Encoding.UTF8.GetBytes(input);
                cipher.Init(true, keyParamWithIV);
                byte[] outputBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
                int length = cipher.ProcessBytes(inputBytes, outputBytes, 0);
                cipher.DoFinal(outputBytes, length); //Do the final block
                string encryptedInput = Convert.ToBase64String(outputBytes);

                return encryptedInput;
#endif
            }
            else
            {
                try
                {
                    //Decrypt
#if NET35
                    string decrypted = "";
                    byte[] decryptedBytes = Convert.FromBase64CharArray(plainStr.ToCharArray(), 0, plainStr.Length);
                    ICryptoTransform decrypto = aesAlg.CreateDecryptor();

                    var data = decrypto.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
                    decrypted = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                    return decrypted;
#else
                    inputBytes = Convert.FromBase64CharArray(input.ToCharArray(), 0, input.Length);
                    cipher.Init(false, keyParamWithIV);
                    byte[] encryptedBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
                    int encryptLength = cipher.ProcessBytes(inputBytes, encryptedBytes, 0);
                    int numOfOutputBytes = cipher.DoFinal(encryptedBytes, encryptLength); //Do the final block
                    int len = Array.IndexOf(encryptedBytes, (byte)0);
                    len = (len == -1) ? encryptedBytes.Length : len;
                    string actualInput = Encoding.UTF8.GetString(encryptedBytes, 0, len);
                    return actualInput;
#endif

                }
                catch (Exception ex)
                {
                    if (config != null)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(), ex.ToString()), config.LogVerbosity);
                    }
                    throw ex;
                    //LoggingMethod.WriteToLog(string.Format("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(), ex.ToString()), pubnubConfig.LogVerbosity);
                    //return "**DECRYPT ERROR**";
                }
            }
        }

    }
}
