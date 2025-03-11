using System;
namespace PubnubApi
{
	public class RetryConfiguration
	{
		public IRetryPolicy RetryPolicy { get; set; }

		public RetryConfiguration(IRetryPolicy retryPolicy)
		{
			this.RetryPolicy = retryPolicy;
		}
		
		public static RetryConfiguration Linear(int delay, int maxRetry) => new RetryConfiguration(new LinearRetryPolicy(delay, maxRetry));

		public static RetryConfiguration Exponential(int minDelay, int maxDelay, int maxRetry) => new RetryConfiguration(new ExponentialRetryPolicy(minDelay, maxDelay, maxRetry));

	}

	public interface IRetryPolicy
	{
		bool ShouldRetry(int attemptedRetries, PNStatus status);
		int GetDelay(int attemptedRetries, PNStatus status, int? retryAfter);
	}

	internal class LinearRetryPolicy : IRetryPolicy
	{
		private int delay;
		private int maxRetry;

		Random numGenerator = new Random();

		public LinearRetryPolicy(int delay, int maxRetry)
		{
			this.delay = Math.Max(2, delay);
			this.maxRetry = Math.Min(10, maxRetry);
		}


		public int GetDelay(int attemptedRetries, PNStatus status, int? retryAfter)
		{
			if (status.StatusCode == 429 && retryAfter.HasValue && retryAfter > 0) return (int)retryAfter;

			return this.delay * 1000 + numGenerator.Next(1000);
		}

		public bool ShouldRetry(int attemptedRetries, PNStatus status)
		{
			return status.StatusCode != 403 && attemptedRetries < this.maxRetry;
		}

		public override string ToString()
		{
			return $"Policy: Linear Delay: {this.delay}, MaxRetry: {this.maxRetry}";
		}
	}

	internal class ExponentialRetryPolicy : IRetryPolicy
	{
		private int minDelay;
		private int maxDelay;
		private int maxRetry;

		Random numGenerator = new Random();
		public ExponentialRetryPolicy(int minDelay, int maxDelay, int maxRetry)
		{
			this.minDelay = Math.Max(2, minDelay);
			this.maxDelay = Math.Min(150, maxDelay);
			this.maxRetry = Math.Min(6, maxRetry);
		}

		public int GetDelay(int attemptedRetries, PNStatus status, int? retryAfter)
		{
			if (status.StatusCode == 429 && retryAfter.HasValue && retryAfter > 0) return (int)retryAfter;
			if (attemptedRetries == 0) return minDelay * 1000 + numGenerator.Next(1000);
			return Math.Min((int)(Math.Pow(2, attemptedRetries) * 1000 + numGenerator.Next(1000)), maxDelay * 1000);
		}

		public bool ShouldRetry(int attemptedRetries, PNStatus status)
		{
			return status.StatusCode != 403 && attemptedRetries < maxRetry;
		}
		public override string ToString()
		{
			return $"Policy: Exponential  MinDelay: {minDelay} MaxRetry: {maxRetry}";
		}
	}
}

