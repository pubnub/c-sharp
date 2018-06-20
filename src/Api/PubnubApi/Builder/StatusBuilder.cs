using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace PubnubApi
{
    public class StatusBuilder
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;

        public StatusBuilder(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public PNStatus CreateStatusResponse<T>(PNOperationType type, PNStatusCategory category, RequestState<T> asyncRequestState, int statusCode, PNException throwable)
        {
            int serverErrorStatusCode = 0;
            bool serverErrorMessage = false;
            List<string> serverAffectedChannels = null;
            List<string> serverAffectedChannelGroups = null;

            PNStatus status = new PNStatus(asyncRequestState != null ? asyncRequestState.EndPointOperation : null);
            status.Category = category;
            status.Operation = type;

            if ((asyncRequestState != null && !asyncRequestState.GotJsonResponse) || throwable != null)
            {
                status.Error = true;
            }

            Exception targetException = null;
            if (throwable != null)
            {
                if (throwable.DirectException)
                {
                    targetException = throwable.InnerException;
                }
                else
                {
                    targetException = throwable as Exception;
                }
            }

            if (targetException != null)
            {
                if (targetException.InnerException != null)
                {
                    PNErrorData errorData = new PNErrorData(jsonLibrary.SerializeToJsonString(targetException.InnerException.Message), targetException);
                    status.ErrorData = errorData;
                }
                else
                {
                    Dictionary<string, object> deserializeStatus = jsonLibrary.DeserializeToDictionaryOfObject(targetException.Message);
                    if (deserializeStatus != null && deserializeStatus.Count >= 1 
                        && deserializeStatus.ContainsKey("error") && string.Equals(deserializeStatus["error"].ToString(), "true", StringComparison.CurrentCultureIgnoreCase)
                        && deserializeStatus.ContainsKey("status") && Int32.TryParse(deserializeStatus["status"].ToString(), out serverErrorStatusCode))
                    {
                        serverErrorMessage = true;
                        if (deserializeStatus.ContainsKey("payload"))
                        {
                            Dictionary<string, object> payloadDic = jsonLibrary.ConvertToDictionaryObject(deserializeStatus["payload"]);
                            if (payloadDic != null && payloadDic.Count > 0)
                            {
                                if (payloadDic.ContainsKey("channels"))
                                {
                                    object[] chDic = jsonLibrary.ConvertToObjectArray(payloadDic["channels"]);
                                    if (chDic != null && chDic.Length > 0)
                                    {
                                        serverAffectedChannels = chDic.Select(x => x.ToString()).ToList();
                                    }
                                }

                                if (payloadDic.ContainsKey("channel-groups"))
                                {
                                    object[] cgDic = jsonLibrary.ConvertToObjectArray(payloadDic["channel-groups"]);
                                    if (cgDic != null && cgDic.Length > 0)
                                    {
                                        serverAffectedChannelGroups = cgDic.Select(x => x.ToString()).ToList();
                                    }
                                }
                            }
                        }
                    }

                    PNErrorData errorData = new PNErrorData(jsonLibrary.SerializeToJsonString(targetException.Message), targetException);
                    status.ErrorData = errorData;
                }
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

                if (serverErrorMessage && serverErrorStatusCode > 0)
                {
                    status.StatusCode = serverErrorStatusCode;
                }
                else if (asyncRequestState.Response != null)
                {
                    status.StatusCode = (int)asyncRequestState.Response.StatusCode;
                }
                else
                {
                    status.StatusCode = statusCode;
                }

                if (serverErrorMessage)
                {
                    status.AffectedChannels = serverAffectedChannels;
                    status.AffectedChannelGroups = serverAffectedChannelGroups;
                }
                else
                {
                    if (asyncRequestState.ChannelGroups != null)
                    {
                        status.AffectedChannelGroups = asyncRequestState.ChannelGroups.ToList<string>();
                    }

                    if (asyncRequestState.Channels != null)
                    {
                        status.AffectedChannels = asyncRequestState.Channels.ToList<string>();
                    }
                }
            }
            else
            {
                status.StatusCode = statusCode;
            }

            if (status.StatusCode == 403)
            {
                status.Category = PNStatusCategory.PNAccessDeniedCategory;
            }


            status.Origin = config.Origin;
            status.TlsEnabled = config.Secure;

            return status;
        }

    }
}
