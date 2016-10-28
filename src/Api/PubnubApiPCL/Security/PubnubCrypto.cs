
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Text;

namespace PubnubApi
{
    public class PubnubCrypto : PubnubCryptoBase
    {
        public PubnubCrypto(string cipher_key)
            : base(cipher_key)
        {
        }

        protected override string ComputeHashRaw(string input)
        {
            Sha256Digest algorithm = new Sha256Digest();
            Byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            Byte[] bufferBytes = new byte[algorithm.GetDigestSize()];
            algorithm.BlockUpdate(inputBytes, 0, inputBytes.Length);
            algorithm.DoFinal(bufferBytes, 0);
            return BitConverter.ToString(bufferBytes);
        }

        protected override string EncryptOrDecrypt(bool type, string plainStr)
        {
            //Demo params
            string keyString = GetEncryptionKey();

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

            if (type)
            {
                // Encrypt
                input = EncodeNonAsciiCharacters(input);
                inputBytes = Encoding.UTF8.GetBytes(input);
                cipher.Init(true, keyParamWithIV);
                byte[] outputBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
                int length = cipher.ProcessBytes(inputBytes, outputBytes, 0);
                cipher.DoFinal(outputBytes, length); //Do the final block
                string encryptedInput = Convert.ToBase64String(outputBytes);

                return encryptedInput;
            }
            else
            {
                try
                {
                    //Decrypt
                    inputBytes = Convert.FromBase64CharArray(input.ToCharArray(), 0, input.Length);
                    cipher.Init(false, keyParamWithIV);
                    byte[] encryptedBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
                    int encryptLength = cipher.ProcessBytes(inputBytes, encryptedBytes, 0);
                    int numOfOutputBytes = cipher.DoFinal(encryptedBytes, encryptLength); //Do the final block
                                                                                          //string actualInput = Encoding.UTF8.GetString(encryptedBytes, 0, encryptedBytes.Length);
                    int len = Array.IndexOf(encryptedBytes, (byte)0);
                    len = (len == -1) ? encryptedBytes.Length : len;
                    string actualInput = Encoding.UTF8.GetString(encryptedBytes, 0, len);
                    return actualInput;

                }
                catch (Exception ex)
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(), ex.ToString()), PNLogVerbosity.BODY);
                    throw ex;
                    //LoggingMethod.WriteToLog(string.Format("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(), ex.ToString()), PNLogVerbosity.BODY);
                    //return "**DECRYPT ERROR**";
                }
            }
        }

    }
}
