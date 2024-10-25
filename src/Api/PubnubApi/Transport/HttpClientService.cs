using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace PubnubApi
{
	public class HttpClientService : IHttpClientService
	{
		private readonly HttpClient httpClient;
		public HttpClientService(IWebProxy proxy = default)
		{
			httpClient = new HttpClient()
			{
				Timeout = Timeout.InfiniteTimeSpan
			};
			if (proxy == null) return;
			httpClient = new HttpClient(new HttpClientHandler()
			{
				Proxy = proxy,
				UseProxy = true
			});
			httpClient.Timeout = Timeout.InfiniteTimeSpan;
		}

		public async Task<TransportResponse> GetRequest(TransportRequest transportRequest)
		{
			TransportResponse response;
			try {
				HttpRequestMessage requestMessage = new HttpRequestMessage(method: HttpMethod.Get, requestUri: transportRequest.RequestUrl);
				if (transportRequest.Headers.Keys.Count > 0) {
					foreach (var kvp in transportRequest.Headers) {
						requestMessage.Headers.Add(kvp.Key, kvp.Value);
					}
				}
				var httpResult = await httpClient.SendAsync(request: requestMessage, cancellationToken: transportRequest.CancellationToken).ConfigureAwait(false);
				var responseContent = await httpResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				response = new TransportResponse() {
					StatusCode = (int)httpResult.StatusCode,
					Content = responseContent,
					Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
					RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
				};
			} catch (Exception ex) {
				response = new TransportResponse() {
					RequestUrl = transportRequest.RequestUrl,
					Error = ex
				};
			}
			return response;
		}

		public async Task<TransportResponse> PostRequest(TransportRequest transportRequest)
		{
			TransportResponse transportResponse; 
			try
			{
				HttpContent postData = null;
				if (!string.IsNullOrEmpty(transportRequest.BodyContentString)) {
					postData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8, "application/json");
				} else if (transportRequest.BodyContentBytes != null)
				{
					postData = new ByteArrayContent(transportRequest.BodyContentBytes);
					foreach (var transportRequestHeader in transportRequest.Headers)
					{
						postData.Headers.Add(transportRequestHeader.Key, transportRequestHeader.Value);
					}
				}
				HttpRequestMessage requestMessage = new HttpRequestMessage(method: HttpMethod.Post, requestUri: transportRequest.RequestUrl) { Content = postData };
				
				var httpResult = await httpClient.SendAsync(request: requestMessage, cancellationToken: transportRequest.CancellationToken).ConfigureAwait(false);
				var responseContent = await httpResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				transportResponse= new TransportResponse() {
					StatusCode = (int)httpResult.StatusCode,
					Content = responseContent,
					Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
					RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
				};
			} catch (Exception e) {
				transportResponse = new TransportResponse() {
					RequestUrl = transportRequest.RequestUrl,
					Error = e
				};
			}
			return transportResponse;
		}

		public async Task<TransportResponse> PutRequest(TransportRequest transportRequest)
		{
			TransportResponse transportResponse;
			try
			{
				HttpContent putData = null;

				if (!string.IsNullOrEmpty(transportRequest.BodyContentString))
				{
					putData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8, "application/json");
				}
				else if (transportRequest.BodyContentBytes != null)
				{
					putData = new ByteArrayContent(transportRequest.FormData);
					foreach (var transportRequestHeader in transportRequest.Headers)
					{
						putData.Headers.Add(transportRequestHeader.Key, transportRequestHeader.Value);
					}
				}

				HttpRequestMessage requestMessage =
					new HttpRequestMessage(method: HttpMethod.Put, requestUri: transportRequest.RequestUrl)
						{ Content = putData };
				if (transportRequest.Headers.Keys.Count > 0)
				{
					foreach (var kvp in transportRequest.Headers)
					{
						requestMessage.Headers.Add(kvp.Key, kvp.Value);
					}
				}

				var httpResult = await httpClient.SendAsync(request: requestMessage,
					cancellationToken: transportRequest.CancellationToken).ConfigureAwait(false);
				var responseContent = await httpResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				transportResponse = new TransportResponse()
				{
					StatusCode = (int)httpResult.StatusCode,
					Content = responseContent,
					Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
					RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
				};
			}
			catch (Exception e)
			{
				transportResponse = new TransportResponse() {
					RequestUrl = transportRequest.RequestUrl,
					Error = e
				};
			}
			return transportResponse;
		}

		public async Task<TransportResponse> DeleteRequest(TransportRequest transportRequest)
		{
			TransportResponse response;
			try {
				if (transportRequest.Timeout.HasValue) httpClient.Timeout = (TimeSpan)transportRequest.Timeout;
				HttpRequestMessage requestMessage = new HttpRequestMessage(method: HttpMethod.Delete, requestUri: transportRequest.RequestUrl);
				if (transportRequest.Headers.Keys.Count > 0) {
					foreach (var kvp in transportRequest.Headers) {
						requestMessage.Headers.Add(kvp.Key, kvp.Value);
					}
				}
				var httpResult = await httpClient.SendAsync(request: requestMessage, cancellationToken: transportRequest.CancellationToken).ConfigureAwait(false);
				var responseContent = await httpResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				response = new TransportResponse() {
					StatusCode = (int)httpResult.StatusCode,
					Content = responseContent,
					Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
					RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
				};
			} catch (Exception ex) {
				response = new TransportResponse() {
					RequestUrl = transportRequest.RequestUrl,
					Error = ex
				};
			}
			return response;
		}

		public async Task<TransportResponse> PatchRequest(TransportRequest transportRequest)
		{
			TransportResponse transportResponse;
			try
			{
				HttpContent patchData = null;

				if (!string.IsNullOrEmpty(transportRequest.BodyContentString))
				{
					patchData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8,
						"application/json");
				}
				else if (transportRequest.BodyContentBytes != null)
				{
					patchData = new ByteArrayContent(transportRequest.FormData);
					foreach (var transportRequestHeader in transportRequest.Headers)
					{
						patchData.Headers.Add(transportRequestHeader.Key, transportRequestHeader.Value);
					}
				}

				HttpRequestMessage requestMessage =
					new HttpRequestMessage(new HttpMethod("PATCH"), requestUri: transportRequest.RequestUrl)
						{ Content = patchData };
				if (transportRequest.Headers.Keys.Count > 0)
				{
					foreach (var kvp in transportRequest.Headers)
					{
						requestMessage.Headers.Add(kvp.Key, kvp.Value);
					}
				}

				var httpResult = await httpClient.SendAsync(request: requestMessage,
					cancellationToken: transportRequest.CancellationToken).ConfigureAwait(false);
				var responseContent = await httpResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				transportResponse = new TransportResponse()
				{
					StatusCode = (int)httpResult.StatusCode,
					Content = responseContent,
					Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
					RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
				};
			}
			catch (Exception e)
			{
				transportResponse = new TransportResponse() {
					RequestUrl = transportRequest.RequestUrl,
					Error = e
				};
			}

			return transportResponse;
		}
	}
}

