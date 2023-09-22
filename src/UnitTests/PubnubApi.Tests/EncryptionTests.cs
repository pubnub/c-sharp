using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using PubnubApi;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using PeterO.Cbor;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using PubnubApi.Security.Crypto.Common;

namespace PubNubMessaging.Tests
{
    /// <summary>
    /// Custom class for testing the encryption and decryption 
    /// </summary>
    public class CustomClass
    {
        public string foo { get; set; } = "hi!";

        public int[] bar { get; set; } = { 1, 2, 3, 4, 5 };
    }

    public class SecretCustomClass
    {
        public string foo { get; set; } = "hello!";

        public int[] bar { get; set; } = { 10, 20, 30, 40, 50 };
    }

    public class PubnubDemoObject
    {
        public double VersionID { get; set; } = 3.4;

        public string Timetoken { get; set; } = "13601488652764619";

        public string OperationName { get; set; } = "Publish";

        public string[] Channels { get; set; } = { "ch1" };

        public PubnubDemoMessage DemoMessage { get; set; } = new PubnubDemoMessage();

        public PubnubDemoMessage CustomMessage { get; set; } = new PubnubDemoMessage("Welcome to the world of Pubnub for Publish and Subscribe. Hah!");

        public Person[] SampleXml { get; set; } = new DemoRoot().Person.ToArray();
    }

    public class PubnubDemoMessage
    {
        public string DefaultMessage { get; set; } = "~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\ {}| asdfghjkl;' :\" zxcvbnm,./ <>? ";

        public PubnubDemoMessage()
        {
        }

        public PubnubDemoMessage(string message)
        {
            DefaultMessage = message;
        }
    }

    public class DemoRoot
    {
        public List<Person> Person
        {
            get
            {
                List<Person> ret = new List<Person>();
                Person p1 = new Person();
                p1.ID = "ABCD123";
                ////PersonID id1 = new PersonID(); id1.ID = "ABCD123" ;
                ////p1.ID = id1;
                Name n1 = new Name();
                n1.First = "John";
                n1.Middle = "P.";
                n1.Last = "Doe";
                p1.Name = n1;

                Address a1 = new Address();
                a1.Street = "123 Duck Street";
                a1.City = "New City";
                a1.State = "New York";
                a1.Country = "United States";
                p1.Address = a1;

                ret.Add(p1);

                Person p2 = new Person();
                p2.ID = "ABCD456";
                ////PersonID id2 = new PersonID(); id2.ID = "ABCD123" ;
                ////p2.ID = id2;
                Name n2 = new Name();
                n2.First = "Peter";
                n2.Middle = "Z.";
                n2.Last = "Smith";
                p2.Name = n2;

                Address a2 = new Address();
                a2.Street = "12 Hollow Street";
                a2.City = "Philadelphia";
                a2.State = "Pennsylvania";
                a2.Country = "United States";
                p2.Address = a2;

                ret.Add(p2);

                return ret;
            }
        }
    }

    public class Person
    {
        public string ID { get; set; }

        public Name Name { get; set; }

        public Address Address { get; set; }
    }

    public class Name
    {
        public string First { get; set; }

        public string Middle { get; set; }

        public string Last { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }
    }

