using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PubnubApi
{
	public class SubscriptionCursor
	{
		public long? Timetoken { get; set; }
		public int? Region { get; set; }
		public SubscriptionCursor() {}
		public SubscriptionCursor(long? timetoken, int? region = null)
		{
			this.Timetoken = timetoken;
			this.Region = region;
		}

		public override string ToString() => $"tt= {Timetoken}, region={Region}";
	}
}

namespace PubnubApi.EventEngine.Subscribe.Common
{
	public class HandshakeResponse
	{
		[JsonPropertyName("t")]
		public Timetoken Timetoken { get; set; }

        [JsonPropertyName("m")]
        public object[] Messages { get; set; }
    }
    public class HandshakeError
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("error")]
        public string ErrorMessage { get; set; }
    }

    public class Timetoken
    {
        [JsonPropertyName("t")]
        public long Timestamp { get; set; }

        [JsonPropertyName("r")]
        public int Region { get; set; }

    }

	public class ReceivingResponse<T>
	{
		[JsonPropertyName("t")]
		public Timetoken Timetoken { get; set; }

		[JsonPropertyName("m")]
		public Message<T>[] Messages { get; set; }
	}

	public class Message<T>
	{
		[JsonPropertyName("a")]
		public string Shard { get; set;}

		[JsonPropertyName("b")]
		public string SubscriptionMatch { get; set;}

		[JsonPropertyName("c")]
		public string Channel { get; set; }

		[JsonPropertyName("d")]
		public T Payload { get; set; }

		[JsonPropertyName("e")]
		public int MessageType { get; set; }

		[JsonPropertyName("f")]
		public string Flags { get; set; }

		[JsonPropertyName("i")]
		public string IssuingClientId { get; set; }

		[JsonPropertyName("k")]
		public string SubscribeKey { get; set; }

		[JsonPropertyName("o")]
		public object OriginatingTimetoken { get; set; }

		[JsonPropertyName("p")]
		public object PublishMetadata { get; set; }

		[JsonPropertyName("s")]
		public long SequenceNumber { get; set; }
		
		[JsonPropertyName("u")]
		public object UserMetadata { get; set; }

		[JsonPropertyName("cmt")]
		public string CustomMessageType { get; set; }
	}
	
	public abstract class SubscriptionState : Core.State
	{
		public IEnumerable<string> Channels;
		public IEnumerable<string> ChannelGroups;
		public SubscriptionCursor Cursor;
	}
}