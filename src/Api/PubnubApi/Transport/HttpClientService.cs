using System;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;

namespace PubnubApi
{
	public class HttpClientService
	{
		private static readonly HttpClient _httpClient = new HttpClient();

		public HttpClientService()
		{
		}

		public async Task<HttpResponseMessage> GetRequest(string url, CancellationToken cancellationToken, TimeSpan? timeout = null)
		{
			if (timeout.HasValue) _httpClient.Timeout = (TimeSpan)timeout;
			return await _httpClient.GetAsync(url, cancellationToken);
		}

		public Task<HttpResponseMessage> PostRequest(string url, HttpContent content, CancellationToken cancellationToken)
		{
			return _httpClient.PostAsync(url, content, cancellationToken);
		}

		public Task<HttpResponseMessage> PutRequest(string url, HttpContent content, CancellationToken cancellationToken)
		{
			return _httpClient.PutAsync(url, content, cancellationToken);
		}

		public Task<HttpResponseMessage> DeleteRequest(string url, CancellationToken cancellationToken)
		{
			return _httpClient.DeleteAsync(url);
		}

	}
}

