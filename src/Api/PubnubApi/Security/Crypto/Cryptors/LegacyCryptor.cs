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
        private const string IDENTIFIER = "0000";

        private readonly bool _useDynamicRandomIV;
        public LegacyCryptor(string cipherKey, bool useDynamicRandomIV, IPubnubLog log): base(cipherKey, useDynamicRandomIV, log)
        {
            _useDynamicRandomIV = useDynamicRandomIV;
        }
        public LegacyCryptor(string cipherKey, bool useDynamicRandomIV): this(cipherKey, useDynamicRandomIV, null)
        {
        }
        public LegacyCryptor(string cipherKey): this(cipherKey, true)
        {
        }

        public new string CipherKey => base.CipherKey;
        public bool UseDynamicRandomIV => _useDynamicRandomIV;
        public override string Identifier => IDENTIFIER;
        public override string Encrypt(string data)
        {
            if (data == null) { throw new ArgumentException("Invalid input","data"); }
            if (data.Length == 0) { throw new PNException("encryption error"); }
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
            catch(PNException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PNException("Encrypt Error", ex);
            }
        }
        public override byte[] Encrypt(byte[] data)
        {
            if (data == null) { throw new ArgumentException("Invalid input","data"); }
            if (data.Length == 0) { throw new PNException("encryption error"); }
            try
            {
                byte[] ivBytes = GenerateRandomIV(_useDynamicRandomIV);
                Log(string.Format(CultureInfo.InvariantCulture, "DateTime {0} IV = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ivBytes.ToDisplayFormat()));
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
                return InternalEncrypt(false, data, ivBytes, keyBytes);
            }
            catch(PNException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PNException("Encrypt Error", ex);
            }
        }
        public override string Decrypt(string encryptedData)
        {
            if (encryptedData == null) { throw new ArgumentException("Invalid input","encryptedData"); }
            if (encryptedData.Length == 0) { throw new PNException("decryption error"); }
            try
            {
                byte[] dataBytes = Convert.FromBase64String(encryptedData);
                byte[] ivBytes = _useDynamicRandomIV ? dataBytes.Take(16).ToArray() : Encoding.UTF8.GetBytes("0123456789012345");
                dataBytes = _useDynamicRandomIV ? dataBytes.Skip(16).ToArray() : dataBytes;
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
                byte[] decryptedBytes = InternalDecrypt(dataBytes, ivBytes, keyBytes);
                return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
            }
            catch(PNException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new PNException("Decrypt Error", ex);
            }
        }
        public override byte[] Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null) { throw new ArgumentException("Invalid input","encryptedData"); }
            if (encryptedData.Length == 0) { throw new PNException("decryption error"); }
            try
            {
                byte[] ivBytes = _useDynamicRandomIV ? encryptedData.Take(16).ToArray() : Encoding.UTF8.GetBytes("0123456789012345");
                byte[] dataBytes = _useDynamicRandomIV ? encryptedData.Skip(16).ToArray() : encryptedData;
                if (dataBytes.Length == 0) { throw new PNException("decryption error"); }
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
                return InternalDecrypt(dataBytes, ivBytes, keyBytes);
            }
            catch(PNException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new PNException("Decrypt Error", ex);
            }
        }

        public override void EncryptFile(string sourceFile, string destinationFile)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("sourceFile is not valid");
            }
            #if !NETSTANDARD10 && !NETSTANDARD11
            bool validSource = System.IO.File.Exists(sourceFile);
            if (!validSource)
            {
                throw new ArgumentException("sourceFile is not valid");
            }
            string destDirectory = System.IO.Path.GetDirectoryName(destinationFile);
            bool validDest = System.IO.Directory.Exists(destDirectory);
            if (!string.IsNullOrEmpty(destDirectory) && !validDest)
            {
                throw new ArgumentException("destination path is not valid");
            }
            byte[] inputBytes = System.IO.File.ReadAllBytes(sourceFile);
            byte[] outputBytes = Encrypt(inputBytes);
            System.IO.File.WriteAllBytes(destinationFile, outputBytes);
            #else
            throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
            #endif
        }

        public override void DecryptFile(string sourceFile, string destinationFile)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("sourceFile is not valid");
            }
            #if !NETSTANDARD10 && !NETSTANDARD11
            bool validSource = System.IO.File.Exists(sourceFile);
            if (!validSource)
            {
                throw new ArgumentException("sourceFile is not valid");
            }
            string destDirectory = System.IO.Path.GetDirectoryName(destinationFile);
            bool validDest = System.IO.Directory.Exists(destDirectory);
            if (!string.IsNullOrEmpty(destDirectory) && !validDest)
            {
                throw new ArgumentException("destination path is not valid");
            }
            byte[] inputBytes = System.IO.File.ReadAllBytes(sourceFile);
            byte[] outputBytes = Decrypt(inputBytes);
            System.IO.File.WriteAllBytes(destinationFile, outputBytes);
            #else
            throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
            #endif
        }
    }
}
