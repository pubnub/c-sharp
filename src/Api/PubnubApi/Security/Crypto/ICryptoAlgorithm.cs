using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PubnubApi.Security.Crypto
{
    public class EncryptedData
    {
        public PNStatus Status { get; set; } //TODO: Need to identify the format to send error/success status
        public string Data { get; set; }
    }
    public class EncryptedBytes
    {
        public PNStatus Status { get; set; } //TODO: Need to identify the format to send error/success status
        public byte[] Data { get; set; }
    }
    public class DecryptedData
    {
        public string Data { get; set; }
        public PNStatus Status { get; set; } //TODO: Need to identify the format to send error/success status
    }

    public class DecryptedBytes
    {
        public byte[] Data { get; set; }
        public PNStatus Status { get; set; } //TODO: Need to identify the format to send error/success status
    }
    public interface ICryptoAlgorithm
    {
        /// <summary>
        /// Unique crypto algorithm identifier.
        ///
        /// Identifier will be encoded into crypto data header and passed along
        /// with encrypted data.
        ///
        /// The identifier **must** be 4 bytes long.
        /// </summary>
        byte[] Identifier { get; }

        EncryptedData Encrypt(string data);
        EncryptedBytes Encrypt(byte[] data);

        DecryptedData Decrypt(string encryptedData);
        DecryptedBytes Decrypt(byte[] encryptedData);
    }
}
