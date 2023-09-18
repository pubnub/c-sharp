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

            Cryptor cryptor = new Cryptor(new LegacyCryptor("enigma", false));
            string decryptedMessage = cryptor.Decrypt(message);
            
            Assert.AreEqual("yay!", decryptedMessage);
        }

        [Test]
        public void TestYayLegacyCryptoDecryptionBasicWithDynamicIV()
        {
            string message = "MTIzNDU2Nzg5MDEyMzQ1NjdnONoCgo0wbuMGGMmfMX0=";
            
            Cryptor cryptor = new Cryptor(new LegacyCryptor("enigma", true));
            string decryptedMessage = cryptor.Decrypt(message);
            
            Assert.AreEqual("yay!", decryptedMessage);
        }

        [Test]
        public void TestYayByteArrayLegacyCryptoDecryptionBasic()
        {
            byte[] message = new byte[] { 171, 252, 73, 170, 163, 122, 169, 184, 153, 49, 118, 38, 137, 0, 181, 23 };

            Cryptor cryptor = new Cryptor(new LegacyCryptor("enigma", false));
            byte[] decryptedMessage = cryptor.Decrypt(message);

            byte[] expectedBytes = new byte[] { 121, 97, 121, 33 };
            Assert.AreEqual(expectedBytes, decryptedMessage);
        }

        [Test]
        public void TestYayByteArrayLegacyCryptoDecryptionBasicWithDynamicIV()
        {
            byte[] message = new byte[] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54, 55, 103, 56, 218, 2, 130, 141, 48, 110, 227, 6, 24, 201, 159, 49, 125 };

            Cryptor cryptor = new Cryptor(new LegacyCryptor("enigma", true));
            byte[] decryptedMessage = cryptor.Decrypt(message);

            byte[] expectedBytes = new byte[] { 121, 97, 121, 33 };
            Assert.AreEqual(expectedBytes, decryptedMessage);
        }

        [Test]
        public void TestYayLegacyCryptoEncryptionBasic()
        {
            //deserialized string
            string message = "yay!";
            Cryptor cryptor = new Cryptor(new LegacyCryptor("enigma", false));
            string encryptedMessage = cryptor.Encrypt(message);

            Assert.AreEqual("q/xJqqN6qbiZMXYmiQC1Fw==", encryptedMessage);
        }
        
        [Test]
        public void TestYayLegacyCryptoEncryptionBasicWithDynamicIV()
        {
            IPubnubUnitTest pubnubUnitTest = new PubnubUnitTest();
            pubnubUnitTest.IV = new byte[16] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54 };

            //deserialized string
            string message = "yay!";
            Cryptor cryptor = new Cryptor(new LegacyCryptor("enigma", true, null, pubnubUnitTest));
            string encryptedMessage = cryptor.Encrypt(message);

            Assert.AreEqual("MTIzNDU2Nzg5MDEyMzQ1NjdnONoCgo0wbuMGGMmfMX0=", encryptedMessage);
        }

        [Test]
        public void TestYayByteArrayLegacyCryptoEncryptionBasic()
        {
            //deserialized string
            string message = "yay!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            Cryptor cryptor = new Cryptor(new LegacyCryptor("enigma", false));
            byte[] encryptedMessage = cryptor.Encrypt(messageBytes);

            byte[] expectedBytes = new byte[] { 171, 252, 73, 170, 163, 122, 169, 184, 153, 49, 118, 38, 137, 0, 181, 23 };
            Assert.AreEqual(expectedBytes, encryptedMessage);
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
            Cryptor cryptor = new Cryptor(new LegacyCryptor("enigma", true, null, pubnubUnitTest));
            byte[] encryptedMessage = cryptor.Encrypt(messageBytes);

            byte[] expectedBytes = new byte[] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54, 55, 103, 56, 218, 2, 130, 141, 48, 110, 227, 6, 24, 201, 159, 49, 125 };
            Assert.AreEqual(expectedBytes, encryptedMessage);
        }
        
        [Test]
        public void TestYayAesCbcCryptoEncryption()
        {
            IPubnubUnitTest pubnubUnitTest = new PubnubUnitTest();
            pubnubUnitTest.IV = new byte[16] { 76, 39, 34, 143, 55, 38, 217, 57, 27, 213, 228, 184, 246, 4, 159, 232 };
            //deserialized string
            string message = "yay!";
            Cryptor cryptor = new Cryptor(new AesCbcCryptor("enigma", new TestLog(), pubnubUnitTest));
            string encryptedMessage = cryptor.Encrypt(message);

            Assert.AreEqual("UE5FRAFBQ1JIEEwnIo83Jtk5G9XkuPYEn+gCu8PDp3G/SOEVWOfY3Ofj", encryptedMessage);
        }
        

        [Test]
        public void TestYayByteArrayAesCbcCryptoEncryption()
        {
            IPubnubUnitTest pubnubUnitTest = new PubnubUnitTest();
            pubnubUnitTest.IV = new byte[16] { 76, 39, 34, 143, 55, 38, 217, 57, 27, 213, 228, 184, 246, 4, 159, 232 };
            //deserialized string
            string message = "yay!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            Cryptor cryptor = new Cryptor(new AesCbcCryptor("enigma", new TestLog(), pubnubUnitTest));
            byte[] encryptedMessage = cryptor.Encrypt(messageBytes);

            byte[] expectedBytes = new byte[] { 80, 78, 69, 68, 1, 65, 67, 82, 72, 16, 76, 39, 34, 143, 55, 38, 217, 57, 27, 213, 228, 184, 246, 4, 159, 232, 2, 187, 195, 195, 167, 113, 191, 72, 225, 21, 88, 231, 216, 220, 231, 227 };
            Assert.AreEqual(expectedBytes, encryptedMessage);
        }
        
        [Test]
        public void TestYayAesCbcCryptoDecryption()
        {
            //deserialized string
            string encryptMessage = "UE5FRAFBQ1JIEEwnIo83Jtk5G9XkuPYEn+gCu8PDp3G/SOEVWOfY3Ofj";

            Cryptor cryptor = new Cryptor(new AesCbcCryptor("enigma"));
            string decryptedData = cryptor.Decrypt(encryptMessage);

            Assert.AreEqual("yay!", decryptedData);
        }
        

        [Test]
        public void TestYayByteArrayAesCbcCryptoDecryption()
        {
            //deserialized string
            string message = "yay!";
            byte[] encryptedBytes = new byte[] { 80, 78, 69, 68, 1, 65, 67, 82, 72, 16, 76, 39, 34, 143, 55, 38, 217, 57, 27, 213, 228, 184, 246, 4, 159, 232, 2, 187, 195, 195, 167, 113, 191, 72, 225, 21, 88, 231, 216, 220, 231, 227 };
            Cryptor cryptor = new Cryptor(new AesCbcCryptor("enigma"));
            byte[] decryptedBytes = cryptor.Decrypt(encryptedBytes);

            byte[] expectedBytes = Encoding.UTF8.GetBytes(message);
            Assert.AreEqual(expectedBytes, decryptedBytes);
        }

    }
}
