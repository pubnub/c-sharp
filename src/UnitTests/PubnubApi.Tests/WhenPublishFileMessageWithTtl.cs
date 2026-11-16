using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenPublishFileMessageWithTtl
    {
        private WireMockServer _mockServer;
        private Pubnub _pubnub;

        [SetUp]
        public void Setup()
        {
            _mockServer = WireMockServer.Start();
            _mockServer
                .Given(Request.Create().WithPath("/v1/files/publish-file/*").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200).WithBody("[1,\"Sent\",\"17000000000000000\"]"));

            var config = new PNConfiguration(new UserId("csharp"))
            {
                SubscribeKey = "demo",
                PublishKey = "demo",
                Origin = $"localhost:{_mockServer.Port}",
                Secure = false
            };
            _pubnub = new Pubnub(config);
        }

        [TearDown]
        public void TearDown()
        {
            _pubnub?.Destroy();
            _mockServer?.Stop();
            _mockServer?.Dispose();
        }

        [Test]
        public async Task ThenTtlIsSentAsTtlQueryParameter()
        {
            var result = await _pubnub.PublishFileMessage()
                .Channel("ch")
                .FileId("id1")
                .FileName("a.txt")
                .Message("hello")
                .Ttl(1)
                .ExecuteAsync();

            Assert.AreEqual(200, result.Status.StatusCode);
            var request = _mockServer.LogEntries.Single().RequestMessage;
            Assert.IsTrue(request.Query.ContainsKey("ttl"), "query was: " + request.RawQuery);
            Assert.AreEqual("1", request.Query["ttl"].Single());
            Assert.IsFalse(request.Query.ContainsKey("tt1"));
        }
    }
}
