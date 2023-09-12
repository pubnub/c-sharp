using NUnit.Framework;
using PubnubApi;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class CryptorTests
    {
        [Test]
        public void TestYayDecryptionBasic()
        {
            string message = "q/xJqqN6qbiZMXYmiQC1Fw==";
            DataEnvelope envelope = new EncryptedData() { Metadata = null, Data = message };

            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", false));
            DecryptedData decryptedMessage = cryptor.Decrypt(envelope);
            
            Assert.AreEqual("yay!", decryptedMessage.Data);
        }

        [Test]
        public void TestYayDecryptionBasicWithDynamicIV()
        {
            string message = "MTIzNDU2Nzg5MDEyMzQ1NjdnONoCgo0wbuMGGMmfMX0=";
            DataEnvelope envelope = new EncryptedData() { Metadata = null, Data = message };
            
            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", true));
            DecryptedData decryptedMessage = cryptor.Decrypt(envelope);
            
            Assert.AreEqual("yay!", decryptedMessage.Data);
        }

        [Test]
        public void TestYayByteArrayDecryptionBasic()
        {
            byte[] message = new byte[] { 171, 252, 73, 170, 163, 122, 169, 184, 153, 49, 118, 38, 137, 0, 181, 23 };
            BytesEnvelope envelope = new EncryptedBytes() { Metadata = null, Data = message };

            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", false));
            DecryptedBytes decryptedMessage = cryptor.Decrypt(envelope);

            byte[] expectedBytes = new byte[] { 121, 97, 121, 33 };
            Assert.AreEqual(expectedBytes, decryptedMessage.Data);
        }

        [Test]
        public void TestYayByteArrayDecryptionBasicWithDynamicIV()
        {
            byte[] message = new byte[] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54, 55, 103, 56, 218, 2, 130, 141, 48, 110, 227, 6, 24, 201, 159, 49, 125 };
            BytesEnvelope envelope = new EncryptedBytes() { Metadata = null, Data = message };

            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", true));
            DecryptedBytes decryptedMessage = cryptor.Decrypt(envelope);

            byte[] expectedBytes = new byte[] { 121, 97, 121, 33 };
            Assert.AreEqual(expectedBytes, decryptedMessage.Data);
        }

                [Test]
        public void TestYayEncryptionBasic()
        {
            //deserialized string
            string message = "yay!";
            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", false));
            EncryptedData encryptedMessage = cryptor.Encrypt(message);

            Assert.AreEqual("q/xJqqN6qbiZMXYmiQC1Fw==", encryptedMessage.Data);
        }
        
        [Test]
        public void TestYayEncryptionBasicWithDynamicIV()
        {
            IPubnubUnitTest pubnubUnitTest = new PubnubUnitTest();
            pubnubUnitTest.IV = new byte[16] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54 };

            //deserialized string
            string message = "yay!";
            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", true, null, pubnubUnitTest));
            EncryptedData encryptedMessage = cryptor.Encrypt(message);

            Assert.AreEqual("MTIzNDU2Nzg5MDEyMzQ1NjdnONoCgo0wbuMGGMmfMX0=", encryptedMessage.Data);
        }

        [Test]
        public void TestYayByteArrayEncryptionBasic()
        {
            //deserialized string
            string message = "yay!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", false));
            EncryptedBytes encryptedMessage = cryptor.Encrypt(messageBytes);

            byte[] expectedBytes = new byte[] { 171, 252, 73, 170, 163, 122, 169, 184, 153, 49, 118, 38, 137, 0, 181, 23 };
            Assert.AreEqual(expectedBytes, encryptedMessage.Data);
        }

        [Test]
        public void TestYayByteArrayEncryptionBasicWithDynamicIV()
        {
            IPubnubUnitTest pubnubUnitTest = new PubnubUnitTest();
            pubnubUnitTest.IV = new byte[16] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54 };

            //deserialized string
            string message = "yay!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            //Encrypt
            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", true, null, pubnubUnitTest));
            EncryptedBytes encryptedMessage = cryptor.Encrypt(messageBytes);

            byte[] expectedBytes = new byte[] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54, 55, 103, 56, 218, 2, 130, 141, 48, 110, 227, 6, 24, 201, 159, 49, 125 };
            Assert.AreEqual(expectedBytes, encryptedMessage.Data);
        }

    }
}
