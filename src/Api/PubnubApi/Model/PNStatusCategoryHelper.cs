
using System;
using System.Diagnostics;
using System.Net;

namespace PubnubApi
{
    internal static class PNStatusCategoryHelper
    {

        public static PNStatusCategory GetPNStatusCategory(WebExceptionStatus webExceptionStatus, string webExceptionMessage)
        {
            PNStatusCategory ret = PNStatusCategory.PNUnknownCategory;
            switch (webExceptionStatus)
            {
                case WebExceptionStatus.RequestCanceled:
                    ret = PNStatusCategory.PNCancelledCategory;
                    break;
                case WebExceptionStatus.ConnectFailure:
                    ret = PNStatusCategory.PNNetworkIssuesCategory;
                    break;
                case WebExceptionStatus.SendFailure:
                    ret = PNStatusCategory.PNNetworkIssuesCategory;
                    break;
                case WebExceptionStatus.Pending:
                    ret = PNStatusCategory.PNNetworkIssuesCategory;
                    break;
                case WebExceptionStatus.Success:
                    ret = PNStatusCategory.PNNetworkIssuesCategory;
                    break;
                default:
                    if (webExceptionStatus.ToString() == "SecureChannelFailure")
                    {
                        ret = PNStatusCategory.PNNetworkIssuesCategory;
                    }
                    else if (webExceptionStatus.ToString() == "NameResolutionFailure")
                    {
                        ret = PNStatusCategory.PNNetworkIssuesCategory;
                    }
                    else
                    {
                        Debug.WriteLine("ATTENTION: webExceptionStatus = " + webExceptionStatus.ToString());
                        ret = PNStatusCategory.PNUnknownCategory;
                    }
                    break;
            }
            return ret;
        }

        public static PNStatusCategory GetPNStatusCategory(Exception ex)
        {
            PNStatusCategory ret = PNStatusCategory.PNUnknownCategory;

            if (ex == null) return ret;

            string errorType = ex.GetType().ToString();
            string errorMessage = ex.Message;

            if (errorType == "System.FormatException" && errorMessage == "Invalid length for a Base-64 char array or string.")
            {
                ret = PNStatusCategory.PNDecryptionErrorCategory;
            }
            else if (errorType == "System.FormatException" && errorMessage == "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters. ")
            {
                ret = PNStatusCategory.PNDecryptionErrorCategory;
            }
            else if (errorType == "System.ObjectDisposedException" && errorMessage == "Cannot access a disposed object.")
            {
                ret = PNStatusCategory.PNUnknownCategory;
            }
            else if (errorType == "System.Net.Sockets.SocketException" && errorMessage == "The requested name is valid, but no data of the requested type was found")
            {
                ret = PNStatusCategory.PNNetworkIssuesCategory;
            }
            else if (errorType == "System.Net.Sockets.SocketException" && errorMessage == "No such host is known")
            {
                ret = PNStatusCategory.PNNetworkIssuesCategory;
            }
            else if (errorType == "System.Security.Cryptography.CryptographicException" && errorMessage == "Padding is invalid and cannot be removed.")
            {
                ret = PNStatusCategory.PNDecryptionErrorCategory;
            }
            else if (errorType == "System.Runtime.InteropServices.SEHException" && errorMessage == "External component has thrown an exception.")
            {
                ret = PNStatusCategory.PNUnknownCategory;
            }
            else if (errorType == "System.Net.WebException" && errorMessage.Contains("The remote name could not be resolved:"))
            {
                ret = PNStatusCategory.PNNetworkIssuesCategory;
            }
            else if (errorType == "System.Net.WebException" && errorMessage.Contains("Unable to connect to the remote server"))
            {
                ret = PNStatusCategory.PNNetworkIssuesCategory;
            }
            else if (errorType == "System.Net.WebException" && errorMessage.Contains("Unable to read data from the transport connection"))
            {
                ret = PNStatusCategory.PNNetworkIssuesCategory;
            }
            else if (errorType == "System.Net.WebException" && errorMessage.Contains("SecureChannelFailure"))
            {
                ret = PNStatusCategory.PNNetworkIssuesCategory;
            }
            else if (errorType == "System.Net.WebException" && errorMessage.Contains("ConnectFailure"))
            {
                ret = PNStatusCategory.PNNetworkIssuesCategory;
            }
            else if (errorType == "System.Net.WebException" && errorMessage.Contains("ReceiveFailure"))
            {
                ret = PNStatusCategory.PNNetworkIssuesCategory;
            }
            else if (errorType == "System.Net.WebException" && errorMessage.Contains("SendFailure"))
            {
                ret = PNStatusCategory.PNNetworkIssuesCategory;
            }
            else if (errorType == "System.ArgumentException" && errorMessage.Contains("cannot be converted to type"))
            {
                ret = PNStatusCategory.PNMalformedResponseCategory;
            }
            else if (errorMessage.Contains("Disconnected"))
            {
                ret = PNStatusCategory.PNDisconnectedCategory;
            }
            else
            {
                //Console.WriteLine("ATTENTION: Error Type = " + errorType);
                //Console.WriteLine("ATTENTION: Error Message = " + errorMessage);
                ret = PNStatusCategory.PNUnknownCategory;
            }
            return ret;
        }

        public static PNStatusCategory GetPNStatusCategory(int statusCode, string httpErrorCodeMessage)
        {
            PNStatusCategory ret = PNStatusCategory.PNUnknownCategory;

            switch (statusCode)
            {
                case 400:
                    if (httpErrorCodeMessage.ToUpper().Contains("MESSAGE TOO LARGE"))
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "INVALID KEY" || httpErrorCodeMessage.ToUpper() == "INVALID SUBSCRIBE KEY")
                    {
                        ret = PNStatusCategory.PNAccessDeniedCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "BADREQUEST")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "NO UUID SPECIFIED")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "INVALID TIMESTAMP")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "INVALID TYPE ARGUMENT")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "CHANNEL GROUP OR GROUPS RESULT IN EMPTY SUBSCRIPTION SET")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "COULD NOT PARSE REQUEST")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    break;
                case 401:
                    ret = PNStatusCategory.PNAccessDeniedCategory;
                    break;
                case 402:
                    if (httpErrorCodeMessage.ToUpper() == "NOT ENABLED")
                    {
                        ret = PNStatusCategory.PNAccessDeniedCategory;
                    }
                    break;
                case 403:
                    if (httpErrorCodeMessage.ToUpper() == "FORBIDDEN")
                    {
                        ret = PNStatusCategory.PNAccessDeniedCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpper() == "SIGNATURE DOES NOT MATCH")
                    {
                        ret = PNStatusCategory.PNAccessDeniedCategory;
                    }
                    break;
                case 404:
                    ret = PNStatusCategory.PNBadRequestCategory;
                    break;
                case 414:
                    ret = PNStatusCategory.PNBadRequestCategory;
                    break;
                case 500:
                    ret = PNStatusCategory.PNBadRequestCategory;
                    break;
                case 502:
                    ret = PNStatusCategory.PNNetworkIssuesCategory;
                    break;
                case 503:
                    ret = PNStatusCategory.PNNetworkIssuesCategory;
                    break;
                case 504:
                    ret = PNStatusCategory.PNNetworkIssuesCategory;
                    break;
                default:
                    ret = PNStatusCategory.PNUnknownCategory;
                    break;
            }

            return ret;
        }
    }
}
