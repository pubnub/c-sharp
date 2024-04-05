using Newtonsoft.Json;
using System.Collections.Generic;
using PubnubApi.EventEngine.Subscribe.Context;

namespace PubnubApi.EventEngine.Subscribe.Common
{
    public class SubscriptionCursor
    {
        public long? Timetoken { get; set; }
        public int? Region { get; set; }
    }
    
    public class HandshakeResponse
    {
        [JsonProperty("t")]
        public Timetoken Timetoken { get; set; }

        [JsonProperty("m")]
        public object[] Messages { get; set; }
    }
    public class HandshakeError
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("error")]
        public string ErrorMessage { get; set; }
    }

    public class Timetoken
    {
        [JsonProperty("t")]
        public long Timestamp { get; set; }

        [JsonProperty("r")]
        public int Region { get; set; }

    }

	public class ReceivingResponse<T>
	{
		[JsonProperty("t")]
		public Timetoken Timetoken { get; set; }

		[JsonProperty("m")]
		public Message<T>[] Messages { get; set; }
	}

	public class Message<T>
	{
		[JsonProperty ("a")]
		public string Shard { get; set;}

		[JsonProperty ("b")]
		public string SubscriptionMatch { get; set;}

		[JsonProperty("c")]
		public string Channel { get; set; }

		[JsonProperty("d")]
		public T Payload { get; set; }

		[JsonProperty("e")]
		public int MessageType { get; set; }

		[JsonProperty("f")]
		public string Flags { get; set; }

		[JsonProperty("i")]
		public string IssuingClientId { get; set; }

		[JsonProperty("k")]
		public string SubscribeKey { get; set; }

		[JsonProperty("o")]
		public object OriginatingTimetoken { get; set; }

		[JsonProperty("p")]
		public object PublishMetadata { get; set; }

		[JsonProperty("s")]
		public long SequenceNumber { get; set; }
		
		[JsonProperty("p")]
		public Timetoken Timetoken { get; set; }
	}
	
	public abstract class SubscriptionState : Core.State
	{
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public ReconnectionConfiguration ReconnectionConfiguration;
	}
}