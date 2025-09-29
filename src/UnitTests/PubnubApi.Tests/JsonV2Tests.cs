using System;
using System.Collections.Generic;
using NUnit.Framework;
using PubnubApi.JsonV2;

namespace PubnubApi.Tests
{
    [TestFixture]
    public class JsonV2Tests
    {
        private IJsonPluggableLibrary jsonLibraryV1;
        private IJsonPluggableLibraryV2 jsonLibraryV2;
        private PNConfiguration testConfig;

        [SetUp]
        public void Setup()
        {
            testConfig = new PNConfiguration(new UserId("test-user"))
            {
                PublishKey = "pub-c-test",
                SubscribeKey = "sub-c-test"
            };

            jsonLibraryV1 = new NewtonsoftJsonDotNet(testConfig);
            jsonLibraryV2 = new NewtonsoftJsonDotNetV2(testConfig);
        }

        #region UUID Metadata Tests

        [Test]
        public void UuidMetadata_ParseFromListObject_V1AndV2ProduceIdenticalResults()
        {
            // Arrange
            var testData = CreateSampleUuidMetadataListData();

            // Act - Test through public interface as SDK developer would
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNSetUuidMetadataResult>(testData);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNSetUuidMetadataResult>(testData);

            // Assert
            AssertUuidMetadataResultsEqual(resultV1, resultV2);
        }

        [Test]
        public void UuidMetadata_ParseFromJsonString_V1AndV2ProduceIdenticalResults()
        {
            // Arrange
            var testData = CreateSampleUuidMetadataListData();
            var jsonString = jsonLibraryV1.SerializeToJsonString(testData);

            // Act - Test through public interface
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNSetUuidMetadataResult>(jsonString);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNSetUuidMetadataResult>(jsonString);

            // Assert
            AssertUuidMetadataResultsEqual(resultV1, resultV2);
        }

        [Test]
        public void UuidMetadata_HandleMissingFields_BothVersionsHandleGracefully()
        {
            // Arrange - minimal data with some missing fields
            var minimalData = new List<object>
            {
                "success",
                new Dictionary<string, object>
                {
                    ["data"] = new Dictionary<string, object>
                    {
                        ["id"] = "minimal-uuid",
                        ["name"] = "Minimal User"
                        // Missing: email, externalId, profileUrl, status, type, updated, custom
                    }
                }
            };

            // Act
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNSetUuidMetadataResult>(minimalData);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNSetUuidMetadataResult>(minimalData);

            // Assert
            AssertUuidMetadataResultsEqual(resultV1, resultV2);
            Assert.AreEqual("minimal-uuid", resultV1.Uuid);
            Assert.AreEqual("Minimal User", resultV1.Name);
            Assert.IsNull(resultV1.Email);
            Assert.IsNull(resultV1.ExternalId);
        }

        [Test]
        public void UuidMetadata_HandleNullData_BothVersionsReturnSafely()
        {
            // Arrange
            List<object> nullData = null;
            var emptyData = new List<object>();

            // Act & Assert - should not throw exceptions
            Assert.DoesNotThrow(() => jsonLibraryV1.DeserializeToObject<PNSetUuidMetadataResult>(nullData));
            Assert.DoesNotThrow(() => jsonLibraryV1.DeserializeToObject<PNSetUuidMetadataResult>(emptyData));
            Assert.DoesNotThrow(() => jsonLibraryV2.DeserializeToObject<PNSetUuidMetadataResult>(nullData));
            Assert.DoesNotThrow(() => jsonLibraryV2.DeserializeToObject<PNSetUuidMetadataResult>(emptyData));
        }

        #endregion

        #region Channel Metadata Tests

        [Test]
        public void ChannelMetadata_ParseFromListObject_V1AndV2ProduceIdenticalResults()
        {
            // Arrange
            var testData = CreateSampleChannelMetadataListData();

            // Act - Test through public interface
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNSetChannelMetadataResult>(testData);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNSetChannelMetadataResult>(testData);

            // Assert
            AssertChannelMetadataResultsEqual(resultV1, resultV2);
        }

        [Test]
        public void ChannelMetadata_ParseFromJsonString_V1AndV2ProduceIdenticalResults()
        {
            // Arrange
            var testData = CreateSampleChannelMetadataListData();
            var jsonString = jsonLibraryV1.SerializeToJsonString(testData);

            // Act - Test through public interface
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNSetChannelMetadataResult>(jsonString);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNSetChannelMetadataResult>(jsonString);

            // Assert
            AssertChannelMetadataResultsEqual(resultV1, resultV2);
        }

