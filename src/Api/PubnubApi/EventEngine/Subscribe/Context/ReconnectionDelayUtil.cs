using System;
namespace PubnubApi.EventEngine.Subscribe.Context
{
	public static class ReconnectionDelayUtil
	{
		public static int CalculateDelay(PNReconnectionPolicy policy, int attempts)
		{
			int delayMillisecond = 0;
			switch (policy) {
				case PNReconnectionPolicy.LINEAR:
					delayMillisecond = 3000;
					break;
				case PNReconnectionPolicy.EXPONENTIAL:
					int i = attempts % int.MaxValue;
					if (attempts % 6 == 0) i = attempts + 1;
					delayMillisecond = (int)((Math.Pow(2, (i % 6)) - 1) * 1000);
					break;
			}
			return delayMillisecond;
		}

		public static bool shouldRetry(ReconnectionConfiguration reconnectionConfiguration, int attemptedRetries)
		{
			if (reconnectionConfiguration.ReconnectionPolicy == PNReconnectionPolicy.NONE) return false;
			if (reconnectionConfiguration.MaximumReconnectionRetries < 0) return true;
			return reconnectionConfiguration.MaximumReconnectionRetries >= attemptedRetries;
		}
	}
}