    [TestFixture]
    public class EncryptionTests
    {
        [Test]
        public void ParseGrantTokenTest()
        {
            string expected = "{\"Version\":2,\"Timestamp\":1568739458,\"TTL\":100,\"Resources\":{\"Channels\":{},\"ChannelGroups\":{},\"Uuids\":{},\"Users\":{},\"Spaces\":{}},\"Patterns\":{\"Channels\":{},\"ChannelGroups\":{},\"Uuids\":{},\"Users\":{\"^emp-*\":{\"Read\":true,\"Write\":true,\"Manage\":false,\"Delete\":false,\"Create\":false,\"Get\":false,\"Update\":false,\"Join\":false},\"^mgr-*\":{\"Read\":true,\"Write\":true,\"Manage\":false,\"Delete\":true,\"Create\":true,\"Get\":false,\"Update\":false,\"Join\":false}},\"Spaces\":{\"^public-*\":{\"Read\":true,\"Write\":true,\"Manage\":false,\"Delete\":false,\"Create\":false,\"Get\":false,\"Update\":false,\"Join\":false},\"^private-*\":{\"Read\":true,\"Write\":true,\"Manage\":false,\"Delete\":true,\"Create\":true,\"Get\":false,\"Update\":false,\"Join\":false}}},\"Meta\":{},\"AuthorizedUuid\":null,\"Signature\":\"LL8xpndq3ILa/a3LOK9ragvO2EqaUmKrPQin2jOSEWQ=\"}";
            string actual = "";
            string token = "p0F2AkF0Gl2BEIJDdHRsGGRDcmVzpERjaGFuoENncnCgQ3VzcqBDc3BjoENwYXSkRGNoYW6gQ2dycKBDdXNyomZeZW1wLSoDZl5tZ3ItKhgbQ3NwY6JpXnB1YmxpYy0qA2pecHJpdmF0ZS0qGBtEbWV0YaBDc2lnWCAsvzGmd2rcgtr9rcs4r2tqC87YSppSYqs9CKfaM5IRZA==";
            //string token = "qEF2AkF0GmFLd-NDdHRsGQWgQ3Jlc6VEY2hhbqFjY2gxGP9DZ3JwoWNjZzEY_0N1c3KgQ3NwY6BEdXVpZKFldXVpZDEY_0NwYXSlRGNoYW6gQ2dycKBDdXNyoENzcGOgRHV1aWShYl4kAURtZXRho2VzY29yZRhkZWNvbG9yY3JlZGZhdXRob3JlcGFuZHVEdXVpZGtteWF1dGh1dWlkMUNzaWdYIP2vlxHik0EPZwtgYxAW3-LsBaX_WgWdYvtAXpYbKll3";
            try
            {
                PNConfiguration config = new PNConfiguration(new UserId("unit-test-uuid"))
                {
                    SubscribeKey = PubnubCommon.SubscribeKey,
                    PublishKey = PubnubCommon.PublishKey,
                };
                Pubnub pubnub = new Pubnub(config);
                PNTokenContent pnGrant = pubnub.ParseToken(token);

                if (pnGrant != null)
                {
                    actual = Newtonsoft.Json.JsonConvert.SerializeObject(pnGrant);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception = " + ex.ToString());
            }
            Assert.AreEqual(actual, expected);
        }

        /// <summary>
        /// Tests the null encryption.
        /// The input is serialized
        /// </summary>
        [Test]
        public void TestNullEncryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////serialized string
            string message = null;
            Assert.Throws<ArgumentException>(() => 
            {
                ////encrypt
                cm.Encrypt(message);
            });
        }

        /// <summary>
        /// Tests the null decryption.
        /// Assumes that the input message is  deserialized  
        /// </summary>        
        [Test]
        public void TestNullDecryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////deserialized string
            string message = null;
            Assert.Throws<ArgumentException>(() =>
            {
                ////decrypt
                cm.Decrypt(message);
            });
        }