        [Test]
        public void ChannelMetadata_HandleMissingFields_BothVersionsHandleGracefully()
        {
            // Arrange - minimal data with some missing fields
            var minimalData = new List<object>
            {
                "success",
                new Dictionary<string, object>
                {
                    ["data"] = new Dictionary<string, object>
                    {
                        ["id"] = "minimal-channel",
                        ["name"] = "Minimal Channel"
                        // Missing: description, status, type, updated, custom
                    }
                }
            };

            // Act
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNSetChannelMetadataResult>(minimalData);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNSetChannelMetadataResult>(minimalData);

            // Assert
            AssertChannelMetadataResultsEqual(resultV1, resultV2);
            Assert.AreEqual("minimal-channel", resultV1.Channel);
            Assert.AreEqual("Minimal Channel", resultV1.Name);
            Assert.IsNull(resultV1.Description);
        }

        #endregion

        #region Core Interface Tests

        [Test]
        public void BasicSerialization_V1VsV2_ProduceIdenticalResults()
        {
            // Arrange
            var testObject = new Dictionary<string, object>
            {
                ["string"] = "test",
                ["int"] = 123,
                ["long"] = 9876543210L,
                ["bool"] = false,
                ["double"] = 3.14,
                ["array"] = new[] { "a", "b", "c" },
                ["nested"] = new Dictionary<string, object>
                {
                    ["inner"] = "value"
                }
            };

            // Act
            var jsonV1 = jsonLibraryV1.SerializeToJsonString(testObject);
            var jsonV2 = jsonLibraryV2.SerializeToJsonString(testObject);

            var deserializedV1 = jsonLibraryV1.DeserializeToDictionaryOfObject(jsonV1);
            var deserializedV2 = jsonLibraryV2.DeserializeToDictionaryOfObject(jsonV2);

            // Assert - JSON strings should be identical
            Assert.AreEqual(jsonV1, jsonV2);
            
            // Deserialized objects should be identical
            Assert.AreEqual(deserializedV1.Count, deserializedV2.Count);
            foreach (var key in deserializedV1.Keys)
            {
                Assert.IsTrue(deserializedV2.ContainsKey(key));
            }
        }

        [Test]
        public void ListDeserialization_V1VsV2_ProduceIdenticalResults()
        {
            // Arrange
            var testList = new List<object> { "first", 123, true, new Dictionary<string, object> { ["key"] = "value" } };
            var jsonString = jsonLibraryV1.SerializeToJsonString(testList);

            // Act
            var listV1 = jsonLibraryV1.DeserializeToListOfObject(jsonString);
            var listV2 = jsonLibraryV2.DeserializeToListOfObject(jsonString);

            // Assert
            Assert.AreEqual(listV1.Count, listV2.Count);
            for (int i = 0; i < listV1.Count; i++)
            {
                if (listV1[i] is Dictionary<string, object> dictV1 && listV2[i] is Dictionary<string, object> dictV2)
                {
                    Assert.AreEqual(dictV1.Count, dictV2.Count);
                    foreach (var kvp in dictV1)
                    {
                        Assert.IsTrue(dictV2.ContainsKey(kvp.Key));
                        Assert.AreEqual(kvp.Value, dictV2[kvp.Key]);
                    }
                }
                else
                {
                    Assert.AreEqual(listV1[i], listV2[i]);
                }
            }
        }

        [Test]
        public void HybridApproach_AutomaticallyDetectsV2()
        {
            // Act - Simulate hybrid detection logic that SDK would use
            var isV1CompatibleWithV2 = jsonLibraryV1 is IJsonPluggableLibraryV2;
            var isV2Native = jsonLibraryV2 is IJsonPluggableLibraryV2;

            // Assert
            Assert.IsFalse(isV1CompatibleWithV2, "V1 library should not implement V2 interface");
            Assert.IsTrue(isV2Native, "V2 library should implement V2 interface");
            
            // Verify inheritance
            Assert.IsTrue(jsonLibraryV2 is IJsonPluggableLibrary, "V2 library should inherit from V1 interface");
        }

        [Test]
        public void Debug_V2ParsingIntegration()
        {
            // Arrange
            var testData = CreateSampleUuidMetadataListData();

            // Act - Test V2 parsing directly
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNSetUuidMetadataResult>(testData);
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNSetUuidMetadataResult>(testData);

            // Debug output
            Console.WriteLine($"V1 Result: Uuid={resultV1?.Uuid}, Updated='{resultV1?.Updated}', IsNull={resultV1 == null}");
            Console.WriteLine($"V2 Result: Uuid={resultV2?.Uuid}, Updated='{resultV2?.Updated}', IsNull={resultV2 == null}");

            // Basic assertions
            Assert.IsNotNull(resultV1, "V1 should not return null");
            Assert.IsNotNull(resultV2, "V2 should not return null");
        }

