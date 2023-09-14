using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PubnubApi.Security.Crypto
{
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

        string Encrypt(string data);
        byte[] Encrypt(byte[] data);

        string Decrypt(string encryptedData);
        byte[] Decrypt(byte[] encryptedData);
    }
}
