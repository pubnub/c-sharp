using System.Threading.Tasks;

namespace PubnubApi
{
	public interface ITransportMiddleware
	{
		TransportRequest PreapareTransportRequest(RequestParameter requestParameter, PNOperationType operationType);
		Task<TransportResponse> Send(TransportRequest transportRequest);
	}
}