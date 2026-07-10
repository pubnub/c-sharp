using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;

namespace PubNubMessaging.Tests
{
    /// <summary>
    /// Real-network integration tests that point the SDK at the HTTP/2-capable origin
    /// "h2.pubnubapi.com" and verify HTTP/2 is actually negotiated.
    ///
    /// Observation is done with a pass-through <see cref="DelegatingHandler"/> wrapping a real
    /// <see cref="SocketsHttpHandler"/>, injected via the public
    /// HttpClientService(HttpMessageHandler, bool) constructor; each captured response is asserted
    /// to have negotiated HTTP/2 (response.Version.Major == 2).
    /// </summary>
    [TestFixture]
    public class WhenHttp2OriginIsUsed
    {
        private const string Http2Origin = "h2.pubnubapi.com";
        private const string RegularOrigin = "ps.pndsn.com";
        private static readonly int ConnectWaitTimeoutMs = 15 * 1000;

        private Pubnub pubnub;

        /// <summary>
        /// Records each request/response protocol version while passing the call through to the
        /// real network via the inner <see cref="SocketsHttpHandler"/>.
        /// </summary>
        private sealed class RecordingPassThroughHandler : DelegatingHandler
        {
            public RecordingPassThroughHandler() : base(new SocketsHttpHandler())
            {
            }

            public List<(Uri Url, Version RequestVersion, Version ResponseVersion)> Calls { get; } =
                new List<(Uri, Version, Version)>();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                lock (Calls)
                {
                    Calls.Add((request.RequestUri, request.Version, response.Version));
                }
                return response;
            }
        }

        private Pubnub CreateHttp2Pubnub(RecordingPassThroughHandler handler, string origin = Http2Origin)
        {
            var config = new PNConfiguration(new UserId("http2-itest-uuid"))
            {
                PublishKey = PubnubCommon.NonPAMPublishKey,
                SubscribeKey = PubnubCommon.NONPAMSubscribeKey,
                Origin = origin,
                Secure = true, // HTTP/2 requires TLS/ALPN
                EnableHttp2 = true,
                LogLevel = PubnubLogLevel.Debug
            };
            var http = new HttpClientService(handler, enableHttp2: true);
            return new Pubnub(config, httpTransportService: http);
        }

        private static void AssertNegotiatedHttp2(RecordingPassThroughHandler handler, string pathFragment)
        {
            List<(Uri Url, Version RequestVersion, Version ResponseVersion)> match;
            lock (handler.Calls)
            {
                match = handler.Calls.FindAll(c => c.Url.AbsolutePath.Contains(pathFragment));
            }

            Assert.IsNotEmpty(match, $"No request captured for '{pathFragment}'");
            foreach (var c in match)
            {
                Assert.AreEqual(2, c.ResponseVersion.Major,
                    $"Expected HTTP/2 for {c.Url.AbsolutePath}, got HTTP/{c.ResponseVersion}");
                Assert.AreEqual(new Version(2, 0), c.RequestVersion,
                    $"Expected request to opt into HTTP/2 for {c.Url.AbsolutePath}, got HTTP/{c.RequestVersion}");
            }
        }

        private static void AssertNegotiatedHttp11(RecordingPassThroughHandler handler, string pathFragment)
        {
            List<(Uri Url, Version RequestVersion, Version ResponseVersion)> match;
            lock (handler.Calls)
            {
                match = handler.Calls.FindAll(c => c.Url.AbsolutePath.Contains(pathFragment));
            }

            Assert.IsNotEmpty(match, $"No request captured for '{pathFragment}'");
            foreach (var c in match)
            {
                // The request opts into HTTP/2 (RequestVersionOrLower), but a non-HTTP/2 origin
                // must transparently fall back to HTTP/1.1.
                Assert.AreEqual(1, c.ResponseVersion.Major,
                    $"Expected HTTP/1.1 fallback for {c.Url.AbsolutePath}, got HTTP/{c.ResponseVersion}");
            }
        }

        [TearDown]
        public void Exit()
        {
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub = null;
            }
        }

        [Test]
        public async Task ThenTimeRequestNegotiatesHttp2()
        {
            var handler = new RecordingPassThroughHandler();
            pubnub = CreateHttp2Pubnub(handler);

            var timeResult = await pubnub.Time().ExecuteAsync();

            Assert.IsNotNull(timeResult, "Time() returned null");
            Assert.IsFalse(timeResult.Status.Error, "Time() request failed");
            AssertNegotiatedHttp2(handler, "/time");
        }

        [Test]
        public async Task ThenPublishNegotiatesHttp2()
        {
            var handler = new RecordingPassThroughHandler();
            pubnub = CreateHttp2Pubnub(handler);

            var publishResult = await pubnub.Publish()
                .Channel("http2_test")
                .Message("hello-h2")
                .ExecuteAsync();

            Assert.IsNotNull(publishResult, "Publish() returned null");
            Assert.IsFalse(publishResult.Status.Error, "Publish() request failed");
            AssertNegotiatedHttp2(handler, "/publish");
        }

        [Test]
        public async Task ThenNonHttp2OriginFallsBackToHttp11()
        {
            // EnableHttp2 is on, but the regular origin does not support HTTP/2; the request
            // must still succeed by transparently falling back to HTTP/1.1.
            var handler = new RecordingPassThroughHandler();
            pubnub = CreateHttp2Pubnub(handler, RegularOrigin);

            var timeResult = await pubnub.Time().ExecuteAsync();

            Assert.IsNotNull(timeResult, "Time() returned null");
            Assert.IsFalse(timeResult.Status.Error, "Time() request failed against the regular origin");
            AssertNegotiatedHttp11(handler, "/time");
        }

        [Test]
        public void ThenSubscribeLoopNegotiatesHttp2()
        {
            var handler = new RecordingPassThroughHandler();
            pubnub = CreateHttp2Pubnub(handler);

            string channel = "http2_subscribe_test";
            var connectedEvent = new ManualResetEvent(false);

            var listener = new SubscribeCallbackExt(
                (_, _) => { },
                (_, _) => { },
                (_, status) =>
                {
                    if (status.StatusCode == 200 && status.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        connectedEvent.Set();
                    }
                });
            pubnub.AddListener(listener);

            pubnub.Subscribe<string>().Channels(new[] { channel }).Execute();

            bool connected = connectedEvent.WaitOne(ConnectWaitTimeoutMs);
            Assert.IsTrue(connected, "Subscribe did not reach connected status in time");

            AssertNegotiatedHttp2(handler, "/subscribe");

            pubnub.Unsubscribe<string>().Channels(new[] { channel }).Execute();
        }
    }
}
