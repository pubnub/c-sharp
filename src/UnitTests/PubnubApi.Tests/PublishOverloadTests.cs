using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.EndPoint;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class PublishOverloadTests
    {
        private Pubnub pubnub;
        private PNConfiguration config;

        [SetUp]
        public void Setup()
        {
            config = new PNConfiguration(new UserId("test-user-id"))
            {
                PublishKey = "test-publish-key",
                SubscribeKey = "test-subscribe-key",
                LogLevel = PubnubLogLevel.All
            };
            pubnub = new Pubnub(config);
        }

        [TearDown]
        public void Cleanup()
        {
            pubnub?.Destroy();
        }

        [Test]
        public void Publish_ReturnsBuilderOperation_WhenCalledWithoutParameters()
        {
            // Act
            var result = pubnub.Publish();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<PublishOperation>(result);
        }

        [Test]
        public void PublishRequest_ShouldValidateRequiredFields()
        {
            // Test valid request
            var validRequest = new PublishRequest
            {
                Message = "Hello World",
                Channel = "test-channel"
            };

            Assert.DoesNotThrow(() => validRequest.Validate());

            // Test invalid requests
            var emptyChannelRequest = new PublishRequest
            {
                Message = "Hello World",
                Channel = ""
            };

            var ex1 = Assert.Throws<ArgumentException>(() => emptyChannelRequest.Validate());
            Assert.AreEqual("Channel", ex1.ParamName);

            var nullMessageRequest = new PublishRequest
            {
                Message = null,
                Channel = "test-channel"
            };

            var ex2 = Assert.Throws<ArgumentException>(() => nullMessageRequest.Validate());
            Assert.AreEqual("Message", ex2.ParamName);
        }

        [Test]
        public void PublishRequest_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var request = new PublishRequest();

            // Assert
            Assert.IsTrue(request.StoreInHistory);
            Assert.AreEqual(-1, request.Ttl);
            Assert.IsFalse(request.UsePost);
            Assert.IsNull(request.Metadata);
            Assert.IsNull(request.CustomMessageType);
            Assert.IsNull(request.QueryParameters);
        }

        [Test]
        public void PublishResponse_CreateSuccess_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var timetoken = 15234567890123456L;
            var channel = "test-channel";
            var statusCode = 200;

            // Act
            var response = PublishResponse.CreateSuccess(timetoken, channel, statusCode);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(timetoken, response.Timetoken);
            Assert.AreEqual(channel, response.Channel);
            Assert.AreEqual(statusCode, response.StatusCode);
            Assert.IsNull(response.ErrorMessage);
            Assert.IsNotNull(response.Headers);
        }

        [Test]
        public void PublishResponse_CreateError_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var channel = "test-channel";
            var errorMessage = "Test error message";
            var statusCode = 400;

            // Act
            var response = PublishResponse.CreateError(channel, errorMessage, statusCode);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(0, response.Timetoken);
            Assert.AreEqual(channel, response.Channel);
            Assert.AreEqual(statusCode, response.StatusCode);
            Assert.AreEqual(errorMessage, response.ErrorMessage);
            Assert.IsNotNull(response.Headers);
        }

        [Test]
        public void PublishOverload_WithNullRequest_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            try
            {
                var task = pubnub.Publish(null);
                task.Wait();
                Assert.Fail("Expected ArgumentNullException was not thrown");
            }
            catch (AggregateException aggEx)
            {
                var ex = aggEx.InnerException as ArgumentNullException;
                Assert.IsNotNull(ex);
                Assert.AreEqual("request", ex.ParamName);
                Assert.IsTrue(ex.Message.Contains("PublishRequest cannot be null"));
            }
        }

        [Test]
        public void PublishOverload_WithInvalidPublishKey_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var invalidConfig = new PNConfiguration(new UserId("test-user"))
            {
                SubscribeKey = "test-subscribe-key"
                // No PublishKey set
            };
            var invalidPubnub = new Pubnub(invalidConfig);

            var request = new PublishRequest
            {
                Message = "Hello World",
                Channel = "test-channel"
            };

            // Act & Assert
            try
            {
                var task = invalidPubnub.Publish(request);
                task.Wait();
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (AggregateException aggEx)
            {
                var ex = aggEx.InnerException as InvalidOperationException;
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Message.Contains("PublishKey is required"));
            }

            invalidPubnub.Destroy();
        }

        [Test]
        public void BothPublishApis_ShouldCoexistOnSameInstance()
        {
            // Test that both APIs are available on the same Pubnub instance

            // Test builder pattern API
            var builderOperation = pubnub.Publish();
            Assert.IsNotNull(builderOperation);
            Assert.IsInstanceOf<PublishOperation>(builderOperation);

            // Test request/response API
            var request = new PublishRequest
            {
                Message = "Test message",
                Channel = "test-channel"
            };

            Assert.DoesNotThrow(() => request.Validate());

            // Both should be available without conflicts
            Assert.IsNotNull(pubnub.Publish()); // Builder pattern
        }
    }
}