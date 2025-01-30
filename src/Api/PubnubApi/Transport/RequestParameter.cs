using System.Collections.Generic;

namespace PubnubApi
{
	public class RequestParameter
	{
		public string RequestType { get; set; }
		public List<string> PathSegment { get; set; }
		public Dictionary<string, string> Query { get; set; } = new Dictionary<string, string>();
		public string BodyContentString { get; set; }
		public byte[] FormData { get; set; }

		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
	}
}
