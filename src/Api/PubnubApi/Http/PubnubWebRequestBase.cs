using System;
using System.IO;
using System.Net;
using System.Text;

namespace PubnubApi
{
    public abstract class PubnubWebRequestBase : WebRequest
    {
        internal IPubnubUnitTest pubnubUnitTest = null;
        private static bool simulateNetworkFailForTesting = false;
        private static bool machineSuspendMode = false;
        private bool terminated = false;
        PubnubErrorFilter.Level filterErrorLevel = PubnubErrorFilter.Level.Info;
        internal HttpWebRequest request;

        internal static bool SimulateNetworkFailForTesting
        {
            get
            {
                return simulateNetworkFailForTesting;
            }
            set
            {
                simulateNetworkFailForTesting = value;
            }
        }

        internal static bool MachineSuspendMode
        {
            get
            {
                return machineSuspendMode;
            }
            set
            {
                machineSuspendMode = value;
            }
        }

        public PubnubWebRequestBase(HttpWebRequest request)
        {
            this.request = request;
        }

        public PubnubWebRequestBase(HttpWebRequest request, IPubnubUnitTest pubnubUnitTest)
        {
            this.request = request;
            this.pubnubUnitTest = pubnubUnitTest;
        }

        public override void Abort()
        {
            if (request != null)
            {
                terminated = true;
                request.Abort();
            }
        }

        public void Abort(Action<PubnubClientError> errorCallback, PubnubErrorFilter.Level errorLevel)
        {
            if (request != null)
            {
                terminated = true;
                try
                {
                    request.Abort();
                }
                catch (WebException webEx)
                {
                    if (errorCallback != null)
                    {
                        HttpStatusCode currentHttpStatusCode;

                        filterErrorLevel = errorLevel;
                        if (webEx.Response.GetType().ToString() == "System.Net.HttpWebResponse"
                                  || webEx.Response.GetType().ToString() == "System.Net.Browser.ClientHttpWebResponse")
                        {
                            currentHttpStatusCode = ((HttpWebResponse)webEx.Response).StatusCode;
                        }
                        else
                        {
                            currentHttpStatusCode = ((PubnubWebResponse)webEx.Response).HttpStatusCode;
                        }
                        string statusMessage = currentHttpStatusCode.ToString();
                        PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, statusMessage);
                        int pubnubStatusCode = (int)pubnubErrorType;
                        string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);

                        PubnubClientError error = new PubnubClientError(pubnubStatusCode, PubnubErrorSeverity.Critical, true, webEx.Message, webEx, PubnubMessageSource.Client, null, null, errorDescription, "", "");
                        GoToCallback(error, errorCallback);
                    }
                }
                catch (Exception ex)
                {
                    if (errorCallback != null)
                    {
                        filterErrorLevel = errorLevel;
                        PubnubErrorCode errorType = PubnubErrorCodeHelper.GetErrorType(ex);
                        int statusCode = (int)errorType;
                        string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(errorType);
                        PubnubClientError error = new PubnubClientError(statusCode, PubnubErrorSeverity.Critical, true, ex.Message, ex, PubnubMessageSource.Client, null, null, errorDescription, "", "");
                        GoToCallback(error, errorCallback);
                    }
                }
            }
        }

        private void GoToCallback(PubnubClientError error, Action<PubnubClientError> Callback)
        {
            if (Callback != null && error != null)
            {
                if ((int)error.Severity <= (int)filterErrorLevel)
                { //Checks whether the error serverity falls in the range of error filter level
                  //Do not send 107 = PubnubObjectDisposedException
                  //Do not send 105 = WebRequestCancelled
                  //Do not send 130 = PubnubClientMachineSleep
                    if (error.StatusCode != 107
                             && error.StatusCode != 105
                             && error.StatusCode != 130)
                    { //Error Code that should not go out
                        Callback(error);
                    }
                }
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                return request.Headers;
            }
            set
            {
                request.Headers = value;
            }
        }

        public override string Method
        {
            get
            {
                return request.Method;
            }
            set
            {
                request.Method = value;
            }
        }

        public override string ContentType
        {
            get
            {
                return request.ContentType;
            }
            set
            {
                request.ContentType = value;
            }
        }

        public override ICredentials Credentials
        {
            get
            {
                return request.Credentials;
            }
            set
            {
                request.Credentials = value;
            }
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            return request.BeginGetRequestStream(callback, state);
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return request.EndGetRequestStream(asyncResult);
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            if (pubnubUnitTest is IPubnubUnitTest && pubnubUnitTest.EnableStubTest)
            {
                return new PubnubWebAsyncResult(callback, state);
            }
            else if (machineSuspendMode)
            {
                return new PubnubWebAsyncResult(callback, state);
            }
            else
            {
                return request.BeginGetResponse(callback, state);
            }
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            if (pubnubUnitTest is IPubnubUnitTest && pubnubUnitTest.EnableStubTest)
            {
                string stubResponse = pubnubUnitTest.GetStubResponse(request);
                return new PubnubWebResponse(new MemoryStream(Encoding.UTF8.GetBytes(stubResponse)));
            }
            else if (machineSuspendMode)
            {
                WebException simulateException = new WebException("Machine suspend mode enabled. No request will be processed.", WebExceptionStatus.Pending);
                throw simulateException;
            }
            else if (simulateNetworkFailForTesting)
            {
                WebException simulateException = new WebException("For simulating network fail, the remote name could not be resolved", WebExceptionStatus.ConnectFailure);
                throw simulateException;
            }
            else
            {
                return new PubnubWebResponse(request.EndGetResponse(asyncResult));
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return request.RequestUri;
            }
        }

        public override bool UseDefaultCredentials
        {
            get
            {
                return request.UseDefaultCredentials;
            }
        }

        public bool Terminated
        {
            get
            {
                return terminated;
            }
        }
    }
}
