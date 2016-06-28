
using System;
using System.Diagnostics;
using System.Net;

namespace PubnubApi
{
    internal static class PubnubErrorCodeHelper
    {

        public static PubnubErrorCode GetErrorType(WebExceptionStatus webExceptionStatus, string webExceptionMessage)
        {
            PubnubErrorCode ret = PubnubErrorCode.None;
            switch (webExceptionStatus)
            {
                case WebExceptionStatus.RequestCanceled:
                    ret = PubnubErrorCode.WebRequestCanceled;
                    break;
                case WebExceptionStatus.ConnectFailure:
                    ret = PubnubErrorCode.ConnectFailure;
                    break;
                case WebExceptionStatus.Pending:
                    if (webExceptionMessage == "Machine suspend mode enabled. No request will be processed.")
                    {
                        ret = PubnubErrorCode.PubnubClientMachineSleep;
                    }
                    break;
                default:
#if NETFX_CORE
                if (webExceptionStatus.ToString() == "NameResolutionFailure")
                {
                    ret = PubnubErrorCode.NameResolutionFailure;
                }
                else
                {
                    Debug.WriteLine("ATTENTION: webExceptionStatus = " + webExceptionStatus.ToString());
                    ret = PubnubErrorCode.None;
                }
#else
                    Debug.WriteLine("ATTENTION: webExceptionStatus = " + webExceptionStatus.ToString());
                    ret = PubnubErrorCode.None;
#endif
                    break;
            }
            return ret;
        }

        public static PubnubErrorCode GetErrorType(Exception ex)
        {
            PubnubErrorCode ret = PubnubErrorCode.None;

            string errorType = ex.GetType().ToString();
            string errorMessage = ex.Message;

            if (errorType == "System.FormatException" && errorMessage == "Invalid length for a Base-64 char array or string.")
            {
                ret = PubnubErrorCode.PubnubMessageDecryptException;
            }
            else if (errorType == "System.FormatException" && errorMessage == "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters. ")
            {
                ret = PubnubErrorCode.PubnubMessageDecryptException;
            }
            else if (errorType == "System.ObjectDisposedException" && errorMessage == "Cannot access a disposed object.")
            {
                ret = PubnubErrorCode.PubnubObjectDisposedException;
            }
            else if (errorType == "System.Net.Sockets.SocketException" && errorMessage == "The requested name is valid, but no data of the requested type was found")
            {
                ret = PubnubErrorCode.PubnubSocketConnectException;
            }
            else if (errorType == "System.Net.Sockets.SocketException" && errorMessage == "No such host is known")
            {
                ret = PubnubErrorCode.PubnubSocketConnectException;
            }
            else if (errorType == "System.Security.Cryptography.CryptographicException" && errorMessage == "Padding is invalid and cannot be removed.")
            {
                ret = PubnubErrorCode.PubnubCryptographicException;
            }
            else if (errorType == "System.Runtime.InteropServices.SEHException" && errorMessage == "External component has thrown an exception.")
            {
                ret = PubnubErrorCode.PubnubInterOpSEHException;
            }
            else if (errorType == "System.Net.WebException" && errorMessage.Contains("The remote name could not be resolved:"))
            {
                ret = PubnubErrorCode.NameResolutionFailure;
            }
            else if (errorType == "System.Net.WebException" && errorMessage.Contains("Unable to connect to the remote server"))
            {
                ret = PubnubErrorCode.NameResolutionFailure;
            }
            else
            {
                //Console.WriteLine("ATTENTION: Error Type = " + errorType);
                //Console.WriteLine("ATTENTION: Error Message = " + errorMessage);
                ret = PubnubErrorCode.None;
            }
            return ret;
        }

        public static PubnubErrorCode GetErrorType(int statusCode, string httpErrorCodeMessage)
        {
            PubnubErrorCode ret = PubnubErrorCode.None;

            switch (statusCode)
            {
                case 400:
                    if (httpErrorCodeMessage.ToUpper().Contains("MESSAGE TOO LARGE"))
                    {
                        ret = PubnubErrorCode.MessageTooLarge;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "INVALID KEY")
                    {
                        ret = PubnubErrorCode.InvalidKey;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "BADREQUEST")
                    {
                        ret = PubnubErrorCode.BadRequest;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "NO UUID SPECIFIED")
                    {
                        ret = PubnubErrorCode.NoUuidSpecified;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "INVALID TIMESTAMP")
                    {
                        ret = PubnubErrorCode.InvalidTimestamp;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "INVALID TYPE ARGUMENT")
                    {
                        ret = PubnubErrorCode.InvalidTypeArgument;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "CHANNEL GROUP OR GROUPS RESULT IN EMPTY SUBSCRIPTION SET")
                    {
                        ret = PubnubErrorCode.EmptyGroupSubscription;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "COULD NOT PARSE REQUEST")
                    {
                        ret = PubnubErrorCode.CouldNotParseRequest;
                    }
                    break;
                case 401:
                    ret = PubnubErrorCode.InvalidSubscribeKey;
                    break;
                case 402:
                    if (httpErrorCodeMessage.ToUpper() == "NOT ENABLED")
                    {
                        ret = PubnubErrorCode.PamNotEnabled;
                    }
                    break;
                case 403:
                    if (httpErrorCodeMessage.ToUpper() == "FORBIDDEN")
                    {
                        ret = PubnubErrorCode.Forbidden;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "SIGNATURE DOES NOT MATCH")
                    {
                        ret = PubnubErrorCode.SignatureDoesNotMatch;
                    }
                    break;
                case 404:
                    ret = PubnubErrorCode.NotFound;
                    break;
                case 414:
                    ret = PubnubErrorCode.RequestUriTooLong;
                    break;
                case 500:
                    ret = PubnubErrorCode.InternalServerError;
                    break;
                case 502:
                    ret = PubnubErrorCode.BadGateway;
                    break;
                case 503:
                    ret = PubnubErrorCode.ServiceUnavailable;
                    break;
                case 504:
                    ret = PubnubErrorCode.GatewayTimeout;
                    break;
                default:
                    ret = PubnubErrorCode.None;
                    break;
            }

            return ret;
        }
    }
}
