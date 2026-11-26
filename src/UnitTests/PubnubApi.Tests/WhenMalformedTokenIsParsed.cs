using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PubnubApi;

namespace PubNubMessaging.Tests
{
    /// <summary>
    /// Offline tests for ParseToken with crafted CBOR payloads: nesting limits, declared lengths that exceed
    /// the token, indefinite-length items, floats and simple values.
    /// </summary>
    [TestFixture]
    public class WhenMalformedTokenIsParsed
    {
        private const string UuidKey = "64 75756964";
        private const string TimestampKey = "61 74";
        private const string MetaKey = "64 6D657461";

        private Pubnub pubnub;

        [SetUp]
        public void Init()
        {
            pubnub = new Pubnub(new PNConfiguration(new UserId("malformed-token-uuid"))
            {
                SubscribeKey = PubnubCommon.SubscribeKey,
                PublishKey = PubnubCommon.PublishKey,
            });
        }

        [TearDown]
        public void Cleanup()
        {
            pubnub?.Destroy();
            pubnub = null;
        }

        private static byte[] Hex(string hex)
        {
            return Convert.FromHexString(hex.Replace(" ", ""));
        }

        private static string ToToken(params byte[][] parts)
        {
            return Convert.ToBase64String(parts.SelectMany(part => part).ToArray());
        }

        private static string NestedArraysInMeta(int arrayCount)
        {
            return ToToken(Hex($"A1 {MetaKey} A1 61 78"), Enumerable.Repeat((byte)0x81, arrayCount).ToArray(), new byte[] { 0x00 });
        }

        [Test]
        public void ThenDeeplyNestedTokenShouldReturnNullInsteadOfOverflowingStack()
        {
            Assert.That(pubnub.ParseToken(NestedArraysInMeta(100000)), Is.Null);
        }

        [Test]
        public void ThenNestingUpToSixtyFourContainersShouldBeParsed()
        {
            // Root map + meta map + 62 arrays = 64 containers.
            PNTokenContent content = pubnub.ParseToken(NestedArraysInMeta(62));

            Assert.That(content, Is.Not.Null);
            object current = content.Meta["x"];
            for (int i = 0; i < 62; i++)
            {
                Assert.That(current, Is.InstanceOf<List<object>>());
                current = ((List<object>)current)[0];
            }
            Assert.That(current, Is.EqualTo(0L));
        }

        [Test]
        public void ThenNestingBeyondSixtyFourContainersShouldReturnNull()
        {
            Assert.That(pubnub.ParseToken(NestedArraysInMeta(63)), Is.Null);
        }

        [TestCase("7A FFFFFFFF", TestName = "text string with 4-byte length 0xFFFFFFFF")]
        [TestCase("7B FFFFFFFFFFFFFFFF", TestName = "text string with 8-byte length 0xFFFFFFFFFFFFFFFF")]
        [TestCase("7B 0000000100000000", TestName = "text string with 8-byte length above Int32.MaxValue")]
        [TestCase("65 6162", TestName = "text string longer than the remaining data")]
        [TestCase("5A 7FFFFFFF", TestName = "byte string with 4-byte length Int32.MaxValue")]
        [TestCase("9A FFFFFFFF", TestName = "array with 4-byte count 0xFFFFFFFF")]
        [TestCase("9B FFFFFFFFFFFFFFFF", TestName = "array with 8-byte count 0xFFFFFFFFFFFFFFFF")]
        [TestCase("BA 00000002 61 61", TestName = "map with more entries than the remaining data")]
        public void ThenDeclaredLengthBeyondTokenShouldReturnNull(string uuidValue)
        {
            Assert.That(pubnub.ParseToken(ToToken(Hex($"A1 {UuidKey} {uuidValue}"))), Is.Null);
        }

        [TestCase("1C", TestName = "reserved additional information 28")]
        [TestCase("1F", TestName = "integer with indefinite length")]
        [TestCase("FF", TestName = "break outside of an indefinite-length item")]
        [TestCase("F8 13", TestName = "one-byte simple value below 32")]
        [TestCase("7F 61 61 41 62 FF", TestName = "indefinite text string with a byte string chunk")]
        [TestCase("7F 7F 61 61 FF FF", TestName = "indefinite text string with a nested indefinite chunk")]
        [TestCase("7F 61 61", TestName = "indefinite text string without break")]
        [TestCase("9F 01", TestName = "indefinite array without break")]
        public void ThenInvalidItemShouldReturnNull(string timestampValue)
        {
            Assert.That(pubnub.ParseToken(ToToken(Hex($"A1 {TimestampKey} {timestampValue}"))), Is.Null);
        }

