using System;
namespace PubnubApi.EventEngine.Subscribe.Context
{
	public class ReconnectionConfiguration
	{
		public PNReconnectionPolicy ReconnectionPolicy { get; set; } = PNReconnectionPolicy.NONE;
		public int MaximumReconnectionRetries;

		public ReconnectionConfiguration(PNReconnectionPolicy policy, int maximumReconnectionRetries)
		{
			this.ReconnectionPolicy = policy;
			this.MaximumReconnectionRetries = maximumReconnectionRetries;
		}
	}

    public interface IRetryPolicy
    {
        bool ShouldRetry(int attemptedRetries, PNStatus status);
        int GetDelay(int attempt);
    }

    public class LinearRetryPolicy: IRetryPolicy
    {
        private int delay;
        private int maxRetry;

		Random numGenerator = new Random();

        public LinearRetryPolicy(int delay, int maxRetry)
        {
            this.delay = delay;
            this.maxRetry = maxRetry;
        }


		public int GetDelay(int attempt)
		{
			return this.delay * 1000 + numGenerator.Next(1000);
		}

		public bool ShouldRetry(int attemptedRetries, PNStatus status)
		{
			if (status.StatusCode == 403) return false;
			return this.maxRetry > attemptedRetries;
		}
	}

    public class ExponentialRetryPolicy: IRetryPolicy
    {
        private int minDelay;
        private int maxDelay;
        private int maxRetry;

		Random numGenerator = new Random();
        public ExponentialRetryPolicy(int minDelay, int maxDelay, int maxRetry)
        {
            this.minDelay = minDelay;
            this.maxRetry = maxRetry;
            this.maxDelay = maxDelay;
        }

		public int GetDelay(int attempt)
		{
			if (attempt == 0) return this.minDelay * 1000 + numGenerator.Next(1000);
			return Math.Min((int)(Math.Pow(2, attempt) * 1000 + numGenerator.Next(1000)), this.maxDelay * 1000);
		}

		public bool ShouldRetry(int attemptedRetries, PNStatus status)
		{
			if (status.StatusCode == 403) return false;
			return this.maxRetry > attemptedRetries;
		}
	}
}

