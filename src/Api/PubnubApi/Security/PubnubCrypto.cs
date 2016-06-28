using System;
using System.Security.Cryptography;
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
#if (SILVERLIGHT || WINDOWS_PHONE || MONOTOUCH || __IOS__ || MONODROID || __ANDROID__ )
            HashAlgorithm algorithm = new System.Security.Cryptography.SHA256Managed();
#elif NETFX_CORE
            HashAlgorithmProvider algorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
#else
            HashAlgorithm algorithm = new SHA256CryptoServiceProvider();
#endif

#if (SILVERLIGHT || WINDOWS_PHONE)
            Byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
#elif NETFX_CORE
            IBuffer inputBuffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
#else
            Byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
#endif
#if NETFX_CORE
            IBuffer hashedBuffer = algorithm.HashData(inputBuffer);
            byte[] inputBytes;
            CryptographicBuffer.CopyToByteArray(hashedBuffer, out inputBytes);
            return BitConverter.ToString(inputBytes);
#else
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes);
#endif
        }

        protected override string EncryptOrDecrypt(bool type, string plainStr)
        {
            byte[] cipherText = null;

#if (SILVERLIGHT || WINDOWS_PHONE)
                AesManaged aesEncryption = new AesManaged();
                aesEncryption.KeySize = 256;
                aesEncryption.BlockSize = 128;
                //get ASCII bytes of the string
                aesEncryption.IV = System.Text.Encoding.UTF8.GetBytes("0123456789012345");
                aesEncryption.Key = System.Text.Encoding.UTF8.GetBytes(GetEncryptionKey());
#elif NETFX_CORE
            SymmetricKeyAlgorithmProvider algoritmProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            IBuffer keyMaterial = CryptographicBuffer.ConvertStringToBinary(GetEncryptionKey(), BinaryStringEncoding.Utf8);
            CryptographicKey key = algoritmProvider.CreateSymmetricKey(keyMaterial);
            IBuffer iv = CryptographicBuffer.ConvertStringToBinary("0123456789012345", BinaryStringEncoding.Utf8);
#else
            RijndaelManaged aesEncryption = new RijndaelManaged();
            aesEncryption.KeySize = 256;
            aesEncryption.BlockSize = 128;
            //Mode CBC
            aesEncryption.Mode = CipherMode.CBC;
            //padding
            aesEncryption.Padding = PaddingMode.PKCS7;
            //get ASCII bytes of the string
            aesEncryption.IV = System.Text.Encoding.ASCII.GetBytes("0123456789012345");
            aesEncryption.Key = System.Text.Encoding.ASCII.GetBytes(GetEncryptionKey());
#endif


            if (type)
            {
                plainStr = EncodeNonAsciiCharacters(plainStr);
#if (SILVERLIGHT || WINDOWS_PHONE)
                ICryptoTransform crypto = aesEncryption.CreateEncryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(plainStr);
                
                //encrypt
                cipherText = crypto.TransformFinalBlock(plainText, 0, plainText.Length);
#elif NETFX_CORE
                IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(plainStr, BinaryStringEncoding.Utf8);
                IBuffer buffEncrypt = CryptographicEngine.Encrypt(key, buffMsg, iv);
                CryptographicBuffer.CopyToByteArray(buffEncrypt, out cipherText);
#else
                ICryptoTransform crypto = aesEncryption.CreateEncryptor();
                byte[] plainText = Encoding.ASCII.GetBytes(plainStr);

                //encrypt
                cipherText = crypto.TransformFinalBlock(plainText, 0, plainText.Length);
#endif

                return Convert.ToBase64String(cipherText);
            }
            else
            {
                string decrypted = "";
                try
                {
                    //decode
                    byte[] decryptedBytes = Convert.FromBase64CharArray(plainStr.ToCharArray(), 0, plainStr.Length);


#if (SILVERLIGHT || WINDOWS_PHONE)
                    ICryptoTransform decrypto = aesEncryption.CreateDecryptor();
                    //decrypt
                    var data = decrypto.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
                    decrypted = Encoding.UTF8.GetString(data, 0, data.Length);
#elif NETFX_CORE
                    IBuffer buffMsg = CryptographicBuffer.DecodeFromBase64String(plainStr);
                    IBuffer buffDecrypted = CryptographicEngine.Decrypt(key, buffMsg, iv);
                    CryptographicBuffer.CopyToByteArray(buffDecrypted, out decryptedBytes);
                    decrypted = Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
#else
                    ICryptoTransform decrypto = aesEncryption.CreateDecryptor();
                    //decrypt                    
                    decrypted = System.Text.Encoding.ASCII.GetString(decrypto.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length));
#endif

                    return decrypted;
                }
                catch (Exception ex)
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelVerbose);
                    throw ex;
                    //LoggingMethod.WriteToLog(string.Format("DateTime {0} Decrypt Error. {1}", DateTime.Now.ToString(), ex.ToString()), LoggingMethod.LevelVerbose);
                    //return "**DECRYPT ERROR**";
                }
            }
        }

    }
}
