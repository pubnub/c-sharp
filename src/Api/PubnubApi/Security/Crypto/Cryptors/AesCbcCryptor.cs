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
    public class AesCbcCryptor : CryptorBase
    {
        private const string IDENTIFIER = "ACRH";

        public AesCbcCryptor(string cipherKey, IPubnubLog log, IPubnubUnitTest unitTest): base(cipherKey, true, log, unitTest)
        {
        }
        public AesCbcCryptor(string cipherKey): this(cipherKey, null, null) { }
        public new string CipherKey => base.CipherKey;
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
                CryptorHeader header = CryptorHeader.FromBytes(Convert.FromBase64String(encryptedData));
                if (header == null || !header.Identifier.SequenceEqual(Identifier))
                {
                    throw new PNException("CryptorHeader mismatch");
                }
                string actualData = Convert.ToBase64String(Convert.FromBase64String(encryptedData).Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray());
                byte[] dataBytes = Convert.FromBase64String(actualData);
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
                CryptorHeader header = CryptorHeader.FromBytes(encryptedData);
                if (header == null || !header.Identifier.SequenceEqual(Identifier))
                {
                    throw new PNException("CryptorHeader mismatch");
                }
                byte[] actualBytes = encryptedData.Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray();
                byte[] ivBytes = actualBytes.Take(16).ToArray();
                byte[] dataBytes = actualBytes.Skip(16).ToArray();
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
