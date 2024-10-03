using System.Threading.Tasks;

namespace PubnubApi
{
	public interface IHttpClientService
	{
		Task<TransportResponse> DeleteRequest(TransportRequest transportRequest);
		Task<TransportResponse> GetRequest(TransportRequest transportRequest);
		Task<TransportResponse> PostRequest(TransportRequest transportRequest);
		Task<TransportResponse> PutRequest(TransportRequest transportRequest);
	}
}