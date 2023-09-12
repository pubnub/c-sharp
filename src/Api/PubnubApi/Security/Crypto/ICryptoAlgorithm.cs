using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PubnubApi.Security.Crypto
{
    public abstract class DataEnvelopeBase
    {
        public abstract string Data { get; set; }
    }
    public abstract class BytesEnvelopeBase
    {
        public abstract byte[] Data { get; set; }
    }
    public abstract class DataEnvelope : DataEnvelopeBase
    {
        public abstract string Metadata { get; set; }
    }
    public abstract class BytesEnvelope : BytesEnvelopeBase
    {
        public abstract byte[] Metadata { get; set; }
    }

    public class EncryptedData : DataEnvelope
    {
        public PNStatus Status { get; set; } //TODO: Need to identify the format to send error/success status
        public override string Data { get; set; }
        public override string Metadata { get; set; }
    }
    public class EncryptedBytes : BytesEnvelope
    {
        public PNStatus Status { get; set; } //TODO: Need to identify the format to send error/success status
        public override byte[] Data { get; set; }
        public override byte[] Metadata { get; set; }
    }
    public class DecryptedData : DataEnvelopeBase
    {
        public override string Data { get; set; }
        public PNStatus Status { get; set; } //TODO: Need to identify the format to send error/success status
    }

    public class DecryptedBytes : BytesEnvelopeBase
    {
        public override byte[] Data { get; set; }
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

        DecryptedData Decrypt(DataEnvelope encryptedData);
        DecryptedBytes Decrypt(BytesEnvelope encryptedBytes);
    }
}
