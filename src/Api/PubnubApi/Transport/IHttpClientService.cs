using System;
using System.Threading.Tasks;

namespace PubnubApi.Transport
{
	public interface IHttpClientService <TRequest, TResponse>
	{
		Task<TResponse> GetRequest(TRequest request);

		Task<TResponse> PostRequest(TRequest request);

		Task<TResponse> PutRequest(TRequest request);

		Task<TResponse> DeleteRequest(TRequest request);
	}
}

