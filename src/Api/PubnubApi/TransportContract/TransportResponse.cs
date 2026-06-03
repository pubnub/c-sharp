using System;
using System.Collections.Generic;

namespace PubnubApi
{
	public class TransportResponse
	{
		public int StatusCode { get; set; }
		public byte[] Content { get; set; }
		public Dictionary<string, IEnumerable<string>> Headers { get; set; }
		public string RequestUrl { get; set; }

		/// <summary>
		/// The negotiated HTTP protocol version of the response (e.g. 2.0 or 1.1),
		/// taken from HttpResponseMessage.Version. Null when no response was received.
		/// </summary>
		public Version NegotiatedProtocolVersion { get; set; }
		public Exception Error { get; set; }
		
		public bool IsTimeOut {get; set;}

		public bool IsCancelled { get; set; }
	}
}