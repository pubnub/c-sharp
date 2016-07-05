using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public interface IPubnubHttp
    {
        PubnubWebRequest SetProxy<T>(PubnubWebRequest request);

        PubnubWebRequest SetTimeout<T>(RequestState<T> pubnubRequestState, PubnubWebRequest request);

        PubnubWebRequest SetServicePointSetTcpKeepAlive(PubnubWebRequest request);

        void SendRequestAndGetResult<T>(Uri requestUri, RequestState<T> pubnubRequestState, PubnubWebRequest request);
    }

}
