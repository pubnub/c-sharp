using PubnubApi.Security.Crypto.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace PubnubApi.Security.Crypto.Cryptors
{
    public class AesCbcCryptoAlgorithm : CryptoAlgorithmBase
    {
        private const string IDENTIFIER = "CRIV";

        public AesCbcCryptoAlgorithm(string cipherKey, IPubnubLog log, IPubnubUnitTest unitTest): base(cipherKey, true, log, unitTest)
        {
        }
        public AesCbcCryptoAlgorithm(string cipherKey): this(cipherKey, null, null) { }
        public override byte[] Identifier => Encoding.ASCII.GetBytes(IDENTIFIER);
        public override string Encrypt(string data)
        {
            if (data == null) { throw new ArgumentException("Invalid input","data"); }
            try
            {
                string input = Util.EncodeNonAsciiCharacters(data);
                byte[] dataBytes = Encoding.UTF8.GetBytes(input);
                byte[] ivBytes = GenerateRandomIV(true);
                Log(string.Format(CultureInfo.InvariantCulture, "DateTime {0} IV = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ivBytes.ToDisplayFormat()));
                byte[] keyBytes = Util.GetEncryptionKeyBytes(CipherKey);
                byte[] encryptedBytes = InternalEncrypt(true, dataBytes, ivBytes, keyBytes);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new PNException("Encrypt Error", ex);
            }
        }
        public override byte[] Encrypt(byte[] data)
        {
            if (data == null) { throw new ArgumentException("Invalid input","data"); }
            try
            {
                byte[] ivBytes = GenerateRandomIV(true);
                byte[] keyBytes = Util.GetEncryptionKeyBytes(CipherKey);
                return InternalEncrypt(true, data, ivBytes, keyBytes);
            }
            catch (Exception ex)
            {
                throw new PNException("Encrypt Error", ex);
            }
        }
        public override string Decrypt(string encryptedData)
        {
            if (encryptedData == null) { throw new ArgumentException("Invalid input","encryptedData"); }
            try
            {
                byte[] dataBytes = Convert.FromBase64String(encryptedData);
                byte[] ivBytes = dataBytes.Take(16).ToArray();
                dataBytes = dataBytes.Skip(16).ToArray();
                byte[] keyBytes = Util.GetEncryptionKeyBytes(CipherKey);
                byte[] decryptedBytes = InternalDecrypt(dataBytes, ivBytes, keyBytes);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch(Exception ex)
            {
                throw new PNException("Decrypt Error", ex);
            }
        }
        public override byte[] Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null) { throw new ArgumentException("Invalid input","encryptedData"); }
            try
            {
                byte[] ivBytes = encryptedData.Take(16).ToArray();
                byte[] dataBytes = encryptedData.Skip(16).ToArray();
                byte[] keyBytes = Util.GetEncryptionKeyBytes(CipherKey);
                return InternalDecrypt(dataBytes, ivBytes, keyBytes);
            }
            catch (Exception ex)
            {
                throw new PNException("Decrypt Error", ex);
            }
        }
    }
}
