using System;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace PubNubMessaging.Core
{
	#region "Logging and error codes -- code split required"

	#if (UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_IOS || UNITY_ANDROID)
	internal class LoggingMethod:MonoBehaviour
	#else
	internal class LoggingMethod
	#endif
	{
		private static int logLevel = 0;
		public static Level LogLevel
		{
			get
			{
				return (Level)logLevel;
			}
			set
			{
				logLevel = (int)value;
			}
		}
		public enum Level
		{
			Off,
			Error,
			Info,
			Verbose,
			Warning
		}

		public static bool LevelError
		{
			get
			{
				return (int)LogLevel >= 1;
			}
		}

		public static bool LevelInfo
		{
			get
			{
				return (int)LogLevel >= 2;
			}
		}

		public static bool LevelVerbose
		{
			get
			{
				return (int)LogLevel >= 3;
			}
		}

		public static bool LevelWarning
		{
			get
			{
				return (int)LogLevel >= 4;
			}
		}

		public static void WriteToLog(string logText, bool writeToLog)
		{
			if (writeToLog)
            {
                #if (SILVERLIGHT || WINDOWS_PHONE || MONOTOUCH || __IOS__ || MONODROID || __ANDROID__ || NETFX_CORE)
                System.Diagnostics.Debug.WriteLine(logText);
				#elif (UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_IOS || UNITY_ANDROID)
				print(logText);
				UnityEngine.Debug.Log (logText);
				#else
				try
				{
					Trace.WriteLine(logText);
					Trace.Flush();
				}
				catch { }
				#endif
			}
		}
	}

	public enum PubnubErrorSeverity
	{
		Critical = 1,
		Warn = 2,
		Info = 3
	}

	public enum PubnubMessageSource
	{
		Server,
		Client
	}

	public class PubnubClientError
	{
		int _statusCode;
		PubnubErrorSeverity _errorSeverity;
		bool _isDotNetException;
		PubnubMessageSource _messageSource;
		string _message = "";
		string _channel = "";
		Exception _detailedDotNetException = null;
		PubnubWebRequest _pubnubWebRequest = null;
		PubnubWebResponse _pubnubWebResponse = null;
		string _description = "";
		DateTime _dateTimeGMT;

		public PubnubClientError()
		{
		}

		public PubnubClientError(int statusCode, PubnubErrorSeverity errorSeverity, bool isDotNetException, string message, Exception detailedDotNetException, PubnubMessageSource source, PubnubWebRequest pubnubWebRequest, PubnubWebResponse pubnubWebResponse, string description, string channel)
		{
			_dateTimeGMT = DateTime.Now.ToUniversalTime();
			_statusCode = statusCode;
			_isDotNetException = isDotNetException;
			_message = message;
			_errorSeverity = errorSeverity;
			_messageSource = source;
			_channel = channel;
			_detailedDotNetException = detailedDotNetException;
			_pubnubWebRequest = pubnubWebRequest;
			_pubnubWebResponse = pubnubWebResponse;
			_description = description;
		}

		public PubnubClientError(int statusCode, PubnubErrorSeverity errorSeverity, string message, PubnubMessageSource source, PubnubWebRequest pubnubWebRequest, PubnubWebResponse pubnubWebResponse, string description, string channel)
		{
			_dateTimeGMT = DateTime.Now.ToUniversalTime();
			_statusCode = statusCode;
			_isDotNetException = false;
			_message = message;
			_errorSeverity = errorSeverity;
			_messageSource = source;
			_channel = channel;
			_detailedDotNetException = null;
			_pubnubWebRequest = pubnubWebRequest;
			_pubnubWebResponse = pubnubWebResponse;
			_description = description;
		}

		public int StatusCode
		{
			get
			{
				return _statusCode;
			}
		}

		public PubnubErrorSeverity Severity
		{
			get
			{
				return _errorSeverity;
			}
		}

		public PubnubMessageSource MessageSource
		{
			get
			{
				return _messageSource;
			}
		}

		public bool IsDotNetException
		{
			get
			{
				return _isDotNetException;
			}
		}

		public string Message
		{
			get
			{
				return _message;
			}
		}

		public Exception DetailedDotNetException
		{
			get
			{
				return _detailedDotNetException;
			}
		}

		public PubnubWebRequest PubnubWebRequest
		{
			get
			{
				return _pubnubWebRequest;
			}
		}

		public PubnubWebResponse PubnubWebResponse
		{
			get
			{
				return _pubnubWebResponse;
			}
		}

		public string Channel
		{
			get
			{
				return _channel;
			}
		}

		public string Description
		{
			get
			{
				return _description;
			}
		}

		public DateTime ErrorDateTimeGMT
		{
			get
			{
				return _dateTimeGMT;
			}
		}

		public override string ToString()
		{
			StringBuilder errorBuilder= new StringBuilder();
			errorBuilder.AppendFormat("StatusCode={0} ", _statusCode);
			errorBuilder.AppendLine();
			errorBuilder.AppendFormat("Severity={0} ", _errorSeverity.ToString());
			errorBuilder.AppendLine();
			errorBuilder.AppendFormat("MessageSource={0} ", _messageSource.ToString());
			errorBuilder.AppendLine();
			errorBuilder.AppendFormat("IsDotNetException={0} ", _isDotNetException.ToString());
			errorBuilder.AppendLine();
			errorBuilder.AppendFormat("Message={0} ", _message);
			errorBuilder.AppendLine();
			errorBuilder.AppendFormat("DetailedDotNetException={0} ", (_detailedDotNetException != null) ? _detailedDotNetException.ToString() : "");
			errorBuilder.AppendLine();
			errorBuilder.AppendFormat("PubnubWebRequest={0} ", (_pubnubWebRequest != null) ? _pubnubWebRequest.ToString() : "");
			errorBuilder.AppendLine();
			errorBuilder.AppendFormat("PubnubWebResponse={0} ", (_pubnubWebResponse != null) ? _pubnubWebResponse.ToString() : "");
			errorBuilder.AppendLine();
			errorBuilder.AppendFormat("Channel={0} ", _channel);
			errorBuilder.AppendLine();
			errorBuilder.AppendFormat("Description={0} ", _description);
			errorBuilder.AppendLine();
			errorBuilder.AppendFormat("ErrorDateTimeGMT={0} ", _dateTimeGMT);
			errorBuilder.AppendLine();

			return errorBuilder.ToString();
		}
	}

	public class PubnubErrorFilter
	{

		private static int errorLevel = 0;
		public static Level ErrorLevel
		{
			get
			{
				return (Level)errorLevel;
			}
			set
			{
				errorLevel = (int)value;
			}
		}

		public enum Level
		{
			Critical =1,
			Warning = 2,
			Info = 3
		}

		public static bool Critical
		{
			get
			{
				return (int)errorLevel >= 1;
			}
		}

		public static bool Warn
		{
			get
			{
				return (int)errorLevel >= 2;
			}
		}
		public static bool Info
		{
			get
			{
				return (int)errorLevel >= 3;
			}
		}


	}

	internal static class PubnubErrorCodeHelper
	{

		public static PubnubErrorCode GetErrorType(WebExceptionStatus webExceptionStatus, string webExceptionMessage)
		{
			PubnubErrorCode ret = PubnubErrorCode.None;
			switch (webExceptionStatus)
            {
                #if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
				case WebExceptionStatus.NameResolutionFailure:
				ret = PubnubErrorCode.NameResolutionFailure;
				break;
				case WebExceptionStatus.ProtocolError:
				ret = PubnubErrorCode.ProtocolError;
				break;
				case WebExceptionStatus.ServerProtocolViolation:
				ret = PubnubErrorCode.ServerProtocolViolation;
				break;
                #endif
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
				if (httpErrorCodeMessage.ToUpper() == "MESSAGE TOO LARGE")
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

	internal enum PubnubErrorCode
	{
		//www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
		None = 0,
		NameResolutionFailure = 103,
		PubnubMessageDecryptException = 104,
		WebRequestCanceled = 105,
		ConnectFailure = 106,
		PubnubObjectDisposedException = 107,
		PubnubSocketConnectException = 108,
		NoInternet = 109,
		YesInternet = 110,
		DuplicateChannel = 111,
		AlreadySubscribed = 112,
		AlreadyPresenceSubscribed = 113,
		PubnubCryptographicException = 114,
		ProtocolError = 115,
		ServerProtocolViolation = 116,
		InvalidChannel = 117,
		NotSubscribed = 118,
		NotPresenceSubscribed = 119,
		UnsubscribeFailed = 120,
		PresenceUnsubscribeFailed = 121,
		NoInternetRetryConnect = 122,
		UnsubscribedAfterMaxRetries = 123,
		PresenceUnsubscribedAfterMaxRetries = 124,
		PublishOperationTimeout = 125,
		HereNowOperationTimeout = 126,
		DetailedHistoryOperationTimeout = 127,
		TimeOperationTimeout = 128,
		PubnubInterOpSEHException = 129,
		PubnubClientMachineSleep = 130,
        SetUserStateTimeout = 131,
        GetUserStateTimeout = 132,
        WhereNowOperationTimeout = 133,
        GlobalHereNowOperationTimeout = 134,
        PAMAccessOperationTimeout = 135,
        UserStateUnchanged = 136,

		MessageTooLarge = 4000,
		BadRequest = 4001,
		InvalidKey = 4002,
        NoUuidSpecified = 4003,
        InvalidTimestamp = 4004,
		InvalidSubscribeKey = 4010,
		PamNotEnabled = 4020,
		Forbidden = 4030,
		SignatureDoesNotMatch = 4031,
        NotFound = 4040,
        RequestUriTooLong = 4140,
		InternalServerError = 5000,
		BadGateway = 5020,
        ServiceUnavailable = 5030,
		GatewayTimeout = 5040
	}

	internal static class PubnubErrorCodeDescription
	{
		private static Dictionary<int, string> dictionaryCodes = new Dictionary<int, string>();

		static PubnubErrorCodeDescription()
		{
			//HTTP ERROR CODES and PubNub Context description
			dictionaryCodes.Add(4000, "If you must publish a message greater than the default of max message size of 1.8K (post-URLEncoded) please enable the elastic message size feature from your admin portal at admin.pubnub.com.");
			dictionaryCodes.Add(4001, "Bad Request. Please check the entered inputs or web request URL");
			dictionaryCodes.Add(4002, "Invalid Key. Please verify your pub and sub keys");
            dictionaryCodes.Add(4003, "No UUID specified. Please ensure that UUID is being passed to server for heartbeat");
            dictionaryCodes.Add(4004, "Invalid Timestamp. Please try again. If the issue continues, please contact PubNub support");
			dictionaryCodes.Add(4010, "Please provide a valid subscribe key");
			dictionaryCodes.Add(4020, "PAM is not enabled for this keyset. Please contact PubNub support for instructions on enabling PAM.");
			dictionaryCodes.Add(4030, "Not authorized. Please ensure that the channel has the correct PAM permission, your authentication key is set correctly, then try again via unsub and re-sub. For further assistance, contact PubNub support.");
			dictionaryCodes.Add(4031, "Please verify pub, sub, and secret keys. For assistance, contact PubNub support");
            dictionaryCodes.Add(4040, "HTTP 404 - Not Found Occured. Please try again. If the issue continues, please contact PubNub support");
			dictionaryCodes.Add(4140, "The URL request too long. Reduce the length by reducing subscription/presence channels or grant/revoke/audit channels/auth key list. Hint: You may spread the load across multiple PubNub instances to prevent this message.");
			dictionaryCodes.Add(5000, "Internal Server Error. Please try again. If the issue continues, please contact PubNub support");
			dictionaryCodes.Add(5020, "Bad Gateway. Please try again. If the issue continues, please contact PubNub support");
            dictionaryCodes.Add(5030, "Service Unavailable. Please try again. If the issue continues, please contact PubNub support");
			dictionaryCodes.Add(5040, "Gateway Timeout. Please try again. If the issue continues, please contact PubNub support");

			//PubNub API ERROR CODES and PubNub Context description
			dictionaryCodes.Add(103, "Please verify origin, host name, and internet connectivity");
			dictionaryCodes.Add(104, "Please verify your cipher key");
			dictionaryCodes.Add(105, "Web Request was cancelled due to change in subsciber/presence channel list or cancelled for object cleaning at the end of Pubnub object session");
			dictionaryCodes.Add(106, "Please check network/internet connection");
			dictionaryCodes.Add(107, "Internal exception. Please ignore"); //This won't go to callback. It will be suppressed.
			dictionaryCodes.Add(108, "Please check network/internet connection");
			dictionaryCodes.Add(109, "No network/internet connection. Please check network/internet connection");
			dictionaryCodes.Add(110, "Network/internet connection is back. Active subscriber/presence channels will be restored.");
			dictionaryCodes.Add(111, "Duplicate channel subscription is not allowed. Internally Pubnub API removes the duplicates before processing");
			dictionaryCodes.Add(112, "Channel Already Subscribed. Duplicate channel subscription not allowed");
			dictionaryCodes.Add(113, "Channel Already Presence-Subscribed. Duplicate channel presence-subscription not allowed");
			dictionaryCodes.Add(114, "Please verify your cipher key");
			dictionaryCodes.Add(115, "Protocol Error. Please contact PubNub with log, use-case, and error details.");
			dictionaryCodes.Add(116, "ServerProtocolViolation. Please contact PubNub with error details.");
			dictionaryCodes.Add(117, "Input contains invalid channel name");
			dictionaryCodes.Add(118, "Channel not subscribed yet");
			dictionaryCodes.Add(119, "Channel not subscribed for presence yet");
			dictionaryCodes.Add(120, "Incomplete unsubscribe. Try again for unsubscribe.");
			dictionaryCodes.Add(121, "Incomplete presence-unsubscribe. Try again for presence-unsubscribe.");
			dictionaryCodes.Add(122, "Network/Internet connection not available. C# client retrying again to verify connection. No action is needed from your side.");
			dictionaryCodes.Add(123, "During non-availability of network/internet, max retries for connection were attempted. So unsubscribed the channel.");
			dictionaryCodes.Add(124, "During non-availability of network/internet, max retries for connection were attempted. So presence-unsubscribed the channel.");
			dictionaryCodes.Add(125, "Publish operation timeout occured.");
			dictionaryCodes.Add(126, "HereNow operation timeout occured.");
			dictionaryCodes.Add(127, "Detailed History operation timeout occured.");
			dictionaryCodes.Add(128, "Time operation timeout occured.");
			dictionaryCodes.Add(129, "Error occured in external component. Please contact PubNub support with full error object details for further investigation");
            dictionaryCodes.Add(130, "Client machine is sleeping. Please check your machine.");
            dictionaryCodes.Add(131, "Timeout occured while setting user state. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(132, "Timeout occured while getting user state. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(133, "Timeout occured while running WhereNow. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(134, "Timeout occured while running GlobalHereNow. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(135, "Timeout occured while running PAM operations. Please try again. If it continues, please contact PubNub support");
            dictionaryCodes.Add(136, "User State Unchanged");
			dictionaryCodes.Add(0, "Undocumented error. Please contact PubNub support with full error object details for further investigation");
		}

		public static string GetStatusCodeDescription(PubnubErrorCode pubnubErrorCode)
		{
			string defaultDescription = "Please contact PubNub support with your error object details";
			int key = (int)pubnubErrorCode;
			string description = dictionaryCodes.ContainsKey(key) ? dictionaryCodes[key] : defaultDescription;
			return description;
		}
	}
	#endregion
}

