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
        public class TestLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine(logText);
            }
        }

        [Test]
        public void TestYayLegacyCryptoDecryptionBasic()
        {
            string message = "q/xJqqN6qbiZMXYmiQC1Fw==";
            DataEnvelope envelope = new EncryptedData() { Metadata = null, Data = message };

            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", false));
            DecryptedData decryptedMessage = cryptor.Decrypt(envelope);
            
            Assert.AreEqual("yay!", decryptedMessage.Data);
        }

        [Test]
        public void TestYayLegacyCryptoDecryptionBasicWithDynamicIV()
        {
            string message = "MTIzNDU2Nzg5MDEyMzQ1NjdnONoCgo0wbuMGGMmfMX0=";
            DataEnvelope envelope = new EncryptedData() { Metadata = null, Data = message };
            
            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", true));
            DecryptedData decryptedMessage = cryptor.Decrypt(envelope);
            
            Assert.AreEqual("yay!", decryptedMessage.Data);
        }

        [Test]
        public void TestYayByteArrayLegacyCryptoDecryptionBasic()
        {
            byte[] message = new byte[] { 171, 252, 73, 170, 163, 122, 169, 184, 153, 49, 118, 38, 137, 0, 181, 23 };
            BytesEnvelope envelope = new EncryptedBytes() { Metadata = null, Data = message };

            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", false));
            DecryptedBytes decryptedMessage = cryptor.Decrypt(envelope);

            byte[] expectedBytes = new byte[] { 121, 97, 121, 33 };
            Assert.AreEqual(expectedBytes, decryptedMessage.Data);
        }

        [Test]
        public void TestYayByteArrayLegacyCryptoDecryptionBasicWithDynamicIV()
        {
            byte[] message = new byte[] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54, 55, 103, 56, 218, 2, 130, 141, 48, 110, 227, 6, 24, 201, 159, 49, 125 };
            BytesEnvelope envelope = new EncryptedBytes() { Metadata = null, Data = message };

            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", true));
            DecryptedBytes decryptedMessage = cryptor.Decrypt(envelope);

            byte[] expectedBytes = new byte[] { 121, 97, 121, 33 };
            Assert.AreEqual(expectedBytes, decryptedMessage.Data);
        }

        [Test]
        public void TestYayLegacyCryptoEncryptionBasic()
        {
            //deserialized string
            string message = "yay!";
            Cryptor cryptor = new Cryptor(new LegacyCryptoAlgorithm("enigma", false));
            EncryptedData encryptedMessage = cryptor.Encrypt(message);

            Assert.AreEqual("q/xJqqN6qbiZMXYmiQC1Fw==", encryptedMessage.Data);
        }
        
        [Test]
        public void TestYayLegacyCryptoEncryptionBasicWithDynamicIV()
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
        public void TestYayByteArrayLegacyCryptoEncryptionBasic()
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
        public void TestYayByteArrayLegacyCryptoEncryptionBasicWithDynamicIV()
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
        
        [Test]
        public void TestYayAesCbcCryptoEncryption()
        {
            IPubnubUnitTest pubnubUnitTest = new PubnubUnitTest();
            pubnubUnitTest.IV = new byte[16] { 76, 39, 34, 143, 55, 38, 217, 57, 27, 213, 228, 184, 246, 4, 159, 232 };
            //deserialized string
            string message = "yay!";
            Cryptor cryptor = new Cryptor(new AesCbcCryptoAlgorithm("enigma", new TestLog(), pubnubUnitTest));
            EncryptedData encryptedMessage = cryptor.Encrypt(message);

            Assert.AreEqual("UE5FRAFDUklWEEwnIo83Jtk5G9XkuPYEn+gCu8PDp3G/SOEVWOfY3Ofj", encryptedMessage.Data);
        }
        

        [Test]
        public void TestYayByteArrayAesCbcCryptoEncryption()
        {
            IPubnubUnitTest pubnubUnitTest = new PubnubUnitTest();
            pubnubUnitTest.IV = new byte[16] { 76, 39, 34, 143, 55, 38, 217, 57, 27, 213, 228, 184, 246, 4, 159, 232 };
            //deserialized string
            string message = "yay!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            Cryptor cryptor = new Cryptor(new AesCbcCryptoAlgorithm("enigma", new TestLog(), pubnubUnitTest));
            EncryptedBytes encryptedMessage = cryptor.Encrypt(messageBytes);

            byte[] expectedBytes = new byte[] { 80, 78, 69, 68, 1, 67, 82, 73, 86, 16, 76, 39, 34, 143, 55, 38, 217, 57, 27, 213, 228, 184, 246, 4, 159, 232, 2, 187, 195, 195, 167, 113, 191, 72, 225, 21, 88, 231, 216, 220, 231, 227 };
            Assert.AreEqual(expectedBytes, encryptedMessage.Data);
        }
        
        [Test]
        public void TestYayAesCbcCryptoDecryption()
        {
            //deserialized string
            string encryptMessage = "UE5FRAFDUklWEEwnIo83Jtk5G9XkuPYEn+gCu8PDp3G/SOEVWOfY3Ofj";
            DataEnvelope envelope = new EncryptedData { Data = encryptMessage };
            Cryptor cryptor = new Cryptor(new AesCbcCryptoAlgorithm("enigma"));
            DecryptedData decryptedData = cryptor.Decrypt(envelope);

            Assert.AreEqual("yay!", decryptedData.Data);
        }
        

        [Test]
        public void TestYayByteArrayAesCbcCryptoDecryption()
        {
            //deserialized string
            string message = "yay!";
            byte[] encryptedBytes = new byte[] { 80, 78, 69, 68, 1, 67, 82, 73, 86, 16, 76, 39, 34, 143, 55, 38, 217, 57, 27, 213, 228, 184, 246, 4, 159, 232, 2, 187, 195, 195, 167, 113, 191, 72, 225, 21, 88, 231, 216, 220, 231, 227 };
            BytesEnvelope envelope = new EncryptedBytes { Data = encryptedBytes };
            Cryptor cryptor = new Cryptor(new AesCbcCryptoAlgorithm("enigma"));
            DecryptedBytes decryptedBytes = cryptor.Decrypt(envelope);

            byte[] expectedBytes = Encoding.UTF8.GetBytes(message);
            Assert.AreEqual(expectedBytes, decryptedBytes.Data);
        }

    }
}
