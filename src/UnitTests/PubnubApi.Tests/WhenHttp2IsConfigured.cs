using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;

namespace PubNubMessaging.Tests
{
    /// <summary>
    /// Offline tests for HTTP/2 request shaping, protocol/operation logging, HTTP/1.1 fallback,
    /// and protocol re-negotiation. These inject a custom HttpMessageHandler into HttpClientService
    /// so no real network I/O occurs. The custom MockServer is intentionally NOT used because it
    /// cannot negotiate HTTP/2.
    /// </summary>
    [TestFixture]
    public class WhenHttp2IsConfigured
    {
        /// <summary>
        /// Records the last outgoing request and returns a response whose protocol version is
        /// taken from a caller-supplied queue (falls back to the last value once drained).
        /// </summary>
        private class RecordingHandler : HttpMessageHandler
        {
            private readonly Queue<Version> responseVersions;
            private Version lastResponseVersion = new Version(1, 1);

            public HttpRequestMessage LastRequest { get; private set; }
            public Version LastRequestVersion { get; private set; }
            public int CallCount { get; private set; }

            public RecordingHandler(params Version[] versions)
            {
                responseVersions = new Queue<Version>(versions ?? Array.Empty<Version>());
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                LastRequest = request;
                LastRequestVersion = request.Version;

                if (responseVersions.Count > 0)
                {
                    lastResponseVersion = responseVersions.Dequeue();
                }

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Version = lastResponseVersion,
                    Content = new StringContent("[]"),
                    RequestMessage = request
                };
                return Task.FromResult(response);
            }
        }

        private class CapturingLogger : IPubnubLogger
        {
            public List<string> Messages { get; } = new List<string>();
            public void Trace(string logMessage) => Messages.Add(logMessage);
            public void Debug(string logMessage) => Messages.Add(logMessage);
            public void Info(string logMessage) => Messages.Add(logMessage);
            public void Warn(string logMessage) => Messages.Add(logMessage);
            public void Error(string logMessage) => Messages.Add(logMessage);
        }

        private static TransportRequest CreateTransportRequest(string method, PNOperationType operationType)
        {
            return new TransportRequest
            {
                RequestType = method,
                OperationType = operationType,
                RequestUrl = "https://ps.pndsn.com/time/0",
                BodyContentString = method is "POST" or "PUT" or "PATCH" ? "{}" : null,
                CancellationTokenSource = new CancellationTokenSource()
            };
        }

        private static Task<TransportResponse> Dispatch(HttpClientService service, TransportRequest request)
        {
            return request.RequestType switch
            {
                "POST" => service.PostRequest(request),
                "PUT" => service.PutRequest(request),
                "PATCH" => service.PatchRequest(request),
                "DELETE" => service.DeleteRequest(request),
                _ => service.GetRequest(request)
            };
        }

        // A. Request shaping
        [TestCase("GET", PNOperationType.PNSubscribeOperation)]
        [TestCase("POST", PNOperationType.PNPublishOperation)]
        [TestCase("PUT", PNOperationType.PNSetUuidMetadataOperation)]
        [TestCase("PATCH", PNOperationType.PNSetUuidMetadataOperation)]
        [TestCase("DELETE", PNOperationType.PNDeleteMessageOperation)]
        public async Task WhenHttp2EnabledRequestUsesHttp2(string method, PNOperationType operationType)
        {
            var handler = new RecordingHandler(new Version(2, 0));
            var service = new HttpClientService(handler, enableHttp2: true);

            await Dispatch(service, CreateTransportRequest(method, operationType));

            Assert.AreEqual(new Version(2, 0), handler.LastRequestVersion, $"{method} should request HTTP/2");
#if NET6_0_OR_GREATER
            Assert.AreEqual(HttpVersionPolicy.RequestVersionOrLower, handler.LastRequest.VersionPolicy,
                $"{method} should allow HTTP/1.1 fallback");
#endif
        }

