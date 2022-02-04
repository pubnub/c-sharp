using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi
{
    public interface IStatelessPubnubHttp
    {
        HttpWebRequest SetProxy<T>(HttpWebRequest request);

        HttpWebRequest SetTimeout<T>(RequestState<T> pubnubRequestState, HttpWebRequest request);

        HttpWebRequest SetNoCache<T>(HttpWebRequest request);

        HttpWebRequest SetServicePointSetTcpKeepAlive(HttpWebRequest request);

        Task<string> SendRequestAndGetJsonResponse<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, CancellationToken cancellationToken);

        Task<string> SendRequestAndGetJsonResponseWithPOST<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, byte[] postData, string contentType, CancellationToken cancellationToken);

        Task<string> SendRequestAndGetJsonResponseWithPATCH<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, byte[] patchData, CancellationToken cancellationToken);

        Task<byte[]> SendRequestAndGetStreamResponse<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, CancellationToken cancellationToken);
    }

}
