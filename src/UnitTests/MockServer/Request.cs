using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MockServer
{
    public class Request
    {
        internal string Path { get; set; }

        internal string Response { get; set; }

        internal HttpStatusCode StatusCode { get; set; }

        internal string Method { get; set; }

        internal List<string> Parameters { get; set; }

        public Request()
        {
            Parameters = new List<string>();
        }

        /// <summary>
        /// Add a path to Request List
        /// </summary>
        /// <param name="path">Path string</param>
        /// <returns>this</returns>
        public Request WithPath(string path)
        {
            this.Path = path;
            return this;
        }

        /// <summary>
        /// Add a parameter
        /// </summary>
        /// <param name="parameter">Parameter</param>
        /// <param name="value">Value</param>
        /// <returns>this</returns>
        public Request WithParameter(string parameter, string value)
        {
            Parameters.Add(String.Format("{0}={1}", parameter, value));
            return this;
        }

        /// <summary>
        /// Request Method like: GET, POST, DELETE
        /// </summary>
        /// <param name="method">Method</param>
        /// <returns>this</returns>
        public Request WithMethod(string method)
        {
            this.Method = method;
            return this;
        }

        /// <summary>
        /// Response for Request
        /// </summary>
        /// <param name="response">Response string</param>
        /// <returns>this</returns>
        public Request WithResponse(string response)
        {
            this.Response = response;
            return this;
        }

        /// <summary>
        /// Return with HttpStatusCode
        /// </summary>
        /// <param name="statusCode">HttpStatusCode</param>
        /// <returns>this</returns>
        public Request WithStatusCode(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
            return this;
        }
    }
}