        [Test]
        public void TypeConversion_ComplexScenarios_V1VsV2HandleIdentically()
        {
            // Arrange
            var complexObject = new Dictionary<string, object>
            {
                ["stringToInt"] = "123",
                ["stringToLong"] = "9876543210",
                ["stringToBool"] = "true",
                ["intToString"] = 456,
                ["boolToString"] = false,
                ["nullValue"] = null,
                ["complexNested"] = new Dictionary<string, object>
                {
                    ["array"] = new object[] { 1, "two", true, null },
                    ["mixedTypes"] = new Dictionary<string, object>
                    {
                        ["timestamp"] = "1234567890",
                        ["flags"] = new[] { true, false, true }
                    }
                }
            };

            var jsonString = jsonLibraryV1.SerializeToJsonString(complexObject);

            // Act - Test both string and object deserialization
            var fromStringV1 = jsonLibraryV1.DeserializeToObject(jsonString);
            var fromStringV2 = jsonLibraryV2.DeserializeToObject(jsonString);

            var dictV1 = jsonLibraryV1.DeserializeToDictionaryOfObject(jsonString);
            var dictV2 = jsonLibraryV2.DeserializeToDictionaryOfObject(jsonString);

            // Assert - Both approaches should handle type conversions identically
            Assert.IsNotNull(fromStringV1);
            Assert.IsNotNull(fromStringV2);
            Assert.IsNotNull(dictV1);
            Assert.IsNotNull(dictV2);
            
            Assert.AreEqual(dictV1.Count, dictV2.Count);
        }

        [Test]
        public void SerializationRoundTrip_ComplexMetadata_PreservesDataIntegrity()
        {
            // Arrange
            var originalUuidData = CreateComplexUuidMetadata();
            var originalChannelData = CreateComplexChannelMetadata();

            // Act - Full round-trip serialization
            var uuidJsonV1 = jsonLibraryV1.SerializeToJsonString(originalUuidData);
            var uuidJsonV2 = jsonLibraryV2.SerializeToJsonString(originalUuidData);
            
            var channelJsonV1 = jsonLibraryV1.SerializeToJsonString(originalChannelData);
            var channelJsonV2 = jsonLibraryV2.SerializeToJsonString(originalChannelData);

            // Both should produce identical JSON
            Assert.AreEqual(uuidJsonV1, uuidJsonV2);
            Assert.AreEqual(channelJsonV1, channelJsonV2);

            // Parse back to objects
            var uuidResultV1 = jsonLibraryV1.DeserializeToObject<PNSetUuidMetadataResult>(uuidJsonV1);
            var uuidResultV2 = jsonLibraryV2.DeserializeToObject<PNSetUuidMetadataResult>(uuidJsonV2);
            
            var channelResultV1 = jsonLibraryV1.DeserializeToObject<PNSetChannelMetadataResult>(channelJsonV1);
            var channelResultV2 = jsonLibraryV2.DeserializeToObject<PNSetChannelMetadataResult>(channelJsonV2);

            // Assert - Final objects should be identical
            AssertUuidMetadataResultsEqual(uuidResultV1, uuidResultV2);
            AssertChannelMetadataResultsEqual(channelResultV1, channelResultV2);
        }

        #endregion

        #region History Tests

        [Test]
        public void History_ParseFromListObject_V1AndV2ProduceIdenticalResults()
        {
            // Arrange
            var testData = CreateSampleHistoryListData();

            // Act - Test through public interface
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNHistoryResult>(testData);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNHistoryResult>(testData);

            // Assert
            AssertHistoryResultsEqual(resultV1, resultV2);
        }

        [Test]
        public void History_ParseFromJsonString_V1AndV2ProduceIdenticalResults()
        {
            // Arrange
            var testData = CreateSampleHistoryListData();
            var jsonString = jsonLibraryV1.SerializeToJsonString(testData);

            // Act - Test through public interface
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNHistoryResult>(jsonString);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNHistoryResult>(jsonString);

            // Assert
            AssertHistoryResultsEqual(resultV1, resultV2);
        }

        [Test]
        public void History_HandleMixedMessageFormats_BothVersionsHandleGracefully()
        {
            // Arrange - mixed format with structured and simple messages
            var mixedData = CreateMixedFormatHistoryData();

            // Act
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNHistoryResult>(mixedData);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNHistoryResult>(mixedData);

            // Assert
            AssertHistoryResultsEqual(resultV1, resultV2);
            Assert.IsTrue(resultV1.Messages.Count >= 3, "Should have multiple message types");
        }

