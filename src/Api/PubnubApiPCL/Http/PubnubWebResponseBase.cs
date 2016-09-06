using System;
using System.IO;
using System.Net;

namespace PubnubApi
{
    public abstract class PubnubWebResponseBase : WebResponse
    {
        protected WebResponse response { get; set; }
        readonly Stream _responseStream;
        HttpStatusCode httpStatusCode;

        public PubnubWebResponseBase(WebResponse response)
        {
            this.response = response;
        }

        public PubnubWebResponseBase(WebResponse response, HttpStatusCode statusCode)
        {
            this.response = response;
            this.httpStatusCode = statusCode;
        }

        public PubnubWebResponseBase(Stream responseStream)
        {
            _responseStream = responseStream;
        }

        public PubnubWebResponseBase(Stream responseStream, HttpStatusCode statusCode)
        {
            _responseStream = responseStream;
            this.httpStatusCode = statusCode;
        }

        public override Stream GetResponseStream()
        {
            if (response != null)
                return response.GetResponseStream();
            else
                return _responseStream;
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                return response.Headers;
            }
        }

        public override long ContentLength
        {
            get
            {
                return response.ContentLength;
            }
        }

        public override string ContentType
        {
            get
            {
                return response.ContentType;
            }
        }

        public override Uri ResponseUri
        {
            get
            {
                return response.ResponseUri;
            }
        }

        public HttpStatusCode HttpStatusCode
        {
            get
            {
                return httpStatusCode;
            }
        }
    }
}
