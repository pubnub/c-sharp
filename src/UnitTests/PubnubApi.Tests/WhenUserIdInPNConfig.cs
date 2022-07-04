using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenUserIdInPNConfig : TestHarness
    {
        [SetUp]
        public static void Init()
        { 
        }

        [TearDown]
        public static void Exit()
        {
            
        }

        [Test]
        public static void ThenUuidSetShouldFailWithUserIdConstructorValue()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                PNConfiguration config = new PNConfiguration(new UserId("newuserid"))
                {
                    SubscribeKey = "somesubkey",
                    PublishKey = "somepubkey",
                    SecretKey = "someseckey",
                    Uuid = "altnewuuidthatshouldfail"
                };
            });
        }

        [Test]
        public static void ThenUserIdSetShouldFailWithUuidConstructorValue()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                PNConfiguration config = new PNConfiguration("newuuid")
                {
                    SubscribeKey = "somesubkey",
                    PublishKey = "somepubkey",
                    SecretKey = "someseckey",
                    UserId = new UserId("altnewuseridthatshouldfail")
                };
            });
        }

        [Test]
        public static void ThenUserIdSetShouldGiveSameForUuid() 
        {
            PNConfiguration config = new PNConfiguration(new UserId("newuserid"))
            {
                SubscribeKey = "somesubkey",
                PublishKey = "somepubkey",
                SecretKey = "someseckey",
            };
            Assert.AreEqual(config.UserId.ToString(), config.Uuid);
        }

        [Test]
        public static void ThenUuidSetShouldGiveSameForUserId()
        {
            PNConfiguration config = new PNConfiguration("newuuid")
            {
                SubscribeKey = "somesubkey",
                PublishKey = "somepubkey",
                SecretKey = "someseckey",
            };
            Assert.AreEqual(config.UserId.ToString(), config.Uuid);
        }

    }
}