        [Test]
        public void History_HandleEmptyAndNullData_BothVersionsReturnSafely()
        {
            // Arrange
            List<object> emptyData = new List<object>();
            List<object> nullData = null;

            // Act & Assert - should not throw exceptions
            Assert.DoesNotThrow(() => jsonLibraryV1.DeserializeToObject<PNHistoryResult>(emptyData));
            Assert.DoesNotThrow(() => jsonLibraryV1.DeserializeToObject<PNHistoryResult>(nullData));
            Assert.DoesNotThrow(() => jsonLibraryV2.DeserializeToObject<PNHistoryResult>(emptyData));
            Assert.DoesNotThrow(() => jsonLibraryV2.DeserializeToObject<PNHistoryResult>(nullData));

            var resultV1Empty = jsonLibraryV1.DeserializeToObject<PNHistoryResult>(emptyData);
            var resultV2Empty = jsonLibraryV2.DeserializeToObject<PNHistoryResult>(emptyData);
            
            Assert.AreEqual(0, resultV1Empty.StartTimeToken);
            Assert.AreEqual(0, resultV2Empty.StartTimeToken);
            AssertHistoryResultsEqual(resultV1Empty, resultV2Empty);
        }

        #endregion

        #region Access Manager Grant Tests

        [Test]
        public void AccessManagerGrant_ParseFromListObject_V1AndV2ProduceIdenticalResults()
        {
            // Arrange
            var testData = CreateSampleAccessManagerGrantListData();

            // Act - Test through public interface
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNAccessManagerGrantResult>(testData);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNAccessManagerGrantResult>(testData);

            // Assert
            AssertAccessManagerGrantResultsEqual(resultV1, resultV2);
        }

        [Test]
        public void AccessManagerGrant_ParseFromJsonString_V1AndV2ProduceIdenticalResults()
        {
            // Arrange
            var testData = CreateSampleAccessManagerGrantListData();
            var jsonString = jsonLibraryV1.SerializeToJsonString(testData);

            // Act - Test through public interface
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNAccessManagerGrantResult>(jsonString);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNAccessManagerGrantResult>(jsonString);

            // Assert
            AssertAccessManagerGrantResultsEqual(resultV1, resultV2);
        }

        [Test]
        public void AccessManagerGrant_HandleComplexNestedStructure_BothVersionsHandleCorrectly()
        {
            // Arrange - complex structure with channels, channel-groups, and uuids
            var complexData = CreateComplexAccessManagerGrantData();

            // Act
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNAccessManagerGrantResult>(complexData);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNAccessManagerGrantResult>(complexData);

            // Assert
            AssertAccessManagerGrantResultsEqual(resultV1, resultV2);
            
            // Verify complex structure was parsed correctly
            Assert.IsNotNull(resultV1.Channels, "Should have channels");
            Assert.IsNotNull(resultV1.ChannelGroups, "Should have channel groups");
            Assert.IsNotNull(resultV1.Uuids, "Should have uuids");
            Assert.IsTrue(resultV1.Channels.Count > 0, "Should have parsed channels");
        }

        [Test]
        public void AccessManagerGrant_HandleSingleItemFormat_BothVersionsHandleCorrectly()
        {
            // Arrange - single item format instead of multi-item
            var singleItemData = CreateSingleItemAccessManagerGrantData();

            // Act
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNAccessManagerGrantResult>(singleItemData);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNAccessManagerGrantResult>(singleItemData);

            // Assert
            AssertAccessManagerGrantResultsEqual(resultV1, resultV2);
        }

        [Test]
        public void AccessManagerGrant_HandleSubkeyLevel_BothVersionsHandleCorrectly()
        {
            // Arrange - subkey level grants
            var subkeyData = CreateSubkeyAccessManagerGrantData();

            // Act
            var resultV1 = jsonLibraryV1.DeserializeToObject<PNAccessManagerGrantResult>(subkeyData);
            var resultV2 = jsonLibraryV2.DeserializeToObject<PNAccessManagerGrantResult>(subkeyData);

            // Assert
            AssertAccessManagerGrantResultsEqual(resultV1, resultV2);
            Assert.AreEqual("subkey", resultV1.Level);
        }

        #endregion

        #region Test Data Creation Methods

        private List<object> CreateSampleUuidMetadataListData()
        {
            return new List<object>
            {
                "success",
                new Dictionary<string, object>
                {
                    ["data"] = new Dictionary<string, object>
                    {
                        ["id"] = "test-uuid-123",
                        ["name"] = "Test User",
                        ["email"] = "test@example.com",
                        ["externalId"] = "ext-123",
                        ["profileUrl"] = "https://example.com/profile.jpg",
                        ["status"] = "active",
                        ["type"] = "user",
                        ["updated"] = "2023-12-01T12:00:00Z",
                        ["custom"] = new Dictionary<string, object>
                        {
                            ["department"] = "engineering",
                            ["level"] = "senior"
                        }
                    }
                }
            };
        }

