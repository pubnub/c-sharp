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
    /// Cross-SDK known-answer tests for the AES-256-GCM ("ACGM") cryptor. The vectors were generated with
    /// node:crypto, independently of this SDK; decrypting them here must reproduce the original plaintext
    /// byte for byte.
    /// Layout: "PNED" | 0x01 | "ACGM" | 0x0C | nonce (12) | cipher text | tag (16).
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
            public string CiphertextWithTag;
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
                CiphertextWithTag = "ea1b5780788b593294f898dabd1d02d7219c77218ad262c4a9aecc",
                Metadata = "0102030405060708090a0b0c",
                Payload = "504e4544014143474d0c0102030405060708090a0b0cea1b5780788b593294f898dabd1d02d7219c77218ad262c4a9aecc"
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
                CiphertextWithTag = "d138fbfc4bd0c337e29d9f2c98a5dd072360f4433e2979f440fc7827b3",
                Metadata = "111111111111111111111111",
                Payload = "504e4544014143474d0c111111111111111111111111d138fbfc4bd0c337e29d9f2c98a5dd072360f4433e2979f440fc7827b3"
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
                CiphertextWithTag = "c411f0f18fefca7becf7235b4d09b970d4d56d19",
                Metadata = "000102030405060708090a0b",
                Payload = "504e4544014143474d0c000102030405060708090a0bc411f0f18fefca7becf7235b4d09b970d4d56d19"
            },
            new AcgmVector
            {
                Name = "single-byte",
                CipherKey = "customKey",
                KeySha256 = "128cf0f9f9027c04c79c0ff574341db71cb7a7545c40469d59804bf78377f215",
                Plaintext = "a",
                PlaintextHex = "61",
                Nonce = "a1a2a3a4a5a6a7a8a9aaabac",
                Tag = "eef1ccce8f5a09087348786b99b75076",
                Ciphertext = "fb",
                CiphertextWithTag = "fbeef1ccce8f5a09087348786b99b75076",
                Metadata = "a1a2a3a4a5a6a7a8a9aaabac",
                Payload = "504e4544014143474d0ca1a2a3a4a5a6a7a8a9aaabacfbeef1ccce8f5a09087348786b99b75076"
            },
            new AcgmVector
            {
                Name = "one-block",
                CipherKey = "enigma",
                KeySha256 = "67a4f45f0d1d9bc606486fc42dc4941668e71d34ee500735fe9b7ea4625c687c",
                Plaintext = "0123456789abcdef",
                PlaintextHex = "30313233343536373839616263646566",
                Nonce = "8f5a2d1b6e9c4ef0a37d83b9",
                Tag = "8c21b92d48f4dd5c71e99dce881c2ec9",
                Ciphertext = "fd24a488008bd084305aaee605b34902",
                CiphertextWithTag = "fd24a488008bd084305aaee605b349028c21b92d48f4dd5c71e99dce881c2ec9",
                Metadata = "8f5a2d1b6e9c4ef0a37d83b9",
                Payload = "504e4544014143474d0c8f5a2d1b6e9c4ef0a37d83b9fd24a488008bd084305aaee605b349028c21b92d48f4dd5c71e99dce881c2ec9"
            },
            new AcgmVector
            {
                Name = "multi-block-json",
                CipherKey = "enigma",
                KeySha256 = "67a4f45f0d1d9bc606486fc42dc4941668e71d34ee500735fe9b7ea4625c687c",
                // U+1F30D (globe showing Europe-Africa) is written as an escape so the exact bytes survive editing.
                Plaintext = "{\"text\":\"Hello, 世界! \U0001F30D\",\"n\":42,\"list\":[1,2,3],\"nested\":{\"ok\":true}}",
                PlaintextHex = "7b2274657874223a2248656c6c6f2c20e4b896e7958c2120f09f8c8d222c226e223a34322c226c697374223a5b312c322c335d2c226e6573746564223a7b226f6b223a747275657d7d",
                Nonce = "ffffffffffffffffffffffff",
                Tag = "d307669e83fa4c9a8b13743a07a21b7f",
                Ciphertext = "0d54b73ce2509d4e7655043f5b103cb4f1e78738a73a2f56109813955359b4784774de19050b4f14f7a439a859ac6c327856fe1af874bbe888945c603004176ddab22578e407663346",
                CiphertextWithTag = "0d54b73ce2509d4e7655043f5b103cb4f1e78738a73a2f56109813955359b4784774de19050b4f14f7a439a859ac6c327856fe1af874bbe888945c603004176ddab22578e407663346d307669e83fa4c9a8b13743a07a21b7f",
                Metadata = "ffffffffffffffffffffffff",
                Payload = "504e4544014143474d0cffffffffffffffffffffffff0d54b73ce2509d4e7655043f5b103cb4f1e78738a73a2f56109813955359b4784774de19050b4f14f7a439a859ac6c327856fe1af874bbe888945c603004176ddab22578e407663346d307669e83fa4c9a8b13743a07a21b7f"
            },
            new AcgmVector
            {
                Name = "unicode-key",
                // U+1F511 (key) is written as an escape so the exact bytes survive editing.
                CipherKey = "ключ \U0001F511 key",
                KeySha256 = "c87d40b9e09784a5d672976e22b6bc9701da1e6fe5b8db721ed9ce715aedbdb0",
                Plaintext = "Line one\n\tLine two with \"quotes\" & symbols <>/\\ \u2014 spans several AES blocks.",
                PlaintextHex = "4c696e65206f6e650a094c696e652074776f2077697468202271756f7465732220262073796d626f6c73203c3e2f5c20e28094207370616e73207365766572616c2041455320626c6f636b732e",
                Nonce = "000000000000000000000000",
                Tag = "8390572d9b05255f3097614e888fcca4",
                Ciphertext = "326052d9ff1b757ed8810f5d6f87a7c021920eb09b82e47049efecf5094f6f5837691e9fb66539dfc47d434cc01b783527c4d62ae2d8163043f86dc7ac405b90971ceaa07134add2be50dfd60f",
                CiphertextWithTag = "326052d9ff1b757ed8810f5d6f87a7c021920eb09b82e47049efecf5094f6f5837691e9fb66539dfc47d434cc01b783527c4d62ae2d8163043f86dc7ac405b90971ceaa07134add2be50dfd60f8390572d9b05255f3097614e888fcca4",
                Metadata = "000000000000000000000000",
                Payload = "504e4544014143474d0c000000000000000000000000326052d9ff1b757ed8810f5d6f87a7c021920eb09b82e47049efecf5094f6f5837691e9fb66539dfc47d434cc01b783527c4d62ae2d8163043f86dc7ac405b90971ceaa07134add2be50dfd60f8390572d9b05255f3097614e888fcca4"
            }
        };

        private static IEnumerable<AcgmVector> AllVectors => Vectors;

        private static IEnumerable<AcgmVector> TextVectors => Vectors.Where(v => v.Plaintext != null);

        // "PNED" | version 1 | "ACGM" | cryptor data size 12 (nonce only)
        private const string ExpectedHeaderHex = "504e4544" + "01" + "4143474d" + "0c";
        private const int HeaderLength = 10;
        private const int NonceLength = 12;
        private const int TagLength = 16;

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
            Assert.AreEqual(vector.Nonce, vector.Metadata, "metadata must contain only the nonce");
            Assert.AreEqual(vector.Ciphertext + vector.Tag, vector.CiphertextWithTag, "tag must be appended to the cipher text");
            Assert.AreEqual(ExpectedHeaderHex + vector.Metadata + vector.CiphertextWithTag, vector.Payload);

            CryptorHeader header = CryptorHeader.FromBytes(FromHex(vector.Payload));
            Assert.AreEqual("ACGM", header.Identifier);
            Assert.AreEqual(NonceLength, header.DataSize);
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
            int cipherTextOffset = HeaderLength + NonceLength;
            int tagOffset = payload.Length - TagLength;

            var failures = new List<PNException>
            {
                Assert.Throws<PNException>(() => cryptor.Decrypt(FlipByte(payload, HeaderLength))),
                Assert.Throws<PNException>(() => cryptor.Decrypt(FlipByte(payload, cipherTextOffset))),
                Assert.Throws<PNException>(() => cryptor.Decrypt(FlipByte(payload, tagOffset))),
                Assert.Throws<PNException>(() => cryptor.Decrypt(FlipByte(payload, payload.Length - 1))),
                Assert.Throws<PNException>(() => new AesGcmCryptor(vector.CipherKey + "x").Decrypt(payload))
            };

            foreach (PNException ex in failures)
            {
                Assert.AreEqual("Decrypt Error", ex.Message);
            }
        }

        [TestCaseSource(nameof(AllVectors))]
        public void PreviousTagFirstLayoutIsRejected(AcgmVector vector)
        {
            // Same nonce, tag and cipher text, but in the superseded layout: size 28 | nonce | tag | cipher text.
            string previousLayout = "504e4544014143474d1c" + vector.Nonce + vector.Tag + vector.Ciphertext;

            Assert.Throws<PNException>(() => new AesGcmCryptor(vector.CipherKey).Decrypt(FromHex(previousLayout)));
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
