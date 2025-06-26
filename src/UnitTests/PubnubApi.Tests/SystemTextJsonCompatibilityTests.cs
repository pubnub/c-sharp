using System;
using System.Collections.Generic;
using NUnit.Framework;
using PubnubApi;

namespace PubnubApi.Tests
{
    [TestFixture]
    public class SystemTextJsonCompatibilityTests
    {
        private PNConfiguration config;
        private Pubnub pubnub;

        [SetUp]
        public void Setup()
        {
            // Configuration with System.Text.Json (now the only option)
            config = new PNConfiguration(new UserId("testUserId"))
            {
                SubscribeKey = "test-subscribe-key",
                PublishKey = "test-publish-key"
            };

            pubnub = new Pubnub(config);
        }

        [TearDown]
        public void TearDown()
        {
            pubnub?.Destroy();
        }

        [Test]
        public void TestDefaultJsonLibraryIsSystemTextJson()
        {
            // Test that default configuration uses System.Text.Json
            Assert.IsInstanceOf<SystemTextJsonDotNet>(pubnub.JsonPluggableLibrary);
        }

        [Test]
        public void TestDefaultJsonLibraryIsSystemTextJsonWithNewInstance()
        {
            // Test that a new instance also uses System.Text.Json
            var defaultConfig = new PNConfiguration(new UserId("defaultUserId"));
            var defaultPubnub = new Pubnub(defaultConfig);
            
            Assert.IsInstanceOf<SystemTextJsonDotNet>(defaultPubnub.JsonPluggableLibrary);
            
            defaultPubnub.Destroy();
        }

        [Test]
        public void TestSerializeToJsonString()
        {
            var testObject = new { Message = "Hello", Timestamp = 123456 };

            string jsonResult = pubnub.JsonPluggableLibrary.SerializeToJsonString(testObject);

            Assert.IsNotNull(jsonResult);
            Assert.IsTrue(jsonResult.Contains("Hello"));
            Assert.IsTrue(jsonResult.Length > 0);
        }

        [Test]
        public void TestDeserializeToObject()
        {
            string jsonString = "{\"Message\":\"Hello\",\"Timestamp\":123456}";

            var jsonResult = pubnub.JsonPluggableLibrary.DeserializeToObject(jsonString);

            Assert.IsNotNull(jsonResult);
        }

        [Test]
        public void TestIsDictionaryCompatible()
        {
            string jsonObject = "{\"key1\":\"value1\",\"key2\":\"value2\"}";
            string jsonArray = "[\"item1\",\"item2\",\"item3\"]";

            bool jsonObjectResult = pubnub.JsonPluggableLibrary.IsDictionaryCompatible(jsonObject, PNOperationType.PNPublishOperation);
            bool jsonArrayResult = pubnub.JsonPluggableLibrary.IsDictionaryCompatible(jsonArray, PNOperationType.PNPublishOperation);

            Assert.IsTrue(jsonObjectResult);
            Assert.IsFalse(jsonArrayResult);
        }

        [Test]
        public void TestDeserializeToListOfObject()
        {
            string jsonArray = "[\"item1\",\"item2\",\"item3\"]";

            var jsonResult = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(jsonArray);

            Assert.IsNotNull(jsonResult);
            Assert.IsInstanceOf<List<object>>(jsonResult);
            Assert.AreEqual(3, jsonResult.Count);
        }

        [Test]
        public void TestDeserializeToDictionaryOfObject()
        {
            string jsonObject = "{\"key1\":\"value1\",\"key2\":\"value2\"}";

            var jsonResult = pubnub.JsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonObject);

            Assert.IsNotNull(jsonResult);
            Assert.IsInstanceOf<Dictionary<string, object>>(jsonResult);
            Assert.AreEqual(2, jsonResult.Count);
            Assert.IsTrue(jsonResult.ContainsKey("key1"));
        }

        [Test]
        public void TestDeserializeToObjectGeneric()
        {
            string jsonString = "{\"Message\":\"Hello\",\"Timestamp\":123456}";

            var jsonResult = pubnub.JsonPluggableLibrary.DeserializeToObject<TestMessage>(jsonString);

            Assert.IsNotNull(jsonResult);
            Assert.IsInstanceOf<TestMessage>(jsonResult);
            Assert.AreEqual("Hello", jsonResult.Message);
            Assert.AreEqual(123456, jsonResult.Timestamp);
        }

        [Test]
        public void TestBuildJsonObject()
        {
            string jsonString = "{\"Message\":\"Hello\",\"Timestamp\":123456}";

            var jsonResult = pubnub.JsonPluggableLibrary.BuildJsonObject(jsonString);

            Assert.IsNotNull(jsonResult);
        }

        [Test]
        public void TestDeserializeWithInvalidJson()
        {
            string invalidJson = "{invalid json}";

            var jsonResult = pubnub.JsonPluggableLibrary.DeserializeToObject(invalidJson);

            // System.Text.Json should handle invalid JSON gracefully
            Assert.DoesNotThrow(() => pubnub.JsonPluggableLibrary.DeserializeToObject(invalidJson));
        }

        [Test]
        public void TestJsonFastCheck()
        {
            string validJson = "{\"key\":\"value\"}";
            string invalidJson = "not json";

            bool validResult = SystemTextJsonDotNet.JsonFastCheck(validJson);
            bool invalidResult = SystemTextJsonDotNet.JsonFastCheck(invalidJson);

            Assert.IsTrue(validResult);
            Assert.IsFalse(invalidResult);
        }
    }

    public class TestMessage
    {
        public string Message { get; set; }
        public long Timestamp { get; set; }
    }
} 