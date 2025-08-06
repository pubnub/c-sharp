using System;
using NUnit.Framework;
using PubnubApi;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenPresenceIntervalValidation : TestHarness
    {
        private TestLogger testLogger;
        private PNConfiguration config;

        [SetUp]
        public void SetUp()
        {
            testLogger = new TestLogger();
            config = new PNConfiguration(new UserId("test-user"))
            {
                PublishKey = "test-pub-key",
                SubscribeKey = "test-sub-key",
                LogLevel = PubnubLogLevel.Warn
            };
            // Add our test logger to capture log messages
            config.Logger.AddLogger(testLogger);
        }

        [Test]
        public void SetPresenceTimeoutWithCustomInterval_ShouldAllowValidInterval()
        {
            // Test with valid interval (>= 3 seconds)
            config.SetPresenceTimeoutWithCustomInterval(60, 5);
            
            Assert.AreEqual(5, config.PresenceInterval, "Valid interval should be set correctly");
            Assert.IsEmpty(testLogger.WarnMessages, "No warning should be logged for valid interval");
        }

        [Test]
        public void SetPresenceTimeoutWithCustomInterval_ShouldResetIntervalBelow3Seconds()
        {
            // Test with interval below 3 seconds
            config.SetPresenceTimeoutWithCustomInterval(60, 1);
            
            Assert.AreEqual(3, config.PresenceInterval, "Interval below 3 seconds should be reset to 3");
            Assert.IsNotEmpty(testLogger.WarnMessages, "Warning should be logged");
            StringAssert.Contains("presence/heartbeat interval cannot be set to less than 3 seconds", testLogger.WarnMessages[0]);
            StringAssert.Contains("Provided value 1 has been reset to 3 seconds", testLogger.WarnMessages[0]);
        }

        [Test]
        public void SetPresenceTimeoutWithCustomInterval_ShouldResetIntervalOfZero()
        {
            // Test with interval of 0
            config.SetPresenceTimeoutWithCustomInterval(60, 0);
            
            Assert.AreEqual(3, config.PresenceInterval, "Interval of 0 should be reset to 3");
            Assert.IsNotEmpty(testLogger.WarnMessages, "Warning should be logged");
            StringAssert.Contains("presence/heartbeat interval cannot be set to less than 3 seconds", testLogger.WarnMessages[0]);
            StringAssert.Contains("Provided value 0 has been reset to 3 seconds", testLogger.WarnMessages[0]);
        }

        [Test]
        public void SetPresenceTimeoutWithCustomInterval_ShouldResetNegativeInterval()
        {
            // Test with negative interval
            config.SetPresenceTimeoutWithCustomInterval(60, -5);
            
            Assert.AreEqual(3, config.PresenceInterval, "Negative interval should be reset to 3");
            Assert.IsNotEmpty(testLogger.WarnMessages, "Warning should be logged");
            StringAssert.Contains("presence/heartbeat interval cannot be set to less than 3 seconds", testLogger.WarnMessages[0]);
            StringAssert.Contains("Provided value -5 has been reset to 3 seconds", testLogger.WarnMessages[0]);
        }

        [Test]
        public void SetPresenceTimeoutWithCustomInterval_ShouldAllowExactlyThreeSeconds()
        {
            // Test with exactly 3 seconds (boundary condition)
            config.SetPresenceTimeoutWithCustomInterval(60, 3);
            
            Assert.AreEqual(3, config.PresenceInterval, "Interval of exactly 3 seconds should be allowed");
            Assert.IsEmpty(testLogger.WarnMessages, "No warning should be logged for interval of exactly 3 seconds");
        }

        [Test]
        public void PresenceTimeout_WithSmallValue_ShouldTriggerIntervalValidation()
        {
            // Test PresenceTimeout property that calculates interval as (timeout / 2) - 1
            // For timeout = 8, interval would be (8 / 2) - 1 = 3, which is acceptable
            config.PresenceTimeout = 8;
            
            Assert.AreEqual(3, config.PresenceInterval, "Calculated interval should be 3");
            Assert.IsEmpty(testLogger.WarnMessages, "No warning should be logged for calculated interval of 3");
            
            // Clear logs for next test
            testLogger.WarnMessages.Clear();
            
            // For timeout = 6, interval would be (6 / 2) - 1 = 2, which should trigger validation
            config.PresenceTimeout = 6;
            
            Assert.AreEqual(3, config.PresenceInterval, "Calculated interval below 3 should be reset to 3");
            Assert.IsNotEmpty(testLogger.WarnMessages, "Warning should be logged for calculated interval below 3");
            StringAssert.Contains("presence/heartbeat interval cannot be set to less than 3 seconds", testLogger.WarnMessages[0]);
            StringAssert.Contains("Provided value 2 has been reset to 3 seconds", testLogger.WarnMessages[0]);
        }

        [Test]
        public void SetPresenceTimeoutWithCustomInterval_ReturnsSelfForChaining()
        {
            // Test method chaining
            var result = config.SetPresenceTimeoutWithCustomInterval(60, 5);
            
            Assert.AreSame(config, result, "Method should return the same configuration instance for chaining");
        }

        // Helper class to capture log messages during tests
        private class TestLogger : IPubnubLogger
        {
            public List<string> WarnMessages { get; } = new List<string>();
            public List<string> DebugMessages { get; } = new List<string>();
            public List<string> ErrorMessages { get; } = new List<string>();
            public List<string> InfoMessages { get; } = new List<string>();
            public List<string> TraceMessages { get; } = new List<string>();

            public void Trace(string logMessage) => TraceMessages.Add(logMessage);
            public void Debug(string logMessage) => DebugMessages.Add(logMessage);
            public void Info(string logMessage) => InfoMessages.Add(logMessage);
            public void Warn(string logMessage) => WarnMessages.Add(logMessage);
            public void Error(string logMessage) => ErrorMessages.Add(logMessage);
        }
    }
}