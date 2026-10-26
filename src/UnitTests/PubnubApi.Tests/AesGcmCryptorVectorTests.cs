using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubNubMessaging.Tests
{
    /// <summary>
    /// Cross-SDK known-answer tests for the AES-256-GCM ("ACGM") cryptor. The vectors were produced by
    /// another PubNub SDK; decrypting them here must reproduce the original plaintext byte for byte.
    /// </summary>
    [TestFixture]
    public class AesGcmCryptorVectorTests
    {
        public class AcgmVector
        {
            public string Name;
            public string CipherKey;
            public string KeySha256;
            public string Plaintext;
            public string PlaintextHex;
            public string Nonce;
            public string Tag;
            public string Ciphertext;
            public string Metadata;
            public string Payload;

            public override string ToString() => Name;
        }

        private static readonly AcgmVector[] Vectors =
        {
            new AcgmVector
            {
                Name = "hello",
                CipherKey = "enigma",
                KeySha256 = "67a4f45f0d1d9bc606486fc42dc4941668e71d34ee500735fe9b7ea4625c687c",
                Plaintext = "Hello world",
                PlaintextHex = "48656c6c6f20776f726c64",
                Nonce = "0102030405060708090a0b0c",
                Tag = "dabd1d02d7219c77218ad262c4a9aecc",
                Ciphertext = "ea1b5780788b593294f898",
                Metadata = "0102030405060708090a0b0cdabd1d02d7219c77218ad262c4a9aecc",
                Payload = "504e4544014143474d1c0102030405060708090a0b0cdabd1d02d7219c77218ad262c4a9aeccea1b5780788b593294f898"
            },
            new AcgmVector
            {
                Name = "unicode",
                CipherKey = "enigma",
                KeySha256 = "67a4f45f0d1d9bc606486fc42dc4941668e71d34ee500735fe9b7ea4625c687c",
                Plaintext = "héllo 你好",
                PlaintextHex = "68c3a96c6c6f20e4bda0e5a5bd",
                Nonce = "111111111111111111111111",
                Tag = "a5dd072360f4433e2979f440fc7827b3",
                Ciphertext = "d138fbfc4bd0c337e29d9f2c98",
                Metadata = "111111111111111111111111a5dd072360f4433e2979f440fc7827b3",
                Payload = "504e4544014143474d1c111111111111111111111111a5dd072360f4433e2979f440fc7827b3d138fbfc4bd0c337e29d9f2c98"
            },
            new AcgmVector
            {
                Name = "binary",
                CipherKey = "myCipherKey",
                KeySha256 = "8c33871ac40eea7eb80fdd88465c48f0ee10054de5dfe77af9ed8c0ef626663e",
                Plaintext = null,
                PlaintextHex = "00ff7f80",
                Nonce = "000102030405060708090a0b",
                Tag = "8fefca7becf7235b4d09b970d4d56d19",
                Ciphertext = "c411f0f1",
                Metadata = "000102030405060708090a0b8fefca7becf7235b4d09b970d4d56d19",
                Payload = "504e4544014143474d1c000102030405060708090a0b8fefca7becf7235b4d09b970d4d56d19c411f0f1"
            }
        };

        private static IEnumerable<AcgmVector> AllVectors => Vectors;

        private static IEnumerable<AcgmVector> TextVectors => Vectors.Where(v => v.Plaintext != null);

        // "PNED" | version 1 | "ACGM" | cryptor data size 28
        private const string ExpectedHeaderHex = "504e4544" + "01" + "4143474d" + "1c";
        private const int HeaderLength = 10;
        private const int NonceLength = 12;

        [TestCaseSource(nameof(AllVectors))]
        public void KeyDerivationMatchesVector(AcgmVector vector)
        {
            byte[] key;
            using (SHA256 sha256 = SHA256.Create())
            {
                key = sha256.ComputeHash(Encoding.UTF8.GetBytes(vector.CipherKey));
            }

            Assert.AreEqual(vector.KeySha256, ToHex(key));
        }

        [TestCaseSource(nameof(AllVectors))]
        public void PayloadLayoutMatchesSpecification(AcgmVector vector)
        {
            Assert.AreEqual(vector.Nonce + vector.Tag, vector.Metadata, "metadata must be nonce followed by tag");
            Assert.AreEqual(ExpectedHeaderHex + vector.Metadata + vector.Ciphertext, vector.Payload);

            CryptorHeader header = CryptorHeader.FromBytes(FromHex(vector.Payload));
            Assert.AreEqual("ACGM", header.Identifier);
            Assert.AreEqual(28, header.DataSize);
        }

        [TestCaseSource(nameof(TextVectors))]
        public void PlaintextHexIsUtf8OfPlaintext(AcgmVector vector)
        {
            Assert.AreEqual(vector.PlaintextHex, ToHex(Encoding.UTF8.GetBytes(vector.Plaintext)));
        }

        [TestCaseSource(nameof(AllVectors))]
        public void DecryptBytesReproducesPlaintext(AcgmVector vector)
        {
            byte[] decrypted = new AesGcmCryptor(vector.CipherKey).Decrypt(FromHex(vector.Payload));

            Assert.AreEqual(vector.PlaintextHex, ToHex(decrypted));
        }

        [TestCaseSource(nameof(TextVectors))]
        public void DecryptStringReproducesPlaintext(AcgmVector vector)
        {
            string payloadBase64 = Convert.ToBase64String(FromHex(vector.Payload));

            string decrypted = new AesGcmCryptor(vector.CipherKey).Decrypt(payloadBase64);

            Assert.AreEqual(vector.Plaintext, decrypted);
        }

        [TestCaseSource(nameof(AllVectors))]
        public void CryptoModuleRoutesVectorToAesGcmCryptor(AcgmVector vector)
        {
            CryptoModule module = new CryptoModule(new AesCbcCryptor(vector.CipherKey),
                new List<ICryptor> { new LegacyCryptor(vector.CipherKey), new AesGcmCryptor(vector.CipherKey) });

            byte[] decrypted = module.Decrypt(FromHex(vector.Payload));

            Assert.AreEqual(vector.PlaintextHex, ToHex(decrypted));
        }

        [TestCaseSource(nameof(AllVectors))]
        public void TamperedVectorFailsAuthentication(AcgmVector vector)
        {
            byte[] payload = FromHex(vector.Payload);
            AesGcmCryptor cryptor = new AesGcmCryptor(vector.CipherKey);
            int tagOffset = HeaderLength + NonceLength;

            var failures = new List<PNException>
            {
                Assert.Throws<PNException>(() => cryptor.Decrypt(FlipByte(payload, HeaderLength))),
                Assert.Throws<PNException>(() => cryptor.Decrypt(FlipByte(payload, tagOffset))),
                Assert.Throws<PNException>(() => cryptor.Decrypt(FlipByte(payload, payload.Length - 1))),
                Assert.Throws<PNException>(() => new AesGcmCryptor(vector.CipherKey + "x").Decrypt(payload))
            };

            foreach (PNException ex in failures)
            {
                Assert.AreEqual("Decrypt Error", ex.Message);
            }
        }

        private static byte[] FromHex(string hex) => Convert.FromHexString(hex);

        private static string ToHex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();

        private static byte[] FlipByte(byte[] data, int index)
        {
            byte[] copy = (byte[])data.Clone();
            copy[index] ^= 0x01;
            return copy;
        }
    }
}
