using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using PubnubApi.Security.Crypto.Common;

namespace PubnubApi.Security.Crypto.Cryptors;

/// <summary>
/// AES-256-GCM cryptor (identifier "ACGM").
///
/// Currently decrypt-only: it can be registered as an additional decryptor in a
/// <see cref="CryptoModule"/>, but cannot be used as the default (encrypting) cryptor.
/// </summary>
public class AesGcmCryptor : CryptorBase
{
    private const string IDENTIFIER = "ACGM";
    private const int NONCE_SIZE = 12;
    private const int TAG_SIZE = 16;
    private const int CRYPTOR_DATA_SIZE = NONCE_SIZE + TAG_SIZE;
    private const string ENCRYPTION_NOT_SUPPORTED = "AesGcmCryptor is decrypt-only. Encryption is not supported yet.";

    public AesGcmCryptor(string cipherKey, PubnubLogModule logger) : base(cipherKey, logger)
    {
    }

    public AesGcmCryptor(string cipherKey) : this(cipherKey, null)
    {
    }

    public override string Identifier => IDENTIFIER;

    public override string Encrypt(string data)
    {
        throw new NotSupportedException(ENCRYPTION_NOT_SUPPORTED);
    }

    public override byte[] Encrypt(byte[] data)
    {
        throw new NotSupportedException(ENCRYPTION_NOT_SUPPORTED);
    }

    public override void EncryptFile(string sourceFile, string destinationFile)
    {
        throw new NotSupportedException(ENCRYPTION_NOT_SUPPORTED);
    }

    public override string Decrypt(string encryptedData)
    {
        if (encryptedData == null)
        {
            logger?.Debug("AesGcmCryptor decrypt data string input is null");
            throw new ArgumentException("Invalid input", nameof(encryptedData));
        }
        if (encryptedData.Length == 0)
        {
            logger?.Debug("AesGcmCryptor decrypt data string input is of zero length");
            throw new PNException("decryption error");
        }

        byte[] encryptedBytes;
        try
        {
            encryptedBytes = Convert.FromBase64String(encryptedData);
        }
        catch (FormatException ex)
        {
            logger?.Error($"AesGcmCryptor Error while decoding string data. ErrorMessage {ex.Message}");
            throw new PNException("Decrypt Error");
        }

        byte[] decryptedBytes = DecryptData(encryptedBytes);
        logger?.Debug("AesGcmCryptor String data decrypted successfully");
        return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
    }

    public override byte[] Decrypt(byte[] encryptedData)
    {
        if (encryptedData == null)
        {
            logger?.Debug("AesGcmCryptor decrypt data bytes input is null");
            throw new ArgumentException("Invalid input", nameof(encryptedData));
        }
        if (encryptedData.Length == 0)
        {
            logger?.Debug("AesGcmCryptor decrypt data bytes input is of zero length");
            throw new PNException("decryption error");
        }

        byte[] decryptedBytes = DecryptData(encryptedData);
        logger?.Debug("AesGcmCryptor Bytes data decrypted successfully");
        return decryptedBytes;
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
        logger?.Debug("AesGcmCryptor decrypting file");
        byte[] outputBytes = Decrypt(inputBytes);
        logger?.Debug("AesGcmCryptor file decrypted successfully");
        System.IO.File.WriteAllBytes(destinationFile, outputBytes);
#else
        throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
#endif
    }

    private byte[] DecryptData(byte[] encryptedData)
    {
        try
        {
            CryptorHeader header = CryptorHeader.FromBytes(encryptedData);
            if (header == null || !header.Identifier.Equals(Identifier))
            {
                throw new PNException("unknown cryptor error");
            }
            if (header.DataSize != CRYPTOR_DATA_SIZE)
            {
                throw new PNException("decryption error");
            }

            int headerLength = 4 + 1 + header.Identifier.Length + (header.DataSize < 255 ? 1 : 3);
            int cipherTextOffset = headerLength + CRYPTOR_DATA_SIZE;
            int cipherTextLength = encryptedData.Length - cipherTextOffset;
            if (cipherTextLength <= 0)
            {
                throw new PNException("decryption error");
            }

            logger?.Debug($"AesGcmCryptor Decrypting bytes data. Length {encryptedData.Length}");
            byte[] nonce = new byte[NONCE_SIZE];
            Buffer.BlockCopy(encryptedData, headerLength, nonce, 0, NONCE_SIZE);

            // BouncyCastle expects the authentication tag appended to the cipher text.
            byte[] cipherTextWithTag = new byte[cipherTextLength + TAG_SIZE];
            Buffer.BlockCopy(encryptedData, cipherTextOffset, cipherTextWithTag, 0, cipherTextLength);
            Buffer.BlockCopy(encryptedData, headerLength + NONCE_SIZE, cipherTextWithTag, cipherTextLength, TAG_SIZE);

            byte[] keyBytes = Util.GetEncryptionKeyBytes(CipherKey);
            return GcmDecrypt(cipherTextWithTag, nonce, keyBytes);
        }
        catch (PNException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger?.Error($"AesGcmCryptor Error while decrypting bytes. ErrorMessage {ex.Message}, StackTrace {ex.StackTrace}");
            throw new PNException("Decrypt Error");
        }
    }

    private byte[] GcmDecrypt(byte[] cipherTextWithTag, byte[] nonce, byte[] keyBytes)
    {
        GcmBlockCipher gcm = new GcmBlockCipher(AesUtilities.CreateEngine());
        gcm.Init(false, new AeadParameters(new KeyParameter(keyBytes), TAG_SIZE * 8, nonce));
        byte[] output = new byte[gcm.GetOutputSize(cipherTextWithTag.Length)];
        try
        {
            int length = gcm.ProcessBytes(cipherTextWithTag, 0, cipherTextWithTag.Length, output, 0);
            length += gcm.DoFinal(output, length);
            if (length == output.Length)
            {
                return output;
            }
            byte[] result = new byte[length];
            Buffer.BlockCopy(output, 0, result, 0, length);
            return result;
        }
        catch (InvalidCipherTextException ex)
        {
            // ProcessBytes may already have written plaintext that failed authentication, so it
            // must never leave this method. Wrong key, tampered data, nonce or tag all surface as
            // the same generic error; the specific cause is kept in the internal log only.
            Array.Clear(output, 0, output.Length);
            logger?.Error($"AesGcmCryptor Decryption failed: {ex.Message}");
            throw new PNException("Decrypt Error");
        }
    }
}
