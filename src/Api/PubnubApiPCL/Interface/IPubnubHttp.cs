using System;
using System.Threading.Tasks;

namespace PubnubApi
{
    public interface IPubnubHttp
    {
        PubnubWebRequest SetProxy<T>(PubnubWebRequest request);

        PubnubWebRequest SetTimeout<T>(RequestState<T> pubnubRequestState, PubnubWebRequest request);

        PubnubWebRequest SetServicePointSetTcpKeepAlive(PubnubWebRequest request);

        //void SendRequestAndGetResult<T>(Uri requestUri, RequestState<T> pubnubRequestState, PubnubWebRequest request);

        Task<string> SendRequestAndGetJsonResponse<T>(Uri requestUri, RequestState<T> pubnubRequestState, PubnubWebRequest request);
    }

}
