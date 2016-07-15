using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace PubnubApi
{
    internal class PNCallbackService
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLib = null;

        public PNCallbackService(PNConfiguration pnConfiguation, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            this.config = pnConfiguation;
            this.jsonLib = jsonPluggableLibrary;
        }

        private void JsonResponseToCallback<T>(List<object> result, Action<T> callback)
        {
            string callbackJson = "";

            if (typeof(T) == typeof(string))
            {
                callbackJson = jsonLib.SerializeToJsonString(result);

                Action<string> castCallback = callback as Action<string>;
                castCallback(callbackJson);
            }
        }

        private void JsonResponseToCallback<T>(object result, Action<T> callback)
        {
            string callbackJson = "";

            if (typeof(T) == typeof(string))
            {
                callbackJson = jsonLib.SerializeToJsonString(result);

                Action<string> castCallback = callback as Action<string>;
                castCallback(callbackJson);
            }
        }

        private void JsonResponseToCallback<T>(long result, Action<T> callback)
        {
            if (typeof(T) == typeof(long))
            {
                Action<long> castCallback = callback as Action<long>;
                castCallback(result);
            }
        }

        //		protected void GoToCallback<T> (object result, Action<T> Callback)
        //		{
        //			if (Callback != null) {
        //				if (typeof(T) == typeof(string)) {
        //					JsonResponseToCallback (result, Callback);
        //				} else {
        //					Callback ((T)(object)result);
        //				}
        //			}
        //		}

        internal void GoToCallback<T>(List<object> result, Action<T> Callback, bool internalObject, ResponseType type)
        {
            if (Callback != null)
            {
                if (typeof(T) == typeof(string))
                {
                    JsonResponseToCallback(result, Callback);
                }
                else if (typeof(T) == typeof(long) && type == ResponseType.Time)
                {
                    long timetoken;
                    Int64.TryParse(result[0].ToString(), out timetoken);
                    JsonResponseToCallback(timetoken, Callback);
                }
                else
                {
                    T ret = default(T);
                    if (!internalObject)
                    {
                        ret = jsonLib.DeserializeToObject<T>(result);
                    }
                    else
                    {
                        NewtonsoftJsonDotNet jsonLib = new NewtonsoftJsonDotNet();
                        ret = jsonLib.DeserializeToObject<T>(result);
                    }

                    Callback(ret);
                }
            }
        }

        protected void GoToCallback(object result, Action<string> Callback)
        {
            if (Callback != null)
            {
                JsonResponseToCallback(result, Callback);
            }
        }

        protected void GoToCallback(object result, Action<object> Callback)
        {
            if (Callback != null)
            {
                Callback(result);
            }
        }

        internal void GoToCallback(PubnubClientError error, Action<PubnubClientError> Callback)
        {
            if (Callback != null && error != null)
            {
                if ((int)error.Severity <= (int)config.ErrorLevel)
                { //Checks whether the error serverity falls in the range of error filter level
                  //Do not send 107 = PubnubObjectDisposedException
                  //Do not send 105 = WebRequestCancelled
                  //Do not send 130 = PubnubClientMachineSleep
                    if (error.StatusCode != 107
                        && error.StatusCode != 105
                        && error.StatusCode != 130
                        && error.StatusCode != 4040) //Error Code that should not go out
                    {
                        Callback(error);
                    }
                }
            }
        }

        #region "Error Callbacks"

        internal PubnubClientError CallErrorCallback(PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
                                                         string channel, string channelGroup, Action<PubnubClientError> errorCallback,
                                                         string message, PubnubErrorCode errorType, PubnubWebRequest req,
                                                         PubnubWebResponse res)
        {
            int statusCode = (int)errorType;

            string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(errorType);

            PubnubClientError error = new PubnubClientError(statusCode, errSeverity, message, msgSource, req, res, errorDescription, channel, channelGroup);
            GoToCallback(error, errorCallback);
            return error;
        }

        internal PubnubClientError CallErrorCallback(PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
                                                         string channel, string channelGroup, Action<PubnubClientError> errorCallback,
                                                         string message, int currentHttpStatusCode, string statusMessage,
                                                         PubnubWebRequest req, PubnubWebResponse res)
        {
            PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, statusMessage);

            int statusCode = (int)pubnubErrorType;

            string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);

            PubnubClientError error = new PubnubClientError(statusCode, errSeverity, message, msgSource, req, res, errorDescription, channel, channelGroup);
            GoToCallback(error, errorCallback);
            return error;
        }

        internal PubnubClientError CallErrorCallback(PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
                                                         string channel, string channelGroup, Action<PubnubClientError> errorCallback,
                                                         Exception ex, PubnubWebRequest req,
                                                         PubnubWebResponse res)
        {
            PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType(ex);

            int statusCode = (int)errorType;
            string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(errorType);

            PubnubClientError error = new PubnubClientError(statusCode, errSeverity, true, ex.Message, ex, msgSource, req, res, errorDescription, channel, channelGroup);
            GoToCallback(error, errorCallback);
            return error;
        }

        internal PubnubClientError CallErrorCallback(PubnubErrorSeverity errSeverity, PubnubMessageSource msgSource,
                                                         string channel, string channelGroup, Action<PubnubClientError> errorCallback,
                                                         WebException webex, PubnubWebRequest req,
                                                         PubnubWebResponse res)
        {
            PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType(webex.Status, webex.Message);
            int statusCode = (int)errorType;
            string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(errorType);

            PubnubClientError error = new PubnubClientError(statusCode, errSeverity, true, webex.Message, webex, msgSource, req, res, errorDescription, channel, channelGroup);
            GoToCallback(error, errorCallback);
            return error;
        }

        #endregion
    }
}