        private List<object> CreateSampleChannelMetadataListData()
        {
            return new List<object>
            {
                "success",
                new Dictionary<string, object>
                {
                    ["data"] = new Dictionary<string, object>
                    {
                        ["id"] = "test-channel-456",
                        ["name"] = "Test Channel",
                        ["description"] = "A test channel for demonstrations",
                        ["status"] = "active",
                        ["type"] = "public",
                        ["updated"] = "2023-12-01T12:00:00Z",
                        ["custom"] = new Dictionary<string, object>
                        {
                            ["category"] = "general",
                            ["priority"] = "high"
                        }
                    }
                }
            };
        }

        private List<object> CreateComplexUuidMetadata()
        {
            return new List<object>
            {
                "success",
                new Dictionary<string, object>
                {
                    ["data"] = new Dictionary<string, object>
                    {
                        ["id"] = "complex-uuid-789",
                        ["name"] = "Complex User",
                        ["email"] = "complex@example.com",
                        ["externalId"] = "ext-complex-789",
                        ["profileUrl"] = "https://example.com/complex-profile.jpg",
                        ["status"] = "active",
                        ["type"] = "premium-user",
                        ["updated"] = "2023-12-01T15:30:00Z",
                        ["custom"] = new Dictionary<string, object>
                        {
                            ["department"] = "research",
                            ["level"] = "principal",
                            ["permissions"] = new[] { "read", "write", "admin" },
                            ["metadata"] = new Dictionary<string, object>
                            {
                                ["created"] = "2023-01-01",
                                ["lastLogin"] = "2023-12-01T10:00:00Z",
                                ["preferences"] = new Dictionary<string, object>
                                {
                                    ["theme"] = "dark",
                                    ["notifications"] = true
                                }
                            }
                        }
                    }
                }
            };
        }

        private List<object> CreateComplexChannelMetadata()
        {
            return new List<object>
            {
                "success",
                new Dictionary<string, object>
                {
                    ["data"] = new Dictionary<string, object>
                    {
                        ["id"] = "complex-channel-789",
                        ["name"] = "Complex Channel",
                        ["description"] = "A complex channel with extensive metadata for comprehensive testing",
                        ["status"] = "active",
                        ["type"] = "premium-public",
                        ["updated"] = "2023-12-01T15:30:00Z",
                        ["custom"] = new Dictionary<string, object>
                        {
                            ["category"] = "enterprise",
                            ["priority"] = "critical",
                            ["tags"] = new[] { "production", "high-traffic", "monitored" },
                            ["settings"] = new Dictionary<string, object>
                            {
                                ["maxMembers"] = 1000,
                                ["allowGuests"] = false,
                                ["features"] = new Dictionary<string, object>
                                {
                                    ["fileSharing"] = true,
                                    ["videoConference"] = true,
                                    ["encryption"] = "AES-256"
                                }
                            }
                        }
                    }
                }
            };
        }

