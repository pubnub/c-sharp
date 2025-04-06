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
		public Exception Error { get; set; }
		
		public bool IsTimeOut {get; set;}

		public bool IsCancelled { get; set; }
	}
}