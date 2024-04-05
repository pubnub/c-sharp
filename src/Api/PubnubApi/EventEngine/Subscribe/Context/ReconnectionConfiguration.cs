using System;
namespace PubnubApi.EventEngine.Subscribe.Context
{
	public class ReconnectionConfiguration
	{
        public PNReconnectionPolicy ReconnectionPolicy { get; set; }
        public int MaximumReconnectionRetries { get; set; }

		public ReconnectionConfiguration(PNReconnectionPolicy policy, int maximumReconnectionRetries)
		{
			this.ReconnectionPolicy = policy;
			this.MaximumReconnectionRetries = maximumReconnectionRetries;
		}
	}
}

