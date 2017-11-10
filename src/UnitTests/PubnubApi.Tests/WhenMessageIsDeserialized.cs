using System;
using NUnit.Framework;
using PubnubApi;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenMessageIsDeserialized
    {
        private static string authKey = "myAuth";
        private PNConfiguration config;
        private IPubnubLog pubnubLog = null;

        [TestFixtureSetUp]
        public void Init()
        {
            config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Uuid = "mytestuuid",
                Secure = false
            };
        }

        [Test]
        public void JArrayMessageShouldDeserializedWithoutError()
        {
            String messageContent = "[{\"a\":1,\"b\":2}{\"c\":3,\"d\":4}]";
            Object[] content = new Object[] { messageContent };
            JArray jArray = new JArray(content);

            NewtonsoftJsonDotNet newtonsoftJsonDotNet = new NewtonsoftJsonDotNet(config, pubnubLog);
            var listObject = new List<object>
            {
                jArray,
                null,
                12345L,
                "MessageName"
            };
            PNMessageResult<String> result = newtonsoftJsonDotNet.DeserializeToObject<PNMessageResult<String>>(listObject);
            Assert.AreEqual("[\"[{\\\"a\\\":1,\\\"b\\\":2}{\\\"c\\\":3,\\\"d\\\":4}]\"]", result.Message);
        }
}
}
