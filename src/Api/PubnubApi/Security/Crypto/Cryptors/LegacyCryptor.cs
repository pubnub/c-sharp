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
    public class LegacyCryptor : CryptorBase
    {
        private static readonly byte[] _identifier = new byte[] { };

        private readonly bool _useDynamicRandomIV;
        public LegacyCryptor(string cipherKey, bool useDynamicRandomIV, IPubnubLog log, IPubnubUnitTest unitTest): base(cipherKey, useDynamicRandomIV, log, unitTest)
        {
            _useDynamicRandomIV = useDynamicRandomIV;
        }
        public LegacyCryptor(string cipherKey, bool useDynamicRandomIV, IPubnubLog log): this(cipherKey, useDynamicRandomIV, log, null)
        {
        }
        public LegacyCryptor(string cipherKey, bool useDynamicRandomIV): this(cipherKey, useDynamicRandomIV, null, null)
        {
        }
        public LegacyCryptor(string cipherKey): this(cipherKey, true, null, null)
        {
        }
        public new string CipherKey => base.CipherKey;
        public bool UseDynamicRandomIV => _useDynamicRandomIV;
        public override byte[] Identifier => _identifier;
        public override string Encrypt(string data)
        {
            if (data == null)
            {
                throw new ArgumentException("Invalid input","data");
            }
            try
            {
                string input = Util.EncodeNonAsciiCharacters(data);
                byte[] dataBytes = Encoding.UTF8.GetBytes(input);
                byte[] ivBytes = GenerateRandomIV(_useDynamicRandomIV);
                Log(string.Format(CultureInfo.InvariantCulture, "DateTime {0} IV = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ivBytes.ToDisplayFormat()));
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
                byte[] encryptedBytes = InternalEncrypt(false, dataBytes, ivBytes, keyBytes);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new PNException("Encrypt Error", ex);
            }
        }
        public override byte[] Encrypt(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentException("Invalid input","data");
            }
            try
            {
                byte[] ivBytes = GenerateRandomIV(_useDynamicRandomIV);
                Log(string.Format(CultureInfo.InvariantCulture, "DateTime {0} IV = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ivBytes.ToDisplayFormat()));
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
                return InternalEncrypt(false, data, ivBytes, keyBytes);
            }
            catch (Exception ex)
            {
                throw new PNException("Encrypt Error", ex);
            }
        }
        public override string Decrypt(string encryptedData)
        {
            if (encryptedData == null)
            {
                throw new ArgumentException("Invalid input","encryptedData");
            }
            try
            {
                byte[] dataBytes = Convert.FromBase64String(encryptedData);
                byte[] ivBytes = _useDynamicRandomIV ? dataBytes.Take(16).ToArray() : Encoding.UTF8.GetBytes("0123456789012345");
                dataBytes = _useDynamicRandomIV ? dataBytes.Skip(16).ToArray() : dataBytes;
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
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
            if (encryptedData == null)
            {
                throw new ArgumentException("Invalid input","encryptedData");
            }
            try
            {
                byte[] ivBytes = _useDynamicRandomIV ? encryptedData.Take(16).ToArray() : Encoding.UTF8.GetBytes("0123456789012345");
                byte[] dataBytes = _useDynamicRandomIV ? encryptedData.Skip(16).ToArray() : encryptedData;
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
                return InternalDecrypt(dataBytes, ivBytes, keyBytes);
            }
            catch(Exception ex)
            {
                throw new PNException("Decrypt Error", ex);
            }
        }
    }
}