        /// <summary>
        /// Tests the yay decryption.
        /// Assumes that the input message is deserialized  
        /// Decrypted string should match yay!
        /// </summary>
        [Test]
        public void TestYayDecryptionBasic()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            string message = "q/xJqqN6qbiZMXYmiQC1Fw==";
            ////decrypt
            string decryptedMessage = cm.Decrypt(message);
            ////deserialize again
            Assert.AreEqual("yay!", decryptedMessage);
        }

        [Test]
        public void TestYayDecryptionBasicWithDynamicIV()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", true, null), null);
            string message = "MTIzNDU2Nzg5MDEyMzQ1NjdnONoCgo0wbuMGGMmfMX0=";
            
            string decryptedMessage = cm.Decrypt(message);
            
            Assert.AreEqual("yay!", decryptedMessage);
        }


        [Test]
        public void TestYayByteArrayDecryptionBasic()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            byte[] messageBytes = new byte[] { 171, 252, 73, 170, 163, 122, 169, 184, 153, 49, 118, 38, 137, 0, 181, 23 };
            ////decrypt
            byte[] decryptedBytes = cm.Decrypt(messageBytes);
            byte[] expectedBytes = new byte[] { 121, 97, 121, 33 };
            Assert.AreEqual(expectedBytes, decryptedBytes);
        }

        [Test]
        public void TestYayByteArrayDecryptionBasicWithDynamicIV()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", true, null), null);
            byte[] messageBytes = new byte[] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54, 55, 103, 56, 218, 2, 130, 141, 48, 110, 227, 6, 24, 201, 159, 49, 125 };
            
            byte[] decryptedBytes = cm.Decrypt(messageBytes);
            byte[] expectedBytes = new byte[] { 121, 97, 121, 33 };
            Assert.AreEqual(expectedBytes, decryptedBytes);
        }

        //[Test]
        public void TestMoonImageDecryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", true, null), null);
            string base64str = "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";
            byte[] messageBytes = Convert.FromBase64CharArray(base64str.ToCharArray(), 0, base64str.Length);

            byte[] decryptedBytes = cm.Decrypt(messageBytes);
            System.IO.File.WriteAllBytes(@"C:\Pandu\temp\file_dec_364234.png", decryptedBytes);
            byte[] expectedBytes = new byte[] { 121, 97, 121, 33 };
            Assert.AreEqual(expectedBytes, decryptedBytes);

        }

        //[Test]
        public void TestFileEncryptionFromPath()
        {
            LegacyCryptor legacyCryptor = new LegacyCryptor("enigma", true, null);
            legacyCryptor.SetTestOnlyConstantRandomIV(new byte[16] { 21, 113, 108, 52, 211, 105, 24, 46, 175, 249, 87, 111, 60, 71, 232, 107 });
            CryptoModule cm = new CryptoModule(legacyCryptor, null);
            byte[] fileByteArray = System.IO.File.ReadAllBytes(@"C:\Pandu\temp\new\input\word_test.txt");

            byte[] decryptedBytes = cm.Encrypt(fileByteArray);
            System.IO.File.WriteAllBytes(@"C:\Pandu\temp\new\input\word_test_enc.txt", decryptedBytes);
            byte[] expectedBytes = new byte[] { 21, 113, 108, 52, 211, 105, 24, 46, 175, 249, 87, 111, 60, 71, 232, 107, 141, 182, 118, 209, 117, 159, 64, 210, 220, 133, 28, 22, 247, 245, 30, 5 };
            Assert.AreEqual(expectedBytes, decryptedBytes);

        }

        [Test]
        public void TestLocalFileEncryptionFromPath()
        {
            string sourceFile = "fileupload.txt";
            string destFile = "fileupload_encrypted.txt";
            if (System.IO.File.Exists(destFile))
            {
                System.IO.File.Delete(destFile);
            }
            PNConfiguration config = new PNConfiguration(new UserId("uuid"));
            Pubnub pn = new Pubnub(config);
            pn.EncryptFile(sourceFile, destFile, "enigma");
            Assert.IsTrue(System.IO.File.Exists(destFile));

        }

        [Test]
        public void TestLocalFileDecryptionFromPath()
        {
            string sourceFile = "fileupload_enc.txt";
            string destFile = "fileupload_enc_decrypted_to_original.txt";
            if (System.IO.File.Exists(destFile))
            {
                System.IO.File.Delete(destFile);
            }
            PNConfiguration config = new PNConfiguration(new UserId("unit-test-uuid"));
            Pubnub pn = new Pubnub(config);
            pn.DecryptFile(sourceFile, destFile, "enigma");
            Assert.IsTrue(System.IO.File.Exists(destFile));

        }

        /// <summary>
        /// Tests the yay encryption.
        /// The output is not serialized
        /// Encrypted string should match q/xJqqN6qbiZMXYmiQC1Fw==
        /// </summary>
        [Test]
        public void TestYayEncryptionBasic()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////deserialized string
            string message = "yay!";
            ////Encrypt
            string encryptedMessage = cm.Encrypt(message);
            Assert.AreEqual("q/xJqqN6qbiZMXYmiQC1Fw==", encryptedMessage);
        }
        
        [Test]
        public void TestYayEncryptionBasicWithDynamicIV()
        {
            LegacyCryptor legacyCryptor = new LegacyCryptor("enigma", true, null);
            legacyCryptor.SetTestOnlyConstantRandomIV(new byte[16] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54 });
            CryptoModule cm = new CryptoModule(legacyCryptor, null);
            //deserialized string
            string message = "yay!";
            //Encrypt
            string encryptedMessage = cm.Encrypt(message);
            Assert.AreEqual("MTIzNDU2Nzg5MDEyMzQ1NjdnONoCgo0wbuMGGMmfMX0=", encryptedMessage);
        }


        /// <summary>
        /// Tests the yay encryption.
        /// The output is not serialized
        /// Encrypted string should match q/xJqqN6qbiZMXYmiQC1Fw==
        /// </summary>
        [Test]
        public void TestYayByteArrayEncryptionBasic()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////deserialized string
            string message = "yay!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            ////Encrypt
            byte[] encryptedBytes = cm.Encrypt(messageBytes);
            byte[] expectedBytes = new byte[] { 171, 252, 73, 170, 163, 122, 169, 184, 153, 49, 118, 38, 137, 0, 181, 23 };
            Assert.AreEqual(expectedBytes, encryptedBytes);
        }

        [Test]
        public void TestYayByteArrayEncryptionBasicWithDynamicIV()
        {
            LegacyCryptor legacyCryptor = new LegacyCryptor("enigma", true, null);
            legacyCryptor.SetTestOnlyConstantRandomIV(new byte[16] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54 });
            CryptoModule cm = new CryptoModule(legacyCryptor, null);
            //deserialized string
            string message = "yay!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            //Encrypt
            byte[] encryptedBytes = cm.Encrypt(messageBytes);


            byte[] expectedBytes = new byte[] { 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 49, 50, 51, 52, 53, 54, 55, 103, 56, 218, 2, 130, 141, 48, 110, 227, 6, 24, 201, 159, 49, 125 };
            Assert.AreEqual(expectedBytes, encryptedBytes);
        }

        /// <summary>
        /// Tests the yay decryption.
        /// Assumes that the input message is not deserialized  
        /// Decrypted and Deserialized string should match yay!
        /// </summary>
        [Test]
        public void TestYayDecryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////string strMessage= "\"q/xJqqN6qbiZMXYmiQC1Fw==\"";
            ////Non deserialized string
            string message = "\"Wi24KS4pcTzvyuGOHubiXg==\"";
            ////Deserialize
            message = JsonConvert.DeserializeObject<string>(message);
            ////decrypt
            string decryptedMessage = cm.Decrypt(message);
            ////deserialize again
            message = JsonConvert.DeserializeObject<string>(decryptedMessage);
            Assert.AreEqual("yay!", message);
        }

        /// <summary>
        /// Tests the yay encryption.
        /// The output is not serialized
        /// Encrypted string should match Wi24KS4pcTzvyuGOHubiXg==
        /// </summary>
        [Test]
        public void TestYayEncryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////deserialized string
            string message = "yay!";
            ////serialize the string
            message = JsonConvert.SerializeObject(message);
            Debug.WriteLine(message);
            ////Encrypt
            string enc = cm.Encrypt(message);
            Assert.AreEqual("Wi24KS4pcTzvyuGOHubiXg==", enc);
        }

        /// <summary>
        /// Tests the array encryption.
        /// The output is not serialized
        /// Encrypted string should match the serialized object
        /// </summary>
        [Test]
        public void TestArrayEncryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////create an empty array object
            object[] emptyArray = { };
            ////serialize
            string serializedArray = JsonConvert.SerializeObject(emptyArray);
            ////Encrypt
            string encryptedMessage = cm.Encrypt(serializedArray);

            Assert.AreEqual("Ns4TB41JjT2NCXaGLWSPAQ==", encryptedMessage);
        }

        /// <summary>
        /// Tests the array decryption.
        /// Assumes that the input message is deserialized
        /// And the output message has to been deserialized.
        /// Decrypted string should match the serialized object
        /// </summary>
        [Test]
        public void TestArrayDecryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////Input the deserialized string
            string message = "Ns4TB41JjT2NCXaGLWSPAQ==";
            ////decrypt
            string decryptedMessage = cm.Decrypt(message);
            ////create a serialized object
            object[] emptyArrayObject = { };
            string result = JsonConvert.SerializeObject(emptyArrayObject);
            ////compare the serialized object and the return of the Decrypt method
            Assert.AreEqual(result, decryptedMessage);
        }

        /// <summary>
        /// Tests the object encryption.
        /// The output is not serialized
        /// Encrypted string should match the serialized object
        /// </summary>
        [Test]
        public void TestObjectEncryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////create an object
            Object obj = new Object();
            ////serialize
            string serializedObject = JsonConvert.SerializeObject(obj);
            ////encrypt
            string encryptedMessage = cm.Encrypt(serializedObject);

            Assert.AreEqual("IDjZE9BHSjcX67RddfCYYg==", encryptedMessage);
        }

        /// <summary>
        /// Tests the object decryption.
        /// Assumes that the input message is deserialized
        /// And the output message has to be deserialized.
        /// Decrypted string should match the serialized object
        /// </summary>
        [Test]
        public void TestObjectDecryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////Deserialized
            string message = "IDjZE9BHSjcX67RddfCYYg==";
            ////Decrypt
            string decryptedMessage = cm.Decrypt(message);
            ////create an object
            Object obj = new Object();
            ////Serialize the object
            string result = JsonConvert.SerializeObject(obj);

            Assert.AreEqual(result, decryptedMessage);
        }

        /// <summary>
        /// Tests my object encryption.
        /// The output is not serialized 
        /// Encrypted string should match the serialized object
        /// </summary>
        [Test]
        public void TestMyObjectEncryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////create an object of the custom class
            CustomClass cc = new CustomClass();
            ////serialize it
            string result = JsonConvert.SerializeObject(cc);
            ////encrypt it
            string encryptedMessage = cm.Encrypt(result);

            Assert.AreEqual("Zbr7pEF/GFGKj1rOstp0tWzA4nwJXEfj+ezLtAr8qqE=", encryptedMessage);
        }

        /// <summary>
        /// Tests my object decryption.
        /// The output is not deserialized
        /// Decrypted string should match the serialized object
        /// </summary>
        [Test]
        public void TestMyObjectDecryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////Deserialized
            string message = "Zbr7pEF/GFGKj1rOstp0tWzA4nwJXEfj+ezLtAr8qqE=";
            ////Decrypt
            string decryptedMessage = cm.Decrypt(message);
            ////create an object of the custom class
            CustomClass cc = new CustomClass();
            ////Serialize it
            string result = JsonConvert.SerializeObject(cc);

            Assert.AreEqual(result, decryptedMessage);
        }

        /// <summary>
        /// Tests the pub nub encryption2.
        /// The output is not serialized
        /// Encrypted string should match f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54=
        /// </summary>
        [Test]
        public void TestPubNubEncryption2()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////Deserialized
            string message = "Pubnub Messaging API 2";
            ////serialize the message
            message = JsonConvert.SerializeObject(message);
            ////encrypt
            string encryptedMessage = cm.Encrypt(message);

            Assert.AreEqual("f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54=", encryptedMessage);
        }

        /// <summary>
        /// Tests the pub nub decryption2.
        /// Assumes that the input message is deserialized  
        /// Decrypted and Deserialized string should match Pubnub Messaging API 2
        /// </summary>
        [Test]
        public void TestPubNubDecryption2()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////Deserialized string    
            string message = "f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54=";
            ////Decrypt
            string decryptedMessage = cm.Decrypt(message);
            ////Deserialize
            message = JsonConvert.DeserializeObject<string>(decryptedMessage);
            Assert.AreEqual("Pubnub Messaging API 2", message);
        }

        /// <summary>
        /// Tests the pub nub encryption1.
        /// The input is not serialized
        /// Encrypted string should match f42pIQcWZ9zbTbH8cyLwByD/GsviOE0vcREIEVPARR0=
        /// </summary>
        [Test]
        public void TestPubNubEncryption1()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////non serialized string
            string message = "Pubnub Messaging API 1";
            ////serialize
            message = JsonConvert.SerializeObject(message);
            ////encrypt
            string encryptedMessage = cm.Encrypt(message);

            Assert.AreEqual("f42pIQcWZ9zbTbH8cyLwByD/GsviOE0vcREIEVPARR0=", encryptedMessage);
        }

        /// <summary>
        /// Tests the pub nub decryption1.
        /// Assumes that the input message is  deserialized  
        /// Decrypted and Deserialized string should match Pubnub Messaging API 1        
        /// </summary>
        [Test]
        public void TestPubNubDecryption1()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////deserialized string
            string message = "f42pIQcWZ9zbTbH8cyLwByD/GsviOE0vcREIEVPARR0=";
            ////decrypt
            string decryptedMessage = cm.Decrypt(message);
            ////deserialize
            message = (decryptedMessage != "**DECRYPT ERROR**") ? JsonConvert.DeserializeObject<string>(decryptedMessage) : "";
            Assert.AreEqual("Pubnub Messaging API 1", message);
        }

        /// <summary>
        /// Tests the stuff can encryption.
        /// The input is serialized
        /// Encrypted string should match zMqH/RTPlC8yrAZ2UhpEgLKUVzkMI2cikiaVg30AyUu7B6J0FLqCazRzDOmrsFsF
        /// </summary>
        [Test]
        public void TestStuffCanEncryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////input serialized string
            string message = "{\"this stuff\":{\"can get\":\"complicated!\"}}";
            ////encrypt
            string encryptedMessage = cm.Encrypt(message);

            Assert.AreEqual("zMqH/RTPlC8yrAZ2UhpEgLKUVzkMI2cikiaVg30AyUu7B6J0FLqCazRzDOmrsFsF", encryptedMessage);
        }

        /// <summary>
        /// Tests the stuffcan decryption.
        /// Assumes that the input message is  deserialized  
        /// </summary>
        [Test]
        public void TestStuffcanDecryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////deserialized string
            string message = "zMqH/RTPlC8yrAZ2UhpEgLKUVzkMI2cikiaVg30AyUu7B6J0FLqCazRzDOmrsFsF";
            ////decrypt
            string decryptedMessage = cm.Decrypt(message);

            Assert.AreEqual("{\"this stuff\":{\"can get\":\"complicated!\"}}", decryptedMessage);
        }

        /// <summary>
        /// Tests the hash encryption.
        /// The input is serialized
        /// Encrypted string should match GsvkCYZoYylL5a7/DKhysDjNbwn+BtBtHj2CvzC4Y4g=
        /// </summary>
        [Test]
        public void TestHashEncryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////serialized string
            string message = "{\"foo\":{\"bar\":\"foobar\"}}";
            ////encrypt
            string encryptedMessage = cm.Encrypt(message);

            Assert.AreEqual("GsvkCYZoYylL5a7/DKhysDjNbwn+BtBtHj2CvzC4Y4g=", encryptedMessage);
        }

        /// <summary>
        /// Tests the hash decryption.
        /// Assumes that the input message is  deserialized  
        /// </summary>        
        [Test]
        public void TestHashDecryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            ////deserialized string
            string message = "GsvkCYZoYylL5a7/DKhysDjNbwn+BtBtHj2CvzC4Y4g=";
            ////decrypt
            string decryptedMessage = cm.Decrypt(message);

            Assert.AreEqual("{\"foo\":{\"bar\":\"foobar\"}}", decryptedMessage);
        }

        /// <summary>
        /// Tests the unicode chars encryption.
        /// The input is not serialized
        /// </summary>
        [Test]
        public void TestUnicodeCharsEncryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            string message = "漢語";

            message = JsonConvert.SerializeObject(message);
            Debug.WriteLine(message);
            string encryptedMessage = cm.Encrypt(message);
            Debug.WriteLine(encryptedMessage);
            Assert.AreEqual("+BY5/miAA8aeuhVl4d13Kg==", encryptedMessage);
        }

        /// <summary>
        /// Tests the unicode decryption.
        /// Assumes that the input message is  deserialized  
        /// Decrypted and Deserialized string should match the unicode chars       
        /// </summary>
        [Test]
        public void TestUnicodeCharsDecryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            string message = "+BY5/miAA8aeuhVl4d13Kg==";
            ////decrypt
            string decryptedMessage = cm.Decrypt(message);
            ////deserialize
            message = (decryptedMessage != "**DECRYPT ERROR**") ? JsonConvert.DeserializeObject<string>(decryptedMessage) : "";

            Assert.AreEqual("漢語", message);
        }

        /// <summary>
        /// Tests the german chars decryption.
        /// Assumes that the input message is  deserialized  
        /// Decrypted and Deserialized string should match the unicode chars  
        /// </summary>
        [Test]
        public void TestGermanCharsDecryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            string message = "stpgsG1DZZxb44J7mFNSzg==";

            ////decrypt
            string decryptedMessage = cm.Decrypt(message);
            ////deserialize
            message = (decryptedMessage != "**DECRYPT ERROR**") ? JsonConvert.DeserializeObject<string>(decryptedMessage) : "";

            Assert.AreEqual("ÜÖ", message);
        }

        /// <summary>
        /// Tests the german encryption.
        /// The input is not serialized
        /// </summary>
        [Test]
        public void TestGermanCharsEncryption()
        {
            CryptoModule cm = new CryptoModule(new LegacyCryptor("enigma", false), null);
            string message = "ÜÖ";

            message = JsonConvert.SerializeObject(message);
            Debug.WriteLine(message);
            string encryptedMessage = cm.Encrypt(message);
            Debug.WriteLine(encryptedMessage);
            Assert.AreEqual("stpgsG1DZZxb44J7mFNSzg==", encryptedMessage);
        }

        [Test]
        public void TestCryptoModuleWithMultipleCryptorsDefaultAesCbc()
        {
            string message = "yay!";
            AesCbcCryptor aesCbcCryptor = CryptoModule.CreateAesCbcCryptor("enigma");
            LegacyCryptor legacyCryptor = CryptoModule.CreateLegacyCryptor("enigma");
            CryptoModule cm = new CryptoModule(aesCbcCryptor, new List<ICryptor> { legacyCryptor });
            string encryptedMessage = cm.Encrypt(message);
            string decryptedMessage = cm.Decrypt(encryptedMessage);
            Assert.AreEqual(message, decryptedMessage);
        }

        [Test]
        public void TestCryptoModuleWithMultipleCryptorsDefaultLegacy()
        {
            string message = "yay!";
            AesCbcCryptor aesCbcCryptor = CryptoModule.CreateAesCbcCryptor("enigma");
            LegacyCryptor legacyCryptor = CryptoModule.CreateLegacyCryptor("enigma");
            CryptoModule cm = new CryptoModule(legacyCryptor, new List<ICryptor> { aesCbcCryptor });
            string encryptedMessage = cm.Encrypt(message);
            string decryptedMessage = cm.Decrypt(encryptedMessage);
            Assert.AreEqual(message, decryptedMessage);
        }

        [Test]
        public void TestPAMSignature()
        {
            string secretKey = "secret";
            string message = "Pubnub Messaging 1";

            string signature = Util.PubnubAccessManagerSign(secretKey, message);
            System.Diagnostics.Debug.WriteLine("TestPAMSignature = " + signature);

            Assert.AreEqual("mIoxTVM2WAM5j-M2vlp9bVblDLoZQI5XIoYyQ48U0as=", signature);
        }

        [Test]
        public void TestPAMv3Signature()
        {
            string secretKey = "wMfbo9G0xVUG8yfTfYw5qIdfJkTd7A";
            string message = "POST\ndemo\n/v3/pam/demo/grant\nPoundsSterling=%C2%A313.37&timestamp=123456789\n{\n  \"ttl\": 1440,\n  \"permissions\": {\n    \"resources\" : {\n      \"channels\": {\n        \"inbox-jay\": 3\n      },\n      \"groups\": {},\n      \"users\": {},\n      \"spaces\": {}\n    },\n    \"patterns\" : {\n      \"channels\": {},\n      \"groups\": {},\n      \"users\": {},\n      \"spaces\": {}\n    },\n    \"meta\": {\n      \"user-id\": \"jay@example.com\",\n      \"contains-unicode\": \"The 💩 test.\"\n    }\n  }\n}";

            string signature = Util.PubnubAccessManagerSign(secretKey, message);
            signature = string.Format("v2.{0}", signature.TrimEnd(new char[] { '=' }));
            System.Diagnostics.Debug.WriteLine("TestPAMv3Signature = " + signature);

            Assert.AreEqual("v2.k80LsDMD-sImA8rCBj-ntRKhZ8mSjHY8Ivngt9W3Yc4", signature);
        }


    }
}