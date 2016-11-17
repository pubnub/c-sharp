using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace PubnubApi
{
    public class StatusBuilder
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;

        public StatusBuilder(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public PNStatus CreateStatusResponse<T>(PNOperationType type, PNStatusCategory category, RequestState<T> asyncRequestState, int statusCode, Exception throwable)
        {
            PNStatus status = new PNStatus();
            status.Category = category;
            status.Operation = type;

            if ((asyncRequestState != null && asyncRequestState.Response == null) || throwable != null)
            {
                status.Error = true;
            }

            if (throwable != null)
            {
                PNErrorData errorData = new PNErrorData(throwable.Message, throwable);
                status.ErrorData = errorData;
            }

            if (asyncRequestState != null)
            {
                if (asyncRequestState.Request != null)
                {
                    status.ClientRequest = asyncRequestState.Request;

                    HttpValueCollection restUriQueryCollection = HttpUtility.ParseQueryString(asyncRequestState.Request.RequestUri.Query);
                    if (restUriQueryCollection.ContainsKey("auth"))
                    {
                        string auth = restUriQueryCollection["auth"];
                        status.AuthKey = auth;
                    }
                    if (restUriQueryCollection.ContainsKey("uuid"))
                    {
                        string uuid = restUriQueryCollection["uuid"];
                        status.Uuid = uuid;
                    }
                }

                if (asyncRequestState.Response != null)
                {
                    status.StatusCode = (int)asyncRequestState.Response.StatusCode;
                }
                else
                {
                    status.StatusCode = statusCode;
                }

                if (asyncRequestState.ChannelGroups != null)
                {
                    status.AffectedChannelGroups = asyncRequestState.ChannelGroups.ToList<string>();
                }

                if (asyncRequestState.Channels != null)
                {
                    status.AffectedChannels = asyncRequestState.Channels.ToList<string>();
                }
            }
            else
            {
                status.StatusCode = statusCode;
            }
            status.Origin = config.Origin;
            status.TlsEnabled = config.Secure;

            return status;
        }

    }
}
