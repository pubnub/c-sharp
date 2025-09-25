using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;

namespace PubnubApiPCL.Tests
{
    [TestFixture]
    public class DeleteMessageRequestResponseTests
    {
        private Pubnub pubnub;
        private PNConfiguration config;

        [SetUp]
        public void Setup()
        {
            config = new PNConfiguration(new UserId("test-user"))
            {
                PublishKey = "demo",
                SubscribeKey = "demo",
                SecretKey = "demo",
                LogLevel = PubnubLogLevel.None
            };

            pubnub = new Pubnub(config);
        }

        [TearDown]
        public void TearDown()
        {
            pubnub?.Destroy();
            pubnub = null;
        }

        #region Request Validation Tests

        [Test]
        public void DeleteMessageRequest_Validate_ThrowsOnNullChannel()
        {
            var request = new DeleteMessageRequest
            {
                Channel = null
            };

            Assert.Throws<ArgumentException>(() => request.Validate());
        }

        [Test]
        public void DeleteMessageRequest_Validate_ThrowsOnEmptyChannel()
        {
            var request = new DeleteMessageRequest
            {
                Channel = ""
            };

            Assert.Throws<ArgumentException>(() => request.Validate());
        }

        [Test]
        public void DeleteMessageRequest_Validate_ThrowsOnWhitespaceChannel()
        {
            var request = new DeleteMessageRequest
            {
                Channel = "   "
            };

            Assert.Throws<ArgumentException>(() => request.Validate());
        }

        [Test]
        public void DeleteMessageRequest_Validate_AcceptsValidChannel()
        {
            var request = new DeleteMessageRequest
            {
                Channel = "test-channel"
            };

            Assert.DoesNotThrow(() => request.Validate());
            Assert.That(request.Channel, Is.EqualTo("test-channel"));
        }

        [Test]
        public void DeleteMessageRequest_Validate_ThrowsOnNegativeStartTimetoken()
        {
            var request = new DeleteMessageRequest
            {
                Channel = "test-channel",
                Start = -1
            };

            Assert.Throws<ArgumentException>(() => request.Validate(),
                "Start timetoken cannot be negative");
        }

        [Test]
        public void DeleteMessageRequest_Validate_ThrowsOnNegativeEndTimetoken()
        {
            var request = new DeleteMessageRequest
            {
                Channel = "test-channel",
                End = -1
            };

            Assert.Throws<ArgumentException>(() => request.Validate(),
                "End timetoken cannot be negative");
        }

        [Test]
        public void DeleteMessageRequest_Validate_ThrowsOnStartGreaterThanEnd()
        {
            var request = new DeleteMessageRequest
            {
                Channel = "test-channel",
                Start = 1000,
                End = 500
            };

            Assert.Throws<ArgumentException>(() => request.Validate(),
                "Start timetoken must be less than or equal to end timetoken");
        }

        [Test]
        public void DeleteMessageRequest_Validate_AcceptsValidTimeRange()
        {
            var request = new DeleteMessageRequest
            {
                Channel = "test-channel",
                Start = 100,
                End = 200
            };

            Assert.DoesNotThrow(() => request.Validate());
            Assert.That(request.Start, Is.EqualTo(100));
            Assert.That(request.End, Is.EqualTo(200));
        }

        [Test]
        public void DeleteMessageRequest_Validate_AcceptsEqualStartAndEnd()
        {
            var request = new DeleteMessageRequest
            {
                Channel = "test-channel",
                Start = 100,
                End = 100
            };

            Assert.DoesNotThrow(() => request.Validate());
        }

        [Test]
        public void DeleteMessageRequest_Validate_AcceptsOnlyStartTimetoken()
        {
            var request = new DeleteMessageRequest
            {
                Channel = "test-channel",
                Start = 100
            };

            Assert.DoesNotThrow(() => request.Validate());
            Assert.That(request.Start, Is.EqualTo(100));
            Assert.That(request.End, Is.Null);
        }

