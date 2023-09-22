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

        public AesCbcCryptor(string cipherKey, IPubnubLog log): base(cipherKey, true, log)
        {
        }
        public AesCbcCryptor(string cipherKey): this(cipherKey, null) { }
        public new string CipherKey => base.CipherKey;
        public override string Identifier => IDENTIFIER;
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
                if (header == null || !header.Identifier.Equals(Identifier))
                {
                    throw new PNException("unknown cryptor error");
                }
                string actualData = Convert.ToBase64String(Convert.FromBase64String(encryptedData).Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray());
                byte[] dataBytes = Convert.FromBase64String(actualData);
                byte[] ivBytes = dataBytes.Take(16).ToArray();
                dataBytes = dataBytes.Skip(16).ToArray();
                byte[] keyBytes = Util.GetEncryptionKeyBytes(CipherKey);
                byte[] decryptedBytes = InternalDecrypt(dataBytes, ivBytes, keyBytes);
                return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
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
                if (header == null || !header.Identifier.Equals(Identifier))
                {
                    throw new PNException("unknown cryptor error");
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
