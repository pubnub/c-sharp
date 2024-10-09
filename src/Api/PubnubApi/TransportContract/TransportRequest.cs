using System;
using System.Collections.Generic;
using System.Threading;

namespace PubnubApi
{
	public class TransportRequest
	{
		public string RequestType { get; set; }
		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
		public string RequestUrl { get; set; }
		public byte[] FormData { get; set; } = default;
		public string BodyContentString { get; set; }
		public byte[] BodyContentBytes { get; set; }
		public CancellationToken CancellationToken { get; set; } = default;
		public TimeSpan? Timeout { get; set; } = null;
	}
}