        [TestCase("GET")]
        [TestCase("POST")]
        [TestCase("PUT")]
        [TestCase("PATCH")]
        [TestCase("DELETE")]
        public async Task WhenHttp2DisabledRequestUsesHttp11(string method)
        {
            var handler = new RecordingHandler(new Version(1, 1));
            var service = new HttpClientService(handler, enableHttp2: false);

            await Dispatch(service, CreateTransportRequest(method, PNOperationType.PNSubscribeOperation));

            Assert.AreEqual(new Version(1, 1), handler.LastRequestVersion, $"{method} should remain HTTP/1.1 when disabled");
        }

        // B. Protocol + operation logging
        [Test]
        public async Task ThenLogsNegotiatedHttp2ProtocolAndOperation()
        {
            var handler = new RecordingHandler(new Version(2, 0));
            var service = new HttpClientService(handler, enableHttp2: true);
            var logger = new CapturingLogger();
            service.SetLogger(new PubnubLogModule(PubnubLogLevel.Debug, new List<IPubnubLogger> { logger }));

            await Dispatch(service, CreateTransportRequest("GET", PNOperationType.PNSubscribeOperation));

            Assert.IsTrue(logger.Messages.Exists(m =>
                    m.Contains("operation=PNSubscribeOperation") && m.Contains("protocol=HTTP/2.0")),
                "Expected a completion log with operation and HTTP/2.0 protocol");
        }

        [Test]
        public async Task ThenLogsNegotiatedHttp11ProtocolOnFallback()
        {
            var handler = new RecordingHandler(new Version(1, 1));
            var service = new HttpClientService(handler, enableHttp2: true);
            var logger = new CapturingLogger();
            service.SetLogger(new PubnubLogModule(PubnubLogLevel.Debug, new List<IPubnubLogger> { logger }));

            await Dispatch(service, CreateTransportRequest("POST", PNOperationType.PNPublishOperation));

            Assert.IsTrue(logger.Messages.Exists(m =>
                    m.Contains("operation=PNPublishOperation") && m.Contains("protocol=HTTP/1.1")),
                "Expected a completion log with operation and HTTP/1.1 protocol");
        }

        // C. Fallback still succeeds
        [Test]
        public async Task WhenServerRespondsHttp11RequestStillSucceeds()
        {
            // Request asks for HTTP/2 but the (simulated) server answers with HTTP/1.1.
            var handler = new RecordingHandler(new Version(1, 1));
            var service = new HttpClientService(handler, enableHttp2: true);

            var response = await Dispatch(service, CreateTransportRequest("GET", PNOperationType.PNTimeOperation));

            Assert.AreEqual(new Version(2, 0), handler.LastRequestVersion, "Request should still attempt HTTP/2");
            Assert.IsNull(response.Error, "Request should succeed despite HTTP/1.1 downgrade");
            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual(new Version(1, 1), response.NegotiatedProtocolVersion, "Negotiated protocol should be HTTP/1.1");
        }

        // D. Re-negotiation / no pinning
        [Test]
        public async Task ThenProtocolCanChangeAcrossRequestsWithoutPinning()
        {
            // Simulates HTTP/2.0 -> reconnect/recreation -> HTTP/1.1 -> HTTP/2.0
            var handler = new RecordingHandler(new Version(2, 0), new Version(1, 1), new Version(2, 0));
            var service = new HttpClientService(handler, enableHttp2: true);

            var first = await Dispatch(service, CreateTransportRequest("GET", PNOperationType.PNSubscribeOperation));
            var second = await Dispatch(service, CreateTransportRequest("GET", PNOperationType.PNSubscribeOperation));
            var third = await Dispatch(service, CreateTransportRequest("GET", PNOperationType.PNSubscribeOperation));

            Assert.AreEqual(new Version(2, 0), first.NegotiatedProtocolVersion);
            Assert.AreEqual(new Version(1, 1), second.NegotiatedProtocolVersion);
            Assert.AreEqual(new Version(2, 0), third.NegotiatedProtocolVersion);
            Assert.AreEqual(3, handler.CallCount, "No extra probe requests should be issued");
        }

        // E. Config default
        [Test]
        public void ThenEnableHttp2DefaultsToTrue()
        {
            var config = new PNConfiguration(new UserId("http2-default-test"));
            Assert.IsTrue(config.EnableHttp2, "EnableHttp2 should default to true");
        }
    }
}
