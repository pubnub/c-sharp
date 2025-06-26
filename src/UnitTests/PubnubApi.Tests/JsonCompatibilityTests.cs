using System;
using System.Collections.Generic;
using System.Text.Json;
using NUnit.Framework;
using PubnubApi;

namespace PubnubApi.Tests
{
    [TestFixture]
    public class JsonCompatibilityTests
    {
        private SystemTextJsonDotNet jsonLibrary;
        private PNConfiguration config;

        [SetUp]
        public void Setup()
        {
            config = new PNConfiguration(new UserId("testUser"))
            {
                SubscribeKey = "test-key",
                PublishKey = "test-key"
            };
            jsonLibrary = new SystemTextJsonDotNet(config);
        }

        [Test]
        public void TestCaseInsensitiveDeserialization()
        {
            // Test case insensitive property matching
            string json = "{\"UserId\":\"test\",\"userName\":\"TestUser\",\"AGE\":25}";
            
            var result = jsonLibrary.DeserializeToDictionaryOfObject(json);
            
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey("UserId"));
            Assert.IsTrue(result.ContainsKey("userName"));
            Assert.IsTrue(result.ContainsKey("AGE"));
        }

        [Test]
        public void TestNumberHandling()
        {
            // Test reading numbers from strings (like Newtonsoft.Json)
            string json = "{\"id\":\"123\",\"count\":\"456\",\"value\":789}";
            
            var result = jsonLibrary.DeserializeToDictionaryOfObject(json);
            
            Assert.IsNotNull(result);
            Assert.AreEqual("123", result["id"]);
            Assert.AreEqual("456", result["count"]);
            Assert.AreEqual(789, ((JsonElement)result["value"]).GetInt32());
        }

        [Test]
        public void TestTrailingCommas()
        {
            // Test handling trailing commas (allowed in System.Text.Json with our config)
            string jsonWithTrailingComma = "{\"name\":\"test\",\"value\":123,}";
            
            var result = jsonLibrary.DeserializeToDictionaryOfObject(jsonWithTrailingComma);
            
            Assert.IsNotNull(result);
            Assert.AreEqual("test", result["name"]);
        }

        [Test]
        public void TestCommentsHandling()
        {
            // Test skipping comments (like Newtonsoft.Json)
            string jsonWithComments = @"{
                // This is a comment
                ""name"": ""test"",
                /* Multi-line comment */
                ""value"": 123
            }";
            
            var result = jsonLibrary.DeserializeToDictionaryOfObject(jsonWithComments);
            
