using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    internal class TimeOperation: PubnubCoreBase
    {
        private PNConfiguration config = null;

        public TimeOperation(PNConfiguration pnConfig):base(pnConfig)
        {
            config = pnConfig;
        }

        internal void Time(Action<long> userCallback, Action<PubnubClientError> errorCallback)
        {
            if (userCallback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }
            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config);
            Uri request = urlBuilder.BuildTimeRequest();

            RequestState<long> requestState = new RequestState<long>();
            requestState.Channels = null;
            requestState.ResponseType = ResponseType.Time;
            requestState.NonSubscribeRegularCallback = userCallback;
            requestState.ErrorCallback = errorCallback;
            requestState.Reconnect = false;

            UrlProcessRequest<long>(request, requestState, false);
        }

    }
}
