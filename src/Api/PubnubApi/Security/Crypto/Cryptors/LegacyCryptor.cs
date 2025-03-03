using PubnubApi.Security.Crypto.Common;
using System;
using System.Linq;
using System.Text;

namespace PubnubApi.Security.Crypto.Cryptors
{
    public class LegacyCryptor : CryptorBase
    {
        private const string IDENTIFIER = "0000";

        private readonly bool _useDynamicRandomIV;
        public LegacyCryptor(string cipherKey, bool useDynamicRandomIV, PubnubLogModule logger): base(cipherKey, useDynamicRandomIV, logger)
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
            if (data == null)
            {
                logger.Debug("LegacyCryptor encrypt data input is null");
                throw new ArgumentException("Invalid input","data");
            }

            if (data.Length == 0)
            {
                logger.Debug("LegacyCryptor encrypt data input is of zero length");
                throw new PNException("encryption error");
            }
            try
            {
                logger.Debug($"LegacyCryptor Encrypting string data: {data}");
                string input = Util.EncodeNonAsciiCharacters(data);
                byte[] dataBytes = Encoding.UTF8.GetBytes(input);
                byte[] ivBytes = GenerateRandomIV(_useDynamicRandomIV);
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
                byte[] encryptedBytes = InternalEncrypt(false, dataBytes, ivBytes, keyBytes);
                logger.Debug($"LegacyCryptor Data encrypted successfully");
                return Convert.ToBase64String(encryptedBytes);
            }
            catch(PNException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"LegacyCryptor Error while encrypting data. ErrorMessage {ex.Message}, StackTrace {ex.StackTrace}");
                throw new PNException("Encrypt Error", ex);
            }
        }
        public override byte[] Encrypt(byte[] data)
        {
            if (data == null) { throw new ArgumentException("Invalid input","data"); }
            if (data.Length == 0) { throw new PNException("encryption error"); }
            try
            {
                logger.Debug($"LegacyCryptor Encrypting bytes data. Length {data.Length}");
                byte[] ivBytes = GenerateRandomIV(_useDynamicRandomIV);
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
                logger.Debug($"LegacyCryptor Bytes Data encrypted successfully");
                return InternalEncrypt(false, data, ivBytes, keyBytes);
            }
            catch(PNException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"LegacyCryptor Error while encrypting data bytes. ErrorMessage {ex.Message}, StackTrace {ex.StackTrace}");
                throw new PNException("Encrypt Error", ex);
            }
        }
        public override string Decrypt(string encryptedData)
        {
            if (encryptedData == null)
            {
                logger.Debug("LegacyCryptor decrypt data string input is null");
                throw new ArgumentException("Invalid input","encryptedData");
            }

            if (encryptedData.Length == 0)
            {
                logger.Debug("LegacyCryptor decrypt data string input is of zero length");
                throw new PNException("decryption error");
            }
            try
            {
                logger.Debug($"LegacyCryptor Decrypting string data: {encryptedData}");
                byte[] dataBytes = Convert.FromBase64String(encryptedData);
                byte[] ivBytes = _useDynamicRandomIV ? dataBytes.Take(16).ToArray() : Encoding.UTF8.GetBytes("0123456789012345");
                dataBytes = _useDynamicRandomIV ? dataBytes.Skip(16).ToArray() : dataBytes;
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
                byte[] decryptedBytes = InternalDecrypt(dataBytes, ivBytes, keyBytes);
                logger.Debug($"LegacyCryptor String data decrypted successfully");
                return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
            }
            catch(PNException)
            {
                throw;
            }
            catch(Exception ex)
            {
                logger.Error($"LegacyCryptor Error while decrypting string data. ErrorMessage {ex.Message}, StackTrace {ex.StackTrace}");
                throw new PNException("Decrypt Error", ex);
            }
        }
        public override byte[] Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null)
            {
                logger.Debug("LegacyCryptor decrypt data bytes input is null");
                throw new ArgumentException("Invalid input","encryptedData");
            }

            if (encryptedData.Length == 0)
            {
                logger.Debug("LegacyCryptor decrypt data bytes input is of zero length");
                throw new PNException("decryption error");
            }
            try
            {
                logger.Debug($"LegacyCryptor Decrypting bytes data. Length {encryptedData.Length}");
                byte[] ivBytes = _useDynamicRandomIV ? encryptedData.Take(16).ToArray() : Encoding.UTF8.GetBytes("0123456789012345");
                byte[] dataBytes = _useDynamicRandomIV ? encryptedData.Skip(16).ToArray() : encryptedData;
                if (dataBytes.Length == 0) { throw new PNException("decryption error"); }
                byte[] keyBytes = Util.GetLegacyEncryptionKey(CipherKey);
                logger.Debug($"LegacyCryptor Bytes Data decrypted successfully");
                return InternalDecrypt(dataBytes, ivBytes, keyBytes);
            }
            catch(PNException)
            {
                throw;
            }
            catch(Exception ex)
            {
                logger.Error($"LegacyCryptor Error while decrypting bytes. ErrorMessage {ex.Message}, StackTrace {ex.StackTrace}");
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
            logger.Debug("LegacyCryptor encrypting file");
            byte[] outputBytes = Encrypt(inputBytes);
            logger.Debug("LegacyCryptor file encrypted successfully");
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
            logger.Debug("LegacyCryptor decrypting file");
            byte[] outputBytes = Decrypt(inputBytes);
            logger.Debug("LegacyCryptor file decrypted successfully");
            System.IO.File.WriteAllBytes(destinationFile, outputBytes);
            #else
            throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
            #endif
        }
    }
}
