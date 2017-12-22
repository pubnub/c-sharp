
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
                case WebExceptionStatus.SendFailure:
                case WebExceptionStatus.Pending:
                case WebExceptionStatus.Success:
                    ret = PNStatusCategory.PNNetworkIssuesCategory;
                    break;
                default:
                    if (string.Compare(webExceptionStatus.ToString(), "SecureChannelFailure", StringComparison.CurrentCultureIgnoreCase) == 0 
                        || string.Compare(webExceptionStatus.ToString(),"NameResolutionFailure", StringComparison.CurrentCultureIgnoreCase) == 0)
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

            if (ex == null) { return ret; }

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
                    if (httpErrorCodeMessage.ToUpperInvariant().Contains("MESSAGE TOO LARGE"))
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpperInvariant() == "INVALID KEY" || httpErrorCodeMessage.ToUpperInvariant() == "INVALID SUBSCRIBE KEY")
                    {
                        ret = PNStatusCategory.PNAccessDeniedCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpperInvariant() == "BADREQUEST")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpperInvariant() == "NO UUID SPECIFIED")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpperInvariant() == "INVALID TIMESTAMP")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpperInvariant() == "INVALID TYPE ARGUMENT")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpperInvariant() == "CHANNEL GROUP OR GROUPS RESULT IN EMPTY SUBSCRIPTION SET")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpperInvariant() == "COULD NOT PARSE REQUEST")
                    {
                        ret = PNStatusCategory.PNBadRequestCategory;
                    }
                    break;
                case 401:
                    ret = PNStatusCategory.PNAccessDeniedCategory;
                    break;
                case 402:
                    if (httpErrorCodeMessage.ToUpperInvariant() == "NOT ENABLED")
                    {
                        ret = PNStatusCategory.PNAccessDeniedCategory;
                    }
                    break;
                case 403:
                    if (httpErrorCodeMessage.ToUpperInvariant() == "FORBIDDEN")
                    {
                        ret = PNStatusCategory.PNAccessDeniedCategory;
                    }
                    else if (httpErrorCodeMessage.ToUpperInvariant() == "SIGNATURE DOES NOT MATCH")
                    {
                        ret = PNStatusCategory.PNAccessDeniedCategory;
                    }
                    break;
                case 404:
                case 414:
                case 500:
                    ret = PNStatusCategory.PNBadRequestCategory;
                    break;
                case 502:
                case 503:
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
