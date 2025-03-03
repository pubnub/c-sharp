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

        public AesCbcCryptor(string cipherKey, PubnubLogModule logger): base(cipherKey, true, logger)
        {
        }
        public AesCbcCryptor(string cipherKey): this(cipherKey, null) { }
        public new string CipherKey => base.CipherKey;
        public override string Identifier => IDENTIFIER;
        public override string Encrypt(string data)
        {
            if (data == null)
            {
                logger.Debug("AesCbcCryptor encrypt data input is null");
                throw new ArgumentException("Invalid input", data);
            }

            if (data.Length == 0)
            {
                logger.Debug("AesCbcCryptor encrypt data input is of zero length");
                throw new PNException($"encryption error empty input");
            }
            try
            {
                logger.Debug($"AesCbcCryptor Encrypting string data: {data}");
                string input = Util.EncodeNonAsciiCharacters(data);
                byte[] dataBytes = Encoding.UTF8.GetBytes(input);
                byte[] ivBytes = GenerateRandomIV(true);
                byte[] keyBytes = Util.GetEncryptionKeyBytes(CipherKey);
                byte[] encryptedBytes = InternalEncrypt(true, dataBytes, ivBytes, keyBytes);
                logger.Debug($"AesCbcCryptor Data encrypted successfully");
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                logger.Error($"AesCbcCryptor Error while encrypting data. ErrorMessage {ex.Message}, StackTrace {ex.StackTrace}");
                throw new PNException("Encrypt Error", ex);
            }
        }
        public override byte[] Encrypt(byte[] data)
        {
            if (data == null) { throw new ArgumentException("Invalid input","data"); }
            if (data.Length == 0) { throw new PNException("encryption error"); }
            try
            {
                logger.Debug($"AesCbcCryptor Encrypting bytes data. Length {data.Length}");
                byte[] ivBytes = GenerateRandomIV(true);
                byte[] keyBytes = Util.GetEncryptionKeyBytes(CipherKey);
                logger.Debug($"AesCbcCryptor Bytes Data encrypted successfully");
                return InternalEncrypt(true, data, ivBytes, keyBytes);
            }
            catch (Exception ex)
            {
                logger.Error($"AesCbcCryptor Error while encrypting data bytes. ErrorMessage {ex.Message}, StackTrace {ex.StackTrace}");
                throw new PNException("Encrypt Error", ex);
            }
        }
        public override string Decrypt(string encryptedData)
        {
            if (encryptedData == null)
            {
                logger.Debug("AesCbcCryptor decrypt data string input is null");
                throw new ArgumentException("Invalid input","encryptedData");
            }

            if (encryptedData.Length == 0)
            {
                logger.Debug("AesCbcCryptor decrypt data string input is of zero length");
                throw new PNException("decryption error");
            }
            try
            {
                CryptorHeader header = CryptorHeader.FromBytes(Convert.FromBase64String(encryptedData));
                if (header == null || !header.Identifier.Equals(Identifier))
                {
                    throw new PNException("unknown cryptor error");
                }
                logger.Debug($"AesCbcCryptor Decrypting string data: {encryptedData}");
                string actualData = Convert.ToBase64String(Convert.FromBase64String(encryptedData).Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray());
                byte[] dataBytes = Convert.FromBase64String(actualData);
                byte[] ivBytes = dataBytes.Take(16).ToArray();
                dataBytes = dataBytes.Skip(16).ToArray();
                byte[] keyBytes = Util.GetEncryptionKeyBytes(CipherKey);
                byte[] decryptedBytes = InternalDecrypt(dataBytes, ivBytes, keyBytes);
                logger.Debug($"AesCbcCryptor String data decrypted successfully");
                return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
            }
            catch(PNException)
            {
                throw;
            }
            catch(Exception ex)
            {
                logger.Error($"AesCbcCryptor Error while decrypting string data. ErrorMessage {ex.Message}, StackTrace {ex.StackTrace}");
                throw new PNException("Decrypt Error", ex);
            }
        }
        public override byte[] Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null)
            {
                logger.Debug("AesCbcCryptor decrypt data bytes input is null");
                throw new ArgumentException("Invalid input","encryptedData");
            }

            if (encryptedData.Length == 0)
            {
                logger.Debug("AesCbcCryptor decrypt data bytes input is of zero length");
                throw new PNException("decryption error");
            }
            try
            {
                CryptorHeader header = CryptorHeader.FromBytes(encryptedData);
                if (header == null || !header.Identifier.Equals(Identifier))
                {
                    throw new PNException("unknown cryptor error");
                }
                logger.Debug($"AesCbcCryptor Decrypting bytes data. Length {encryptedData.Length}");
                byte[] actualBytes = encryptedData.Skip(5 + header.Identifier.Length + ((header.DataSize < 255) ? 1 : 3)).ToArray();
                byte[] ivBytes = actualBytes.Take(header.DataSize).ToArray();
                byte[] dataBytes = actualBytes.Skip(header.DataSize).ToArray();
                if (dataBytes.Length == 0) { throw new PNException("decryption error"); }
                byte[] keyBytes = Util.GetEncryptionKeyBytes(CipherKey);
                logger.Debug($"AesCbcCryptor Bytes Data decrypted successfully");
                return InternalDecrypt(dataBytes, ivBytes, keyBytes);
            }
            catch(PNException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"AesCbcCryptor Error while decrypting bytes. ErrorMessage {ex.Message}, StackTrace {ex.StackTrace}");
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
            logger.Debug("AesCbcCryptor encrypting file");
            byte[] outputBytes = Encrypt(inputBytes);
            logger.Debug("AesCbcCryptor file encrypted successfully");
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
            logger.Debug("AesCbcCryptor decrypting file");
            byte[] outputBytes = Decrypt(inputBytes);
            logger.Debug("AesCbcCryptor file decrypted successfully");
            System.IO.File.WriteAllBytes(destinationFile, outputBytes);
            #else
            throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
            #endif
        }
    }
}
