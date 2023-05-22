using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PubnubApi;
using PubnubApi.EndPoint;

namespace PubnubApi.Unity {
	public static class SubscribeOperationExtensions {
		public static SubscribeOperation<T> Channels<T>(this SubscribeOperation<T> so, List<string> channels) =>
			so.Channels(channels.ToArray());
		
		public static SubscribeOperation<string> Channels(this SubscribeOperation<string> so, List<string> channels) =>
			so.Channels<string>(channels);
		
		public static SubscribeOperation<T> ChannelGroups<T>(this SubscribeOperation<T> so, List<string> channels) =>
			so.ChannelGroups(channels.ToArray());
		
		public static SubscribeOperation<string> ChannelGroups(this SubscribeOperation<string> so, List<string> channels) =>
			so.ChannelGroups<string>(channels);

		
		// TODO This copies the dictionary, need to reconsider
		public static SubscribeOperation<string> QueryParam(this SubscribeOperation<string> so,
			Dictionary<string, string> customQueryParam) => so.QueryParam(customQueryParam.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as object));
	}
}