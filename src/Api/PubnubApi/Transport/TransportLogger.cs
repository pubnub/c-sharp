using System;
using System.Linq;

namespace PubnubApi
{
    /// <summary>
    /// Owns all transport-layer log message construction so that logging concerns stay
    /// out of <see cref="HttpClientService"/>. All messages share the "HttpClient Service"
    /// prefix and route network blocks through <see cref="PubnubLogFormatter"/>.
    /// </summary>
    internal sealed class TransportLogger
    {
        private const string Prefix = "HttpClient Service";
        private readonly PubnubLogModule logger;

        public TransportLogger(PubnubLogModule logger)
        {
            this.logger = logger;
        }

        // Headers and bodies may contain message content, tokens, or PII, so they are only
        // logged at Trace level (matching the headers handling across PubNub SDKs).
        private bool TraceEnabled => logger != null && logger.MinLogLevel <= PubnubLogLevel.Trace;

        public void Request(TransportRequest transportRequest)
        {
            logger?.Debug(() =>
            {
                bool trace = TraceEnabled;
                int bodySize = transportRequest.BodyContentBytes?.Length
                    ?? (transportRequest.BodyContentString != null
                        ? System.Text.Encoding.UTF8.GetByteCount(transportRequest.BodyContentString)
                        : 0);
                return $"{Prefix} " + PubnubLogFormatter.HttpRequest(
                    transportRequest.RequestType, transportRequest.RequestUrl, transportRequest.Headers,
                    bodySize, trace);
            });
        }

        public void Response(TransportRequest transportRequest, TransportResponse transportResponse,
            Version protocolVersion)
        {
            logger?.Debug(() =>
            {
                bool trace = TraceEnabled;
                int bodySize = transportResponse.Content?.Length ?? 0;
                var headers = trace
                    ? transportResponse.Headers?.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
                    : null;
                return
                    $"{Prefix} operation={transportRequest.OperationType} protocol=HTTP/{protocolVersion} " +
                    PubnubLogFormatter.HttpResponse(transportRequest.RequestUrl, transportResponse.StatusCode,
                        headers, bodySize, trace);
            });
        }

        public void Exception(TransportRequest transportRequest, Exception exception)
        {
            logger?.Warn(() =>
                $"{Prefix} Exception for http call url {transportRequest.RequestUrl}, exception message: {exception.Message}, stacktrace: {exception.StackTrace}");
        }

        public void TaskCanceled(TransportRequest transportRequest)
        {
            logger?.Warn($"{Prefix} TaskCanceledException for url {transportRequest.RequestUrl}");
        }

        public void CanceledByTimeout()
        {
            logger?.Debug($"{Prefix} Task canceled due to timeout");
        }

        public void CanceledByRequest()
        {
            logger?.Debug($"{Prefix} Task canceled due to cancellation request");
        }
    }
}
