using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using PubnubApi;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;
using System.Diagnostics;
using PubnubApi.CBOR;

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
        private string HexToUnicodeString(string hexData)
        {
            string hexString = hexData.ToString().Replace("h'", "").Replace("'", "");
            var sb = new StringBuilder();
            for (var i = 0; i < hexString.Length; i += 2)
            {
                var hexChar = hexString.Substring(i, 2);
                sb.Append((char)Convert.ToByte(hexChar, 16));
            }
            string unicodeChar = sb.ToString();
            return unicodeChar;
        }

        private Dictionary<string, string> IterateCborObject(Dahomey.Cbor.ObjectModel.CborObject cborObj)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (Dahomey.Cbor.ObjectModel.CborValue key in cborObj.Keys)
            {
                System.Diagnostics.Debug.WriteLine(cborObj[key].Type.ToString() + " = " + cborObj[key].ToString());
                if (cborObj[key].Type.ToString() == "Object")
                {
                    if (key.ToString().Contains("h'"))
                    {
                        string unicodeChar = HexToUnicodeString(key.ToString());
                        if (!result.ContainsKey(key.ToString()))
                        {
                            result.Add(key.ToString(), string.Format("\"{0}\"",unicodeChar));
                        }
                    }

                    Dahomey.Cbor.ObjectModel.CborObject currentObj = cborObj[key] as Dahomey.Cbor.ObjectModel.CborObject;
                    if (currentObj != null && currentObj.Count > 0)
                    {
                        Dictionary<string, string> currentDic = IterateCborObject(currentObj);
                        foreach (KeyValuePair<string, string> kvp in currentDic)
                        {
                            if (!result.ContainsKey(kvp.Key))
                            {
                                result.Add(kvp.Key, kvp.Value);
                            }
                        }
                    }
                }
                else
                {
                    if (key.ToString().Contains("h'"))
                    {
                        string unicodeChar = HexToUnicodeString(key.ToString());
                        if (!result.ContainsKey(key.ToString()))
                        {
                            result.Add(key.ToString(), string.Format("\"{0}\"",unicodeChar));
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(key.ToString());
                    }

                    if (cborObj[key].Type.ToString() == "ByteString")
                    {
                        string cborByteString = cborObj[key].ToString();
                        string testing = HexToUnicodeString(cborByteString);
                    }
                }
                //dahomeyByteStringKeyData = dahomeyByteStringKeyData.Replace(val.ToString(), unicodeChar1);
            }
            return result;
        }

        [Test]
        public void ParseGrantTokenTest()
        {
            string expected = "{\"Version\":2,\"Timestamp\":1568739458,\"TTL\":100,\"Channels\":{},\"ChannelGroups\":{},\"Users\":{},\"Spaces\":{},\"ChannelPatterns\":{},\"GroupPatterns\":{},\"UserPatterns\":{\"^emp-*\":{\"Read\":true,\"Write\":true,\"Manage\":false,\"Delete\":false,\"Create\":false},\"^mgr-*\":{\"Read\":true,\"Write\":true,\"Manage\":false,\"Delete\":true,\"Create\":true}},\"SpacePatterns\":{\"^public-*\":{\"Read\":true,\"Write\":true,\"Manage\":false,\"Delete\":false,\"Create\":false},\"^private-*\":{\"Read\":true,\"Write\":true,\"Manage\":false,\"Delete\":true,\"Create\":true}},\"Meta\":{},\"Signature\":\"LL8xpndq3ILa/a3LOK9ragvO2EqaUmKrPQin2jOSEWQ=\"}";
            string actual = "";
            string token = "p0F2AkF0Gl2BEIJDdHRsGGRDcmVzpERjaGFuoENncnCgQ3VzcqBDc3BjoENwYXSkRGNoYW6gQ2dycKBDdXNyomZeZW1wLSoDZl5tZ3ItKhgbQ3NwY6JpXnB1YmxpYy0qA2pecHJpdmF0ZS0qGBtEbWV0YaBDc2lnWCAsvzGmd2rcgtr9rcs4r2tqC87YSppSYqs9CKfaM5IRZA==";
            try
            {
                PNConfiguration config = new PNConfiguration
                {
                    SubscribeKey = PubnubCommon.SubscribeKey,
                    PublishKey = PubnubCommon.PublishKey,
                };
                Pubnub pubnub = new Pubnub(config);
                PNGrantToken pnGrant = pubnub.ParseToken(token);
                if (pnGrant != null)
                {
                    actual = Newtonsoft.Json.JsonConvert.SerializeObject(pnGrant);
                }
                pubnub.ClearTokens();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception = " + ex.ToString());
            }
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void NewCborParseTest()
        {
            string expected = "{\"v\":2,\"t\":1568264210,\"ttl\":100,\"res\":{\"chan\":{},\"grp\":{},\"usr\":{\"myuser1\":19},\"spc\":{\"myspace1\":11}},\"pat\":{\"chan\":{},\"grp\":{},\"usr\":{},\"spc\":{}},\"meta\":{},\"sig\":\"HtcG6s5fuao9T2bZCgWRQ3cmR27lnYT03yVs6c6H23o=\"}";
            string actual = "";
            //string token = "p0F2AkF0Gl150BJDdHRsGGRDcmVzpERjaGFuoENncnCgQ3VzcqFnbXl1c2VyMRNDc3BjoWhteXNwYWNlMQtDcGF0pERjaGFuoENncnCgQ3VzcqBDc3BjoERtZXRhoENzaWdYIB7XBurOX7mqPU9m2QoFkUN3Jkdu5Z2E9N8lbOnOh9t6";
            string token = "p0F2AkF0Gl2BEIJDdHRsGGRDcmVzpERjaGFuoENncnCgQ3VzcqBDc3BjoENwYXSkRGNoYW6gQ2dycKBDdXNyomZeZW1wLSoDZl5tZ3ItKhgbQ3NwY6JpXnB1YmxpYy0qA2pecHJpdmF0ZS0qGBtEbWV0YaBDc2lnWCAsvzGmd2rcgtr9rcs4r2tqC87YSppSYqs9CKfaM5IRZA==";
            try
            {
                token = token.Replace('_', '/').Replace('-', '+');//.TrimEnd(new char[] { '=' });
                byte[] tokenByteArray = Convert.FromBase64String(token);
                System.IO.MemoryStream ms = new System.IO.MemoryStream(tokenByteArray);

                object cborItemListObj = ms.DecodeAllCBORItems();
                if (cborItemListObj != null)
                {
                    System.Diagnostics.Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(cborItemListObj));

                    List<object> cborItemList = cborItemListObj as List<object>;
                    if (cborItemList != null && cborItemList.Count > 0)
                    {
                        object tokenItem = cborItemList[0];
                        actual = Newtonsoft.Json.JsonConvert.SerializeObject(tokenItem);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception = " + ex.ToString());
            }
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void DahomeyCborParseTest()
        {
            string expected = "{\"v\":2,\"t\":1568264210,\"ttl\":100,\"res\":{\"chan\":{},\"grp\":{},\"usr\":{\"myuser1\":19},\"spc\":{\"myspace1\":11}},\"pat\":{\"chan\":{},\"grp\":{},\"usr\":{},\"spc\":{}},\"meta\":{},\"sig\":[30,-41,6,-22,-50,95,-71,-86,61,79,102,-39,10,5,-111,67,119,38,71,110,-27,-99,-124,-12,-33,37,108,-23,-50,-121,-37,122]}";
            string actual = "";
            string token = "p0F2AkF0Gl150BJDdHRsGGRDcmVzpERjaGFuoENncnCgQ3VzcqFnbXl1c2VyMRNDc3BjoWhteXNwYWNlMQtDcGF0pERjaGFuoENncnCgQ3VzcqBDc3BjoERtZXRhoENzaWdYIB7XBurOX7mqPU9m2QoFkUN3Jkdu5Z2E9N8lbOnOh9t6";
            try
            {
                token = token.Replace('_', '/').Replace('-', '+').TrimEnd(new char[] { '=' });
                byte[] tokenByteArray = Convert.FromBase64String(token);
                System.IO.Stream stream = new System.IO.MemoryStream(tokenByteArray);

                Dahomey.Cbor.CborOptions options = new Dahomey.Cbor.CborOptions() { IsIndented = false,  EnumFormat = Dahomey.Cbor.ValueFormat.WriteToString };
                Dahomey.Cbor.ObjectModel.CborObject dahomeyRawcborData = Dahomey.Cbor.Cbor.DeserializeAsync<Dahomey.Cbor.ObjectModel.CborObject>(stream, options).Result;
                string dahomeyByteStringKeyData = dahomeyRawcborData.ToString();
                if (dahomeyRawcborData != null && dahomeyRawcborData.Count > 0)
                {
                    Dictionary<string, string> updates = IterateCborObject(dahomeyRawcborData);

                    foreach(KeyValuePair<string, string> kvp in updates)
                    {
                        dahomeyByteStringKeyData = dahomeyByteStringKeyData.Replace(kvp.Key, kvp.Value);
                    }
                }
                System.Diagnostics.Debug.WriteLine("dahomeyRawcborData = " + dahomeyByteStringKeyData);
                actual = dahomeyByteStringKeyData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception = " + ex.ToString());
            }
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void PeteroCborParseTest()
        {
            string expected = "{\"v\":2,\"t\":1568264210,\"ttl\":100,\"res\":{\"chan\":{},\"grp\":{},\"usr\":{\"myuser1\":19},\"spc\":{\"myspace1\":11}},\"pat\":{\"chan\":{},\"grp\":{},\"usr\":{},\"spc\":{}},\"meta\":{},\"sig\":[30,-41,6,-22,-50,95,-71,-86,61,79,102,-39,10,5,-111,67,119,38,71,110,-27,-99,-124,-12,-33,37,108,-23,-50,-121,-37,122]}";
            string actual = "";
            string token = "p0F2AkF0Gl150BJDdHRsGGRDcmVzpERjaGFuoENncnCgQ3VzcqFnbXl1c2VyMRNDc3BjoWhteXNwYWNlMQtDcGF0pERjaGFuoENncnCgQ3VzcqBDc3BjoERtZXRhoENzaWdYIB7XBurOX7mqPU9m2QoFkUN3Jkdu5Z2E9N8lbOnOh9t6";
            try
            {
                token = token.Replace('_', '/').Replace('-', '+').TrimEnd(new char[] { '=' });
                byte[] tokenByteArray = Convert.FromBase64String(token);
                System.Diagnostics.Debug.WriteLine(string.Join("], [", tokenByteArray));
                System.IO.Stream stream = new System.IO.MemoryStream(tokenByteArray);
                PeterO.Cbor.CBORObject peterCborObj = PeterO.Cbor.CBORObject.Read(stream);
                //PeterO.Cbor.CBORObject peterCborObj = PeterO.Cbor.CBORObject.DecodeFromBytes(tokenByteArray);
                string peteroByteStringKeyData = peterCborObj.ToString();
                System.Diagnostics.Debug.WriteLine("peterCborObj = " + peteroByteStringKeyData);
                actual = peteroByteStringKeyData;
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullEncryption()
        {
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////serialized string
            string message = null;

            ////encrypt
            string encryptedMessage = pc.Encrypt(message);
        }

        /// <summary>
        /// Tests the null decryption.
        /// Assumes that the input message is  deserialized  
        /// </summary>        
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullDecryption()
        {
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////deserialized string
            string message = null;
            ////decrypt
            string decryptedMessage = pc.Decrypt(message);

            Assert.AreEqual("", decryptedMessage);
        }

        /// <summary>
        /// Tests the yay decryption.
        /// Assumes that the input message is deserialized  
        /// Decrypted string should match yay!
        /// </summary>
        [Test]
        public void TestYayDecryptionBasic()
        {
            PubnubCrypto pc = new PubnubCrypto("enigma");
            string message = "q/xJqqN6qbiZMXYmiQC1Fw==";
            ////decrypt
            string decryptedMessage = pc.Decrypt(message);
            ////deserialize again
            Assert.AreEqual("yay!", decryptedMessage);
        }

        /// <summary>
        /// Tests the yay encryption.
        /// The output is not serialized
        /// Encrypted string should match q/xJqqN6qbiZMXYmiQC1Fw==
        /// </summary>
        [Test]
        public void TestYayEncryptionBasic()
        {
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////deserialized string
            string message = "yay!";
            ////Encrypt
            string encryptedMessage = pc.Encrypt(message);
            Assert.AreEqual("q/xJqqN6qbiZMXYmiQC1Fw==", encryptedMessage);
        }

        /// <summary>
        /// Tests the yay decryption.
        /// Assumes that the input message is not deserialized  
        /// Decrypted and Deserialized string should match yay!
        /// </summary>
        [Test]
        public void TestYayDecryption()
        {
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////string strMessage= "\"q/xJqqN6qbiZMXYmiQC1Fw==\"";
            ////Non deserialized string
            string message = "\"Wi24KS4pcTzvyuGOHubiXg==\"";
            ////Deserialize
            message = JsonConvert.DeserializeObject<string>(message);
            ////decrypt
            string decryptedMessage = pc.Decrypt(message);
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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////deserialized string
            string message = "yay!";
            ////serialize the string
            message = JsonConvert.SerializeObject(message);
            Debug.WriteLine(message);
            ////Encrypt
            string enc = pc.Encrypt(message);
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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////create an empty array object
            object[] emptyArray = { };
            ////serialize
            string serializedArray = JsonConvert.SerializeObject(emptyArray);
            ////Encrypt
            string encryptedMessage = pc.Encrypt(serializedArray);

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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////Input the deserialized string
            string message = "Ns4TB41JjT2NCXaGLWSPAQ==";
            ////decrypt
            string decryptedMessage = pc.Decrypt(message);
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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////create an object
            Object obj = new Object();
            ////serialize
            string serializedObject = JsonConvert.SerializeObject(obj);
            ////encrypt
            string encryptedMessage = pc.Encrypt(serializedObject);

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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////Deserialized
            string message = "IDjZE9BHSjcX67RddfCYYg==";
            ////Decrypt
            string decryptedMessage = pc.Decrypt(message);
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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////create an object of the custom class
            CustomClass cc = new CustomClass();
            ////serialize it
            string result = JsonConvert.SerializeObject(cc);
            ////encrypt it
            string encryptedMessage = pc.Encrypt(result);

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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////Deserialized
            string message = "Zbr7pEF/GFGKj1rOstp0tWzA4nwJXEfj+ezLtAr8qqE=";
            ////Decrypt
            string decryptedMessage = pc.Decrypt(message);
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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////Deserialized
            string message = "Pubnub Messaging API 2";
            ////serialize the message
            message = JsonConvert.SerializeObject(message);
            ////encrypt
            string encryptedMessage = pc.Encrypt(message);

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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////Deserialized string    
            string message = "f42pIQcWZ9zbTbH8cyLwB/tdvRxjFLOYcBNMVKeHS54=";
            ////Decrypt
            string decryptedMessage = pc.Decrypt(message);
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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////non serialized string
            string message = "Pubnub Messaging API 1";
            ////serialize
            message = JsonConvert.SerializeObject(message);
            ////encrypt
            string encryptedMessage = pc.Encrypt(message);

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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////deserialized string
            string message = "f42pIQcWZ9zbTbH8cyLwByD/GsviOE0vcREIEVPARR0=";
            ////decrypt
            string decryptedMessage = pc.Decrypt(message);
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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////input serialized string
            string message = "{\"this stuff\":{\"can get\":\"complicated!\"}}";
            ////encrypt
            string encryptedMessage = pc.Encrypt(message);

            Assert.AreEqual("zMqH/RTPlC8yrAZ2UhpEgLKUVzkMI2cikiaVg30AyUu7B6J0FLqCazRzDOmrsFsF", encryptedMessage);
        }

        /// <summary>
        /// Tests the stuffcan decryption.
        /// Assumes that the input message is  deserialized  
        /// </summary>
        [Test]
        public void TestStuffcanDecryption()
        {
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////deserialized string
            string message = "zMqH/RTPlC8yrAZ2UhpEgLKUVzkMI2cikiaVg30AyUu7B6J0FLqCazRzDOmrsFsF";
            ////decrypt
            string decryptedMessage = pc.Decrypt(message);

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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////serialized string
            string message = "{\"foo\":{\"bar\":\"foobar\"}}";
            ////encrypt
            string encryptedMessage = pc.Encrypt(message);

            Assert.AreEqual("GsvkCYZoYylL5a7/DKhysDjNbwn+BtBtHj2CvzC4Y4g=", encryptedMessage);
        }

        /// <summary>
        /// Tests the hash decryption.
        /// Assumes that the input message is  deserialized  
        /// </summary>        
        [Test]
        public void TestHashDecryption()
        {
            PubnubCrypto pc = new PubnubCrypto("enigma");
            ////deserialized string
            string message = "GsvkCYZoYylL5a7/DKhysDjNbwn+BtBtHj2CvzC4Y4g=";
            ////decrypt
            string decryptedMessage = pc.Decrypt(message);

            Assert.AreEqual("{\"foo\":{\"bar\":\"foobar\"}}", decryptedMessage);
        }

        /// <summary>
        /// Tests the unicode chars encryption.
        /// The input is not serialized
        /// </summary>
        [Test]
        public void TestUnicodeCharsEncryption()
        {
            PubnubCrypto pc = new PubnubCrypto("enigma");
            string message = "漢語";

            message = JsonConvert.SerializeObject(message);
            Debug.WriteLine(message);
            string encryptedMessage = pc.Encrypt(message);
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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            string message = "+BY5/miAA8aeuhVl4d13Kg==";
            ////decrypt
            string decryptedMessage = pc.Decrypt(message);
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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            string message = "stpgsG1DZZxb44J7mFNSzg==";

            ////decrypt
            string decryptedMessage = pc.Decrypt(message);
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
            PubnubCrypto pc = new PubnubCrypto("enigma");
            string message = "ÜÖ";

            message = JsonConvert.SerializeObject(message);
            Debug.WriteLine(message);
            string encryptedMessage = pc.Encrypt(message);
            Debug.WriteLine(encryptedMessage);
            Assert.AreEqual("stpgsG1DZZxb44J7mFNSzg==", encryptedMessage);
        }

        [Test]
        public void TestPAMSignature()
        {
            PubnubCrypto crypto = new PubnubCrypto("");
            string secretKey = "secret";
            string message = "Pubnub Messaging 1";

            string signature = crypto.PubnubAccessManagerSign(secretKey, message);
            System.Diagnostics.Debug.WriteLine("TestPAMSignature = " + signature);

            Assert.AreEqual("mIoxTVM2WAM5j-M2vlp9bVblDLoZQI5XIoYyQ48U0as=", signature);
        }

        [Test]
        public void TestPAMv3Signature()
        {
            PubnubCrypto crypto = new PubnubCrypto("");
            string secretKey = "wMfbo9G0xVUG8yfTfYw5qIdfJkTd7A";
            string message = "POST\ndemo\n/v3/pam/demo/grant\nPoundsSterling=%C2%A313.37&timestamp=123456789\n{\n  \"ttl\": 1440,\n  \"permissions\": {\n    \"resources\" : {\n      \"channels\": {\n        \"inbox-jay\": 3\n      },\n      \"groups\": {},\n      \"users\": {},\n      \"spaces\": {}\n    },\n    \"patterns\" : {\n      \"channels\": {},\n      \"groups\": {},\n      \"users\": {},\n      \"spaces\": {}\n    },\n    \"meta\": {\n      \"user-id\": \"jay@example.com\",\n      \"contains-unicode\": \"The 💩 test.\"\n    }\n  }\n}";

            string signature = crypto.PubnubAccessManagerSign(secretKey, message);
            signature = string.Format("v2.{0}", signature.TrimEnd(new char[] { '=' }));
            System.Diagnostics.Debug.WriteLine("TestPAMv3Signature = " + signature);

            Assert.AreEqual("v2.k80LsDMD-sImA8rCBj-ntRKhZ8mSjHY8Ivngt9W3Yc4", signature);
        }


        public static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m =>
                {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }
    }
}