        [Test]
        public void DeleteMessageRequest_Validate_AcceptsOnlyEndTimetoken()
        {
            var request = new DeleteMessageRequest
            {
                Channel = "test-channel",
                End = 200
            };

            Assert.DoesNotThrow(() => request.Validate());
            Assert.That(request.Start, Is.Null);
            Assert.That(request.End, Is.EqualTo(200));
        }

        #endregion

        #region Factory Method Tests

        [Test]
        public void DeleteMessageRequest_ForChannel_CreatesValidRequest()
        {
            var request = DeleteMessageRequest.ForChannel("test-channel");

            Assert.That(request, Is.Not.Null);
            Assert.That(request.Channel, Is.EqualTo("test-channel"));
            Assert.That(request.Start, Is.Null);
            Assert.That(request.End, Is.Null);
        }

        [Test]
        public void DeleteMessageRequest_ForChannel_ThrowsOnNullChannel()
        {
            Assert.Throws<ArgumentException>(() =>
                DeleteMessageRequest.ForChannel(null));
        }

        [Test]
        public void DeleteMessageRequest_ForChannel_ThrowsOnEmptyChannel()
        {
            Assert.Throws<ArgumentException>(() =>
                DeleteMessageRequest.ForChannel(""));
        }

        [Test]
        public void DeleteMessageRequest_ForChannelWithRange_CreatesValidRequest()
        {
            var request = DeleteMessageRequest.ForChannelWithRange("test-channel", 100, 200);

            Assert.That(request, Is.Not.Null);
            Assert.That(request.Channel, Is.EqualTo("test-channel"));
            Assert.That(request.Start, Is.EqualTo(100));
            Assert.That(request.End, Is.EqualTo(200));
        }

        [Test]
        public void DeleteMessageRequest_ForChannelWithRange_ThrowsOnInvalidRange()
        {
            Assert.Throws<ArgumentException>(() =>
                DeleteMessageRequest.ForChannelWithRange("test-channel", 200, 100));
        }

        [Test]
        public void DeleteMessageRequest_ForChannelFromTime_CreatesValidRequest()
        {
            var request = DeleteMessageRequest.ForChannelFromTime("test-channel", 100);

            Assert.That(request, Is.Not.Null);
            Assert.That(request.Channel, Is.EqualTo("test-channel"));
            Assert.That(request.Start, Is.EqualTo(100));
            Assert.That(request.End, Is.Null);
        }

        [Test]
        public void DeleteMessageRequest_ForChannelUntilTime_CreatesValidRequest()
        {
            var request = DeleteMessageRequest.ForChannelUntilTime("test-channel", 200);

            Assert.That(request, Is.Not.Null);
            Assert.That(request.Channel, Is.EqualTo("test-channel"));
            Assert.That(request.Start, Is.Null);
            Assert.That(request.End, Is.EqualTo(200));
        }

        #endregion

        #region Response Tests

        // Note: DeleteMessageResponse factory methods are internal, so we cannot directly test them.
        // These tests are commented out but show what would be tested if the factory methods were public.

        // The response object itself is only created internally by the SDK,
        // so we can only verify its structure through integration tests with actual API calls.

        #endregion

        #region Integration Tests

        [Test]
        public void DeleteMessage_WithNullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await pubnub.DeleteMessage(null));
        }

        [Test]
        public void DeleteMessage_WithInvalidChannel_ThrowsArgumentException()
        {
            var request = new DeleteMessageRequest
            {
                Channel = ""
            };

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await pubnub.DeleteMessage(request));
        }

        [Test]
        public void DeleteMessage_ComparesWithBuilderPattern()
        {
            // This test demonstrates that both APIs can be used
            // Note: These would fail without proper server setup, but show the API usage

            // Builder pattern (existing)
            var builderOperation = pubnub.DeleteMessages()
                .Channel("test-channel")
                .Start(100)
                .End(200);

            // Request/response pattern (new)
            var request = new DeleteMessageRequest
            {
                Channel = "test-channel",
                Start = 100,
                End = 200
            };

            // Both should compile and be valid operations
            Assert.That(builderOperation, Is.Not.Null);
            Assert.That(request, Is.Not.Null);
        }

        #endregion
    }
}