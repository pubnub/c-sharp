﻿using System;
namespace PubnubApi.EventEngine.Subscribe.Context
{
	public static class ReconnectionDelayUtil
	{
		public static int CalculateDelay(PNReconnectionPolicy policy, int attempts)
		{
			Random numGenerator = new Random();
			int delayValue = 0;
			int backoff = 5;
			switch (policy) {
				case PNReconnectionPolicy.LINEAR:
					delayValue = attempts * backoff + numGenerator.Next(1000);
					break;
				case PNReconnectionPolicy.EXPONENTIAL:
					delayValue = (int)(Math.Pow(2, attempts - 1) * 1000 + numGenerator.Next(1000));
					break;
			}
			return delayValue;

		}

		public static bool shouldRetry(PNReconnectionPolicy policy, int attempts, int maxAttempts)
		{
			if (policy == PNReconnectionPolicy.NONE) return false;
			return maxAttempts < attempts;
		}
	}
}