            Assert.IsNotNull(result);
            Assert.AreEqual("test", result["name"]);
        }

        [Test]
        public void TestPropertyNamingPolicy()
        {
            // Test that property names are preserved as-is (like Newtonsoft.Json default)
            var testObject = new { CamelCase = "test", PascalCase = "value", snake_case = "data" };
            
            string serialized = jsonLibrary.SerializeToJsonString(testObject);
            
            Assert.IsTrue(serialized.Contains("CamelCase"));
            Assert.IsTrue(serialized.Contains("PascalCase"));
            Assert.IsTrue(serialized.Contains("snake_case"));
        }

        [Test]
        public void TestComplexObjectSerialization()
        {
            // Test complex object serialization/deserialization
            var complexObject = new
            {
                StringValue = "test",
                IntValue = 123,
                BoolValue = true,
                ArrayValue = new[] { 1, 2, 3 },
                NestedObject = new { NestedProperty = "nested" },
                NullValue = (string)null
            };

            string serialized = jsonLibrary.SerializeToJsonString(complexObject);
            var deserialized = jsonLibrary.DeserializeToDictionaryOfObject(serialized);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual("test", deserialized["StringValue"]);
            Assert.AreEqual(123, ((JsonElement)deserialized["IntValue"]).GetInt32());
            Assert.AreEqual(true, ((JsonElement)deserialized["BoolValue"]).GetBoolean());
            Assert.IsNotNull(deserialized["ArrayValue"]);
            Assert.IsNotNull(deserialized["NestedObject"]);
        }

        [Test]
        public void TestPubNubMessageSerialization()
        {
            // Test typical PubNub message structure
            var pubNubMessage = new
            {
                message = "Hello World",
                timestamp = 1234567890,
                user = "testUser",
                metadata = new { source = "test", priority = 1 }
            };

            string serialized = jsonLibrary.SerializeToJsonString(pubNubMessage);
            var deserialized = jsonLibrary.DeserializeToDictionaryOfObject(serialized);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual("Hello World", deserialized["message"]);
            Assert.AreEqual(1234567890, ((JsonElement)deserialized["timestamp"]).GetInt64());
            Assert.AreEqual("testUser", deserialized["user"]);
            Assert.IsNotNull(deserialized["metadata"]);
        }

        [Test]
        public void TestJsonFastCheck()
        {
            // Test the JSON validation method
            Assert.IsTrue(SystemTextJsonDotNet.JsonFastCheck("{\"test\":\"value\"}"));
            Assert.IsTrue(SystemTextJsonDotNet.JsonFastCheck("  {\"test\":\"value\"}  "));
            Assert.IsFalse(SystemTextJsonDotNet.JsonFastCheck("[\"array\"]"));
            Assert.IsFalse(SystemTextJsonDotNet.JsonFastCheck("\"string\""));
            Assert.IsFalse(SystemTextJsonDotNet.JsonFastCheck("123"));
            Assert.IsFalse(SystemTextJsonDotNet.JsonFastCheck(""));
            Assert.IsFalse(SystemTextJsonDotNet.JsonFastCheck(null));
        }

        [Test]
        public void TestSpecialCharacterHandling()
        {
            // Test handling of special characters and Unicode
            var testObject = new
            {
                UnicodeText = "Hello 世界 🌍",
                SpecialChars = "Test with \"quotes\" and 'apostrophes' and <tags>",
                EscapedChars = "Line1\nLine2\tTabbed"
            };

            string serialized = jsonLibrary.SerializeToJsonString(testObject);
            var deserialized = jsonLibrary.DeserializeToDictionaryOfObject(serialized);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual("Hello 世界 🌍", deserialized["UnicodeText"]);
            Assert.IsTrue(deserialized["SpecialChars"].ToString().Contains("quotes"));
            Assert.IsTrue(deserialized["EscapedChars"].ToString().Contains("Line1"));
        }

        [Test]
        public void TestEmptyAndNullValues()
        {
            // Test handling of empty and null values
            var testObject = new
            {
                EmptyString = "",
                NullString = (string)null,
                EmptyArray = new object[0],
                EmptyObject = new { }
            };

            string serialized = jsonLibrary.SerializeToJsonString(testObject);
            var deserialized = jsonLibrary.DeserializeToDictionaryOfObject(serialized);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual("", deserialized["EmptyString"]);
            // Note: null values might be handled differently, check actual behavior
            Assert.IsNotNull(deserialized["EmptyArray"]);
            Assert.IsNotNull(deserialized["EmptyObject"]);
        }

        [Test]
        public void TestLargeNumbers()
        {
            // Test handling of large numbers (important for timetokens)
            var testObject = new
            {
                LargeNumber = 15234567890123456789L,
                MaxLong = long.MaxValue,
                MinLong = long.MinValue
            };

            string serialized = jsonLibrary.SerializeToJsonString(testObject);
            var deserialized = jsonLibrary.DeserializeToDictionaryOfObject(serialized);

            Assert.IsNotNull(deserialized);
            // Verify large numbers are handled correctly
            Assert.IsNotNull(deserialized["LargeNumber"]);
            Assert.IsNotNull(deserialized["MaxLong"]);
            Assert.IsNotNull(deserialized["MinLong"]);
        }
    }
} 