        [Test]
        public void ThenIndefiniteLengthTextShouldBeConcatenatedWithoutChunkHeaders()
        {
            PNTokenContent content = pubnub.ParseToken(ToToken(Hex($"A1 {UuidKey} 7F 62 6162 62 6364 FF")));

            Assert.That(content, Is.Not.Null);
            Assert.That(content.AuthorizedUuid, Is.EqualTo("abcd"));
        }

        [Test]
        public void ThenIndefiniteLengthChunkContainingBreakByteShouldBeKept()
        {
            PNTokenContent content = pubnub.ParseToken(ToToken(Hex($"A1 {MetaKey} A1 61 62 5F 42 FF01 FF")));

            Assert.That(content, Is.Not.Null);
            Assert.That(content.Meta["b"], Is.EqualTo(new byte[] { 0xFF, 0x01 }));
        }

        [Test]
        public void ThenIndefiniteLengthArrayAndMapShouldBeParsed()
        {
            PNTokenContent content = pubnub.ParseToken(ToToken(Hex($"A1 {MetaKey} A2 61 61 9F 01 02 FF 61 6D BF 61 6B 03 FF")));

            Assert.That(content, Is.Not.Null);
            Assert.That(content.Meta["a"], Is.EqualTo(new List<object> { 1L, 2L }));
            Assert.That(content.Meta["m"], Is.EqualTo(new Dictionary<object, object> { { "k", 3L } }));
        }

        [Test]
        public void ThenFloatsShouldBeDecoded()
        {
            PNTokenContent content = pubnub.ParseToken(ToToken(Hex(
                $"A1 {MetaKey} A5 61 68 F9 3800 61 6E F9 7E00 61 69 F9 FC00 61 66 FA 3FC00000 61 64 FB BFF8000000000000")));

            Assert.That(content, Is.Not.Null);
            Assert.That(content.Meta["h"], Is.EqualTo(0.5));
            Assert.That(content.Meta["n"], Is.NaN);
            Assert.That(content.Meta["i"], Is.EqualTo(double.NegativeInfinity));
            Assert.That(content.Meta["f"], Is.EqualTo(1.5));
            Assert.That(content.Meta["d"], Is.EqualTo(-1.5));
        }

        [Test]
        public void ThenOneByteSimpleValueShouldBeDecodedAsNumber()
        {
            PNTokenContent content = pubnub.ParseToken(ToToken(Hex($"A1 {MetaKey} A1 61 73 F8 FF")));

            Assert.That(content, Is.Not.Null);
            Assert.That(content.Meta["s"], Is.EqualTo((byte)255));
        }

        [Test]
        public void ThenLargeIntegersShouldBeDecoded()
        {
            PNTokenContent content = pubnub.ParseToken(ToToken(Hex(
                $"A1 {MetaKey} A3 61 6D 1B 7FFFFFFFFFFFFFFF 61 75 1B FFFFFFFFFFFFFFFF 61 6E 3B 7FFFFFFFFFFFFFFF")));

            Assert.That(content, Is.Not.Null);
            Assert.That(content.Meta["m"], Is.EqualTo(long.MaxValue));
            Assert.That(content.Meta["u"], Is.EqualTo(ulong.MaxValue));
            Assert.That(content.Meta["n"], Is.EqualTo(long.MinValue));
        }

        [Test]
        public void ThenNegativeIntegerBelowInt64RangeShouldReturnNull()
        {
            Assert.That(pubnub.ParseToken(ToToken(Hex($"A1 {TimestampKey} 3B 8000000000000000"))), Is.Null);
        }

        [Test]
        [MaxTime(5000)]
        public void ThenLargeFlatArrayShouldBeParsedInLinearTime()
        {
            const int count = 1000000;
            byte[] countBytes = BitConverter.GetBytes(count);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(countBytes);
            }

            PNTokenContent content = pubnub.ParseToken(ToToken(Hex($"A1 {MetaKey} A1 61 61 9A"), countBytes, new byte[count]));

            Assert.That(content, Is.Not.Null);
            Assert.That(((List<object>)content.Meta["a"]).Count, Is.EqualTo(count));
        }
    }
}
