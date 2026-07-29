using NUnit.Framework;
using PubnubApi;

namespace PubnubApi.Tests.EventEngine
{
    internal class RetryConfigurationTest
    {
        private static PNStatus RetryableStatus() =>
            new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNUnexpectedDisconnectCategory);

        // ---- Linear policy: maxRetry cap (previously Math.Min(10, maxRetry)) is removed ----

        [Test]
        public void Linear_MaxRetryAboveOldCap_IsHonored()
        {
            // maxRetry = 20 was previously clamped to 10; it must now be respected.
            var policy = RetryConfiguration.Linear(5, 20).RetryPolicy;

            // At attempt 15 (which the old cap of 10 would have rejected) it should still retry.
            Assert.IsTrue(policy.ShouldRetry(15, RetryableStatus()));
            // At attempt 19 it should still retry, at 20 it should stop (attemptedRetries < maxRetry).
            Assert.IsTrue(policy.ShouldRetry(19, RetryableStatus()));
            Assert.IsFalse(policy.ShouldRetry(20, RetryableStatus()));
        }

        [Test]
        public void Linear_ForbiddenStatus_NeverRetries()
        {
            var policy = RetryConfiguration.Linear(5, 20).RetryPolicy;
            // A non-null exception is required so PNStatus keeps the 403 (Error must be true).
            var forbidden = new PNStatus(new System.Exception("forbidden"), PNOperationType.PNSubscribeOperation, PNStatusCategory.PNAccessDeniedCategory, null, null, 403);
            Assert.IsFalse(policy.ShouldRetry(0, forbidden));
        }

        // ---- Exponential policy: maxRetry cap (previously Math.Min(6, maxRetry)) is removed ----

        [Test]
        public void Exponential_MaxRetryAboveOldCap_IsHonored()
        {
            // maxRetry = 15 was previously clamped to 6; it must now be respected.
            var policy = RetryConfiguration.Exponential(2, 150, 15).RetryPolicy;

            // At attempt 10 (which the old cap of 6 would have rejected) it should still retry.
            Assert.IsTrue(policy.ShouldRetry(10, RetryableStatus()));
            Assert.IsTrue(policy.ShouldRetry(14, RetryableStatus()));
            Assert.IsFalse(policy.ShouldRetry(15, RetryableStatus()));
        }

        [Test]
        public void Exponential_ForbiddenStatus_NeverRetries()
        {
            var policy = RetryConfiguration.Exponential(2, 150, 15).RetryPolicy;
            // A non-null exception is required so PNStatus keeps the 403 (Error must be true).
            var forbidden = new PNStatus(new System.Exception("forbidden"), PNOperationType.PNSubscribeOperation, PNStatusCategory.PNAccessDeniedCategory, null, null, 403);
            Assert.IsFalse(policy.ShouldRetry(0, forbidden));
        }
    }
}
