using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace PubnubApi
{
	public class HttpClientService : IHttpClientService
	{
		private readonly HttpClient _httpClient;

		public HttpClientService()
		{
			_httpClient = new HttpClient();
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
				System.Diagnostics.Debug.WriteLine($"Making call with {requestMessage.RequestUri}");
				var httpResult = await _httpClient.SendAsync(request: requestMessage, cancellationToken: transportRequest.CancellationToken);
				var responseContent = await httpResult.Content.ReadAsByteArrayAsync();
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
			if (transportRequest.Timeout.HasValue) _httpClient.Timeout = (TimeSpan)transportRequest.Timeout;
			HttpContent postData = null;

			if (!string.IsNullOrEmpty(transportRequest.BodyContentString)) {
				postData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8, "application/json");
			} else if (transportRequest.BodyContentBytes != null) {
				postData = new ByteArrayContent(transportRequest.FormData);
				postData.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
			}
			HttpRequestMessage requestMessage = new HttpRequestMessage(method: HttpMethod.Post, requestUri: transportRequest.RequestUrl) { Content = postData };
			if (transportRequest.Headers.Keys.Count > 0) {
				foreach (var kvp in transportRequest.Headers) {
					requestMessage.Headers.Add(kvp.Key, kvp.Value);
				}
			}
			var httpResult = await _httpClient.SendAsync(request: requestMessage, cancellationToken: transportRequest.CancellationToken);
			var responseContent = await httpResult.Content.ReadAsByteArrayAsync();
			var response = new TransportResponse() {
				StatusCode = (int)httpResult.StatusCode,
				Content = responseContent,
				Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
				RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
			};
			return response;
		}

		public async Task<TransportResponse> PutRequest(TransportRequest transportRequest)
		{
			if (transportRequest.Timeout.HasValue) _httpClient.Timeout = (TimeSpan)transportRequest.Timeout;
			HttpContent postData = null;

			if (!string.IsNullOrEmpty(transportRequest.BodyContentString)) {
				postData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8, "application/json");
			} else if (transportRequest.BodyContentBytes != null) {
				postData = new ByteArrayContent(transportRequest.FormData);
				postData.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
			}
			HttpRequestMessage requestMessage = new HttpRequestMessage(method: HttpMethod.Put, requestUri: transportRequest.RequestUrl) { Content = postData };
			if (transportRequest.Headers.Keys.Count > 0) {
				foreach (var kvp in transportRequest.Headers) {
					requestMessage.Headers.Add(kvp.Key, kvp.Value);
				}
			}
			var httpResult = await _httpClient.SendAsync(request: requestMessage, cancellationToken: transportRequest.CancellationToken);
			var responseContent = await httpResult.Content.ReadAsByteArrayAsync();
			var response = new TransportResponse() {
				StatusCode = (int)httpResult.StatusCode,
				Content = responseContent,
				Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
				RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
			};
			return response;
		}

		public async Task<TransportResponse> DeleteRequest(TransportRequest transportRequest)
		{
			TransportResponse response;
			try {
				if (transportRequest.Timeout.HasValue) _httpClient.Timeout = (TimeSpan)transportRequest.Timeout;
				HttpRequestMessage requestMessage = new HttpRequestMessage(method: HttpMethod.Delete, requestUri: transportRequest.RequestUrl);
				if (transportRequest.Headers.Keys.Count > 0) {
					foreach (var kvp in transportRequest.Headers) {
						requestMessage.Headers.Add(kvp.Key, kvp.Value);
					}
				}
				var httpResult = await _httpClient.SendAsync(request: requestMessage, cancellationToken: transportRequest.CancellationToken);
				var responseContent = await httpResult.Content.ReadAsByteArrayAsync();
				response = new TransportResponse() {
					StatusCode = (int)httpResult.StatusCode,
					Content = responseContent,
					Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
					RequestUrl = httpResult.RequestMessage.RequestUri.AbsolutePath
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
			if (transportRequest.Timeout.HasValue) _httpClient.Timeout = (TimeSpan)transportRequest.Timeout;
			HttpContent patchData = null;

			if (!string.IsNullOrEmpty(transportRequest.BodyContentString)) {
				patchData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8, "application/json");
			} else if (transportRequest.BodyContentBytes != null) {
				patchData = new ByteArrayContent(transportRequest.FormData);
				patchData.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
			}
			HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri: transportRequest.RequestUrl) { Content = patchData };
			if (transportRequest.Headers.Keys.Count > 0) {
				foreach (var kvp in transportRequest.Headers) {
					requestMessage.Headers.Add(kvp.Key, kvp.Value);
				}
			}
			var httpResult = await _httpClient.SendAsync(request: requestMessage, cancellationToken: transportRequest.CancellationToken);
			var responseContent = await httpResult.Content.ReadAsByteArrayAsync();
			var response = new TransportResponse() {
				StatusCode = (int)httpResult.StatusCode,
				Content = responseContent,
				Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
				RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
			};
			return response;
		}
	}
}

