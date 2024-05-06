using System;
using System.Linq;
using PubnubApi.EventEngine.Common;

namespace PubnubApi
{

	class SubscriptionSet
	{
		public SubscriptionSet()
		{
			// Ctor with Channels and channelGroups 
		}

		SubscriptionSet AddSubscription(Subscription subscription)
		{
			// CRUD
			return this;
		}

		SubscriptionSet RemoveSubscription(Subscription subscription)
		{
			// CRUD
			return this;
		}
	}
}