
using System;
using System.Net;
using System.Text;

namespace PubnubApi
{
    public class PubnubClientError
    {
        int _statusCode;
        PubnubErrorSeverity _errorSeverity;
        bool _isDotNetException;
        PubnubMessageSource _messageSource;
        string _message = "";
        string _channel = "";
        string _channelGroup = "";
        Exception _detailedDotNetException = null;
        HttpWebRequest _pubnubWebRequest = null;
        HttpWebResponse _pubnubWebResponse = null;
        string _description = "";
        DateTime _dateTimeGMT;

        public PubnubClientError()
        {
        }

        public PubnubClientError(int statusCode, PubnubErrorSeverity errorSeverity, bool isDotNetException, string message, Exception detailedDotNetException, PubnubMessageSource source, HttpWebRequest pubnubWebRequest, HttpWebResponse pubnubWebResponse, string description, string channel, string channelGroup)
        {
            _dateTimeGMT = DateTime.Now.ToUniversalTime();
            _statusCode = statusCode;
            _isDotNetException = isDotNetException;
            _message = message;
            _errorSeverity = errorSeverity;
            _messageSource = source;
            _channel = channel;
            _channelGroup = channelGroup;
            _detailedDotNetException = detailedDotNetException;
            _pubnubWebRequest = pubnubWebRequest;
            _pubnubWebResponse = pubnubWebResponse;
            _description = description;
        }

        public PubnubClientError(int statusCode, PubnubErrorSeverity errorSeverity, string message, PubnubMessageSource source, HttpWebRequest pubnubWebRequest, HttpWebResponse pubnubWebResponse, string description, string channel, string channelGroup)
        {
            _dateTimeGMT = DateTime.Now.ToUniversalTime();
            _statusCode = statusCode;
            _isDotNetException = false;
            _message = message;
            _errorSeverity = errorSeverity;
            _messageSource = source;
            _channel = channel;
            _channelGroup = channelGroup;
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

        public HttpWebRequest PubnubWebRequest
        {
            get
            {
                return _pubnubWebRequest;
            }
        }

        public HttpWebResponse PubnubWebResponse
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

        public string ChannelGroup
        {
            get
            {
                return _channelGroup;
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
            StringBuilder errorBuilder = new StringBuilder();
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
            errorBuilder.AppendFormat("HttpWebRequest={0} ", (_pubnubWebRequest != null) ? _pubnubWebRequest.ToString() : "");
            errorBuilder.AppendLine();
            errorBuilder.AppendFormat("HttpWebResponse={0} ", (_pubnubWebResponse != null) ? _pubnubWebResponse.ToString() : "");
            errorBuilder.AppendLine();
            errorBuilder.AppendFormat("Channel={0} ", (_channel != null) ? _channel : "");
            errorBuilder.AppendLine();
            errorBuilder.AppendFormat("ChannelGroup={0} ", (_channelGroup != null) ? _channelGroup : "");
            errorBuilder.AppendLine();
            errorBuilder.AppendFormat("Description={0} ", _description);
            errorBuilder.AppendLine();
            errorBuilder.AppendFormat("ErrorDateTimeGMT={0} ", _dateTimeGMT);
            errorBuilder.AppendLine();

            return errorBuilder.ToString();
        }
    }
}