        private List<object> CreateSampleHistoryListData()
        {
            return new List<object>
            {
                // Messages array
                new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["message"] = "Hello World",
                        ["timetoken"] = "15635547929785639",
                        ["uuid"] = "test-sender",
                        ["meta"] = new Dictionary<string, object>
                        {
                            ["type"] = "text"
                        },
                        ["message_type"] = 0
                    },
                    new Dictionary<string, object>
                    {
                        ["message"] = new Dictionary<string, object>
                        {
                            ["text"] = "Structured message",
                            ["data"] = "Additional info"
                        },
                        ["timetoken"] = "15635547929785640"
                    },
                    // Simple message format
                    "Simple string message"
                },
                // Start timetoken
                "15635547929785639",
                // End timetoken  
                "15635547929785641"
            };
        }

        private List<object> CreateMixedFormatHistoryData()
        {
            return new List<object>
            {
                new List<object>
                {
                    // Structured message with all fields
                    new Dictionary<string, object>
                    {
                        ["message"] = "Full structured message",
                        ["timetoken"] = "15635547929785639",
                        ["uuid"] = "sender1",
                        ["meta"] = new Dictionary<string, object>
                        {
                            ["type"] = "announcement",
                            ["priority"] = "high"
                        },
                        ["message_type"] = 1
                    },
                    // Simple dictionary message (not structured format)
                    new Dictionary<string, object>
                    {
                        ["content"] = "Dictionary message",
                        ["timestamp"] = "2023-12-01"
                    },
                    // Raw string message
                    "Plain text message",
                    // Structured message with minimal fields
                    new Dictionary<string, object>
                    {
                        ["message"] = "Minimal structured",
                        ["meta"] = new Dictionary<string, object>
                        {
                            ["source"] = "mobile"
                        }
                    }
                },
                "15635547929785639",
                "15635547929785645"
            };
        }

        private List<object> CreateSampleAccessManagerGrantListData()
        {
            return new List<object>
            {
                new Dictionary<string, object>
                {
                    ["payload"] = new Dictionary<string, object>
                    {
                        ["level"] = "channel",
                        ["subscribe_key"] = "sub-c-test",
                        ["ttl"] = 1440,
                        ["channels"] = new Dictionary<string, object>
                        {
                            ["channel1"] = new Dictionary<string, object>
                            {
                                ["auths"] = new Dictionary<string, object>
                                {
                                    ["auth1"] = new Dictionary<string, object>
                                    {
                                        ["r"] = "1",
                                        ["w"] = "1",
                                        ["m"] = "0",
                                        ["d"] = "0"
                                    },
                                    ["auth2"] = new Dictionary<string, object>
                                    {
                                        ["r"] = "1",
                                        ["w"] = "0",
                                        ["m"] = "0",
                                        ["d"] = "0"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private List<object> CreateComplexAccessManagerGrantData()
        {
            return new List<object>
            {
                new Dictionary<string, object>
                {
                    ["payload"] = new Dictionary<string, object>
                    {
                        ["level"] = "app",
                        ["subscribe_key"] = "sub-c-test",
                        ["ttl"] = 720,
                        ["channels"] = new Dictionary<string, object>
                        {
                            ["channel1"] = new Dictionary<string, object>
                            {
                                ["auths"] = new Dictionary<string, object>
                                {
                                    ["auth1"] = new Dictionary<string, object>
                                    {
                                        ["r"] = "1", ["w"] = "1", ["m"] = "1", ["d"] = "0", ["g"] = "1", ["u"] = "1", ["j"] = "0"
                                    }
                                }
                            },
                            ["channel2"] = new Dictionary<string, object>
                            {
                                ["auths"] = new Dictionary<string, object>
                                {
                                    ["auth2"] = new Dictionary<string, object>
                                    {
                                        ["r"] = "1", ["w"] = "0", ["m"] = "0", ["d"] = "0", ["g"] = "1", ["u"] = "0", ["j"] = "0"
                                    }
                                }
                            }
                        },
                        ["channel-groups"] = new Dictionary<string, object>
                        {
                            ["cg1"] = new Dictionary<string, object>
                            {
                                ["auths"] = new Dictionary<string, object>
                                {
                                    ["auth1"] = new Dictionary<string, object>
                                    {
                                        ["r"] = "1", ["w"] = "1", ["m"] = "1", ["d"] = "1", ["g"] = "0", ["u"] = "0", ["j"] = "0"
                                    }
                                }
                            }
                        },
                        ["uuids"] = new Dictionary<string, object>
                        {
                            ["uuid1"] = new Dictionary<string, object>
                            {
                                ["auths"] = new Dictionary<string, object>
                                {
                                    ["auth1"] = new Dictionary<string, object>
                                    {
                                        ["r"] = "1", ["w"] = "0", ["m"] = "0", ["d"] = "0", ["g"] = "1", ["u"] = "1", ["j"] = "1"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private List<object> CreateSingleItemAccessManagerGrantData()
        {
            return new List<object>
            {
                new Dictionary<string, object>
                {
                    ["payload"] = new Dictionary<string, object>
                    {
                        ["level"] = "channel",
                        ["subscribe_key"] = "sub-c-test",
                        ["ttl"] = 60,
                        ["channel"] = "single-channel",
                        ["auths"] = new Dictionary<string, object>
                        {
                            ["single-auth"] = new Dictionary<string, object>
                            {
                                ["r"] = "1",
                                ["w"] = "1",
                                ["m"] = "0",
                                ["d"] = "0"
                            }
                        }
                    }
                }
            };
        }

        private List<object> CreateSubkeyAccessManagerGrantData()
        {
            return new List<object>
            {
                new Dictionary<string, object>
                {
                    ["payload"] = new Dictionary<string, object>
                    {
                        ["level"] = "subkey",
                        ["subscribe_key"] = "sub-c-test",
                        ["ttl"] = 0
                    }
                }
            };
        }

        #endregion

        #region Assertion Helper Methods

        private void AssertUuidMetadataResultsEqual(PNSetUuidMetadataResult expected, PNSetUuidMetadataResult actual)
        {
            // Check for null objects first
            if (expected == null && actual == null) return;
            
            Assert.IsNotNull(expected, "Expected result is null");
            Assert.IsNotNull(actual, "Actual result is null");
            
            Assert.AreEqual(expected.Uuid, actual.Uuid, "Uuid mismatch");
            Assert.AreEqual(expected.Name, actual.Name, "Name mismatch");
            Assert.AreEqual(expected.Email, actual.Email, "Email mismatch");
            Assert.AreEqual(expected.ExternalId, actual.ExternalId, "ExternalId mismatch");
            Assert.AreEqual(expected.ProfileUrl, actual.ProfileUrl, "ProfileUrl mismatch");
            Assert.AreEqual(expected.Status, actual.Status, "Status mismatch");
            Assert.AreEqual(expected.Type, actual.Type, "Type mismatch");
            
            // Handle differences between empty string and null
            var expectedUpdated = expected.Updated ?? string.Empty;
            var actualUpdated = actual.Updated ?? string.Empty;
            Assert.AreEqual(expectedUpdated, actualUpdated, "Updated mismatch");
            
            AssertDictionariesEqual(expected.Custom, actual.Custom);
        }

        private void AssertChannelMetadataResultsEqual(PNSetChannelMetadataResult expected, PNSetChannelMetadataResult actual)
        {
            // Check for null objects first
            if (expected == null && actual == null) return;
            
            Assert.IsNotNull(expected, "Expected result is null");
            Assert.IsNotNull(actual, "Actual result is null");
            
            Assert.AreEqual(expected.Channel, actual.Channel, "Channel mismatch");
            Assert.AreEqual(expected.Name, actual.Name, "Name mismatch");
            Assert.AreEqual(expected.Description, actual.Description, "Description mismatch");
            Assert.AreEqual(expected.Status, actual.Status, "Status mismatch");
            Assert.AreEqual(expected.Type, actual.Type, "Type mismatch");
            
            // Handle differences between empty string and null
            var expectedUpdated = expected.Updated ?? string.Empty;
            var actualUpdated = actual.Updated ?? string.Empty;
            Assert.AreEqual(expectedUpdated, actualUpdated, "Updated mismatch");
            
            AssertDictionariesEqual(expected.Custom, actual.Custom);
        }

        private void AssertHistoryResultsEqual(PNHistoryResult expected, PNHistoryResult actual)
        {
            // Check for null objects first
            if (expected == null && actual == null) return;

            Assert.IsNotNull(expected, "Expected history result is null");
            Assert.IsNotNull(actual, "Actual history result is null");

            Assert.AreEqual(expected.StartTimeToken, actual.StartTimeToken, "StartTimeToken mismatch");
            Assert.AreEqual(expected.EndTimeToken, actual.EndTimeToken, "EndTimeToken mismatch");

            // Handle null message collections
            if (expected.Messages == null && actual.Messages == null) return;

            Assert.IsNotNull(expected.Messages, "Expected messages is null");
            Assert.IsNotNull(actual.Messages, "Actual messages is null");
            Assert.AreEqual(expected.Messages.Count, actual.Messages.Count, "Message count mismatch");

            for (int i = 0; i < expected.Messages.Count; i++)
            {
                AssertHistoryItemResultsEqual(expected.Messages[i], actual.Messages[i], $"Message[{i}]");
            }
        }

        private void AssertHistoryItemResultsEqual(PNHistoryItemResult expected, PNHistoryItemResult actual, string context)
        {
            Assert.IsNotNull(expected, $"{context} expected item is null");
            Assert.IsNotNull(actual, $"{context} actual item is null");

            Assert.AreEqual(expected.Timetoken, actual.Timetoken, $"{context} timetoken mismatch");
            Assert.AreEqual(expected.Uuid, actual.Uuid, $"{context} uuid mismatch");
            Assert.AreEqual(expected.MessageType, actual.MessageType, $"{context} message type mismatch");

            // Compare entry content - can be complex objects
            AssertObjectsEqual(expected.Entry, actual.Entry, $"{context} entry");

            // Compare meta dictionaries
            AssertDictionariesEqual(expected.Meta, actual.Meta);
        }

        private void AssertAccessManagerGrantResultsEqual(PNAccessManagerGrantResult expected, PNAccessManagerGrantResult actual)
        {
            // Check for null objects first
            if (expected == null && actual == null) return;

            Assert.IsNotNull(expected, "Expected grant result is null");
            Assert.IsNotNull(actual, "Actual grant result is null");

            Assert.AreEqual(expected.Level, actual.Level, "Level mismatch");
            Assert.AreEqual(expected.SubscribeKey, actual.SubscribeKey, "SubscribeKey mismatch");
            Assert.AreEqual(expected.Ttl, actual.Ttl, "Ttl mismatch");

            // Compare channels
            AssertAccessManagerSectionsEqual(expected.Channels, actual.Channels, "Channels");

            // Compare channel groups
            AssertAccessManagerSectionsEqual(expected.ChannelGroups, actual.ChannelGroups, "ChannelGroups");

            // Compare UUIDs
            AssertAccessManagerSectionsEqual(expected.Uuids, actual.Uuids, "Uuids");
        }

        private void AssertAccessManagerSectionsEqual(
            Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> expected, 
            Dictionary<string, Dictionary<string, PNAccessManagerKeyData>> actual, 
            string sectionName)
        {
            if (expected == null && actual == null) return;

            Assert.IsNotNull(expected, $"Expected {sectionName} is null");
            Assert.IsNotNull(actual, $"Actual {sectionName} is null");
            Assert.AreEqual(expected.Count, actual.Count, $"{sectionName} count mismatch");

            foreach (var expectedItem in expected)
            {
                Assert.IsTrue(actual.ContainsKey(expectedItem.Key), $"{sectionName} missing key '{expectedItem.Key}'");

                var expectedAuths = expectedItem.Value;
                var actualAuths = actual[expectedItem.Key];

                Assert.IsNotNull(expectedAuths, $"{sectionName}['{expectedItem.Key}'] expected auths is null");
                Assert.IsNotNull(actualAuths, $"{sectionName}['{expectedItem.Key}'] actual auths is null");
                Assert.AreEqual(expectedAuths.Count, actualAuths.Count, $"{sectionName}['{expectedItem.Key}'] auth count mismatch");

                foreach (var expectedAuth in expectedAuths)
                {
                    Assert.IsTrue(actualAuths.ContainsKey(expectedAuth.Key), $"{sectionName}['{expectedItem.Key}'] missing auth key '{expectedAuth.Key}'");
                    
                    AssertAccessManagerKeyDataEqual(expectedAuth.Value, actualAuths[expectedAuth.Key], 
                        $"{sectionName}['{expectedItem.Key}']['{expectedAuth.Key}']");
                }
            }
        }

        private void AssertAccessManagerKeyDataEqual(PNAccessManagerKeyData expected, PNAccessManagerKeyData actual, string context)
        {
            Assert.IsNotNull(expected, $"{context} expected key data is null");
            Assert.IsNotNull(actual, $"{context} actual key data is null");

            Assert.AreEqual(expected.ReadEnabled, actual.ReadEnabled, $"{context} ReadEnabled mismatch");
            Assert.AreEqual(expected.WriteEnabled, actual.WriteEnabled, $"{context} WriteEnabled mismatch");
            Assert.AreEqual(expected.ManageEnabled, actual.ManageEnabled, $"{context} ManageEnabled mismatch");
            Assert.AreEqual(expected.DeleteEnabled, actual.DeleteEnabled, $"{context} DeleteEnabled mismatch");
            Assert.AreEqual(expected.GetEnabled, actual.GetEnabled, $"{context} GetEnabled mismatch");
            Assert.AreEqual(expected.UpdateEnabled, actual.UpdateEnabled, $"{context} UpdateEnabled mismatch");
            Assert.AreEqual(expected.JoinEnabled, actual.JoinEnabled, $"{context} JoinEnabled mismatch");
        }

        private void AssertObjectsEqual(object expected, object actual, string context)
        {
            if (expected == null && actual == null) return;

            if (expected == null || actual == null)
            {
                Assert.AreEqual(expected, actual, $"{context} null mismatch");
                return;
            }

            // Handle dictionary objects
            if (expected is Dictionary<string, object> expectedDict && actual is Dictionary<string, object> actualDict)
            {
                AssertDictionariesEqual(expectedDict, actualDict);
                return;
            }

            // For other objects, use basic equality
            Assert.AreEqual(expected.ToString(), actual.ToString(), $"{context} content mismatch");
        }

        private void AssertDictionariesEqual(Dictionary<string, object> expected, Dictionary<string, object> actual)
        {
            if (expected == null && actual == null) return;
            
            Assert.IsNotNull(expected, "Expected dictionary is null");
            Assert.IsNotNull(actual, "Actual dictionary is null");
            Assert.AreEqual(expected.Count, actual.Count, "Dictionary count mismatch");

            foreach (var kvp in expected)
            {
                Assert.IsTrue(actual.ContainsKey(kvp.Key), $"Missing key '{kvp.Key}'");
                Assert.AreEqual(kvp.Value, actual[kvp.Key], $"Value mismatch for key '{kvp.Key}'");
            }
        }

        #endregion
    }
}
