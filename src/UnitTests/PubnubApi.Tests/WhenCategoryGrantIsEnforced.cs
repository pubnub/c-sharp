using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;

namespace PubNubMessaging.Tests
{
    /// <summary>
    /// Runs against a keyset with Get All enforcement enabled, where listing all App Context
    /// metadata requires a category grant.
    /// </summary>
    [TestFixture]
    public class WhenCategoryGrantIsEnforced : TestHarness
    {
        private const int PropagationTimeoutMs = 10000;
        private const int PollIntervalMs = 500;

        private Pubnub admin;
        private readonly List<Pubnub> clients = new List<Pubnub>();

        [SetUp]
        public void Init()
        {
            if (string.IsNullOrEmpty(PubnubCommon.CategoryPublishKey)
                || string.IsNullOrEmpty(PubnubCommon.CategorySubscribeKey)
                || string.IsNullOrEmpty(PubnubCommon.CategorySecretKey))
            {
                Assert.Ignore("PN_CAT_PUB_KEY, PN_CAT_SUB_KEY and PN_CAT_SEC_KEY must be set to run category grant enforcement tests.");
            }

            admin = createPubNubInstance(new PNConfiguration(new UserId(UniqueId("category-admin")))
            {
                PublishKey = PubnubCommon.CategoryPublishKey,
                SubscribeKey = PubnubCommon.CategorySubscribeKey,
                SecretKey = PubnubCommon.CategorySecretKey,
            });
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (Pubnub client in clients)
            {
                try { client.Destroy(); } catch { /* ignore */ }
            }
            clients.Clear();

            if (admin != null)
            {
                admin.Destroy();
                admin = null;
            }
        }

        private static string UniqueId(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

        // The client must not have the secret key, because the secret key bypasses Access Manager.
        private Pubnub CreateClient(string userId, Action<PNConfiguration> configure = null)
        {
            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.CategoryPublishKey,
                SubscribeKey = PubnubCommon.CategorySubscribeKey,
            };
            configure?.Invoke(config);
            Pubnub client = createPubNubInstance(config);
            clients.Add(client);
            return client;
        }

        private async Task<string> GrantTokenAsync(string clientUserId, PNTokenResources resources, PNTokenCategories categories)
        {
            PNResult<PNAccessManagerTokenResult> grant = await admin.GrantToken()
                .TTL(10)
                .AuthorizedUuid(clientUserId)
                .Resources(resources)
                .Categories(categories)
                .ExecuteAsync();
            Assert.That(grant.Status.Error, Is.False, grant.Status.ErrorData?.Information);
            Assert.That(grant.Result?.Token, Is.Not.Null.And.Not.Empty, "GrantToken returned no token");
            return grant.Result.Token;
        }

        private async Task<PNResult<PNAccessManagerGrantResult>> GrantCategoriesAsync(string authKey, bool get)
        {
            PNResult<PNAccessManagerGrantResult> grant = await admin.Grant()
                .AuthKeys(new[] { authKey })
                .Categories(new[] { PNGrantCategory.Channels, PNGrantCategory.Uuids })
                .Get(get)
                .TTL(10)
                .ExecuteAsync();
            Assert.That(grant.Status.Error, Is.False, grant.Status.ErrorData?.Information);
            Assert.That(grant.Result, Is.Not.Null, "Grant returned no result");
            return grant;
        }

        private static Task AssertGetAllChannelMetadata(Pubnub client, bool expectAllowed) =>
            AssertGetAllEventually(async () => (await client.GetAllChannelMetadata().Limit(1).ExecuteAsync()).Status,
                expectAllowed, "GetAllChannelMetadata");

        private static Task AssertGetAllUuidMetadata(Pubnub client, bool expectAllowed) =>
            AssertGetAllEventually(async () => (await client.GetAllUuidMetadata().Limit(1).ExecuteAsync()).Status,
                expectAllowed, "GetAllUuidMetadata");

        // Grants and revokes take effect asynchronously, so retry until the expected outcome or the timeout.
        private static async Task AssertGetAllEventually(Func<Task<PNStatus>> getAll, bool expectAllowed, string operation)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            PNStatus status = await getAll();
            while (!HasExpectedOutcome(status, expectAllowed) && stopwatch.ElapsedMilliseconds < PropagationTimeoutMs)
            {
                await Task.Delay(PollIntervalMs);
                status = await getAll();
            }
            AssertGetAllStatus(status, expectAllowed, operation);
        }

        private static bool HasExpectedOutcome(PNStatus status, bool expectAllowed) =>
            status != null && (expectAllowed
                ? !status.Error && status.StatusCode == 200
                : status.Error && status.StatusCode == 403);

        private static void AssertGetAllStatus(PNStatus status, bool expectAllowed, string operation)
        {
            Assert.That(status, Is.Not.Null, $"{operation} returned no status");
            if (expectAllowed)
            {
                Assert.That(status.Error, Is.False, $"{operation} should be allowed: {status.ErrorData?.Information}");
                Assert.That(status.StatusCode, Is.EqualTo(200), $"{operation} status code");
            }
            else
            {
                Assert.That(status.Error, Is.True, $"{operation} should be denied");
                Assert.That(status.StatusCode, Is.EqualTo(403), $"{operation} status code");
            }
        }

        [Test]
        public async Task ThenTokenWithOnlyResourceLevelGetShouldNotAllowGetAll()
        {
            string clientUserId = UniqueId("category-client");
            string token = await GrantTokenAsync(clientUserId, new PNTokenResources
            {
                Channels = new Dictionary<string, PNTokenAuthValues>
                {
                    { UniqueId("category-channel"), new PNTokenAuthValues { Get = true } }
                },
                Uuids = new Dictionary<string, PNTokenAuthValues>
                {
                    { clientUserId, new PNTokenAuthValues { Get = true } }
                }
            }, null);
            Pubnub client = CreateClient(clientUserId);
            client.SetAuthToken(token);

            await AssertGetAllChannelMetadata(client, false);
            await AssertGetAllUuidMetadata(client, false);
        }

        [Test]
        public async Task ThenTokenWithBothCategoriesShouldAllowGetAll()
        {
            string clientUserId = UniqueId("category-client");
            string token = await GrantTokenAsync(clientUserId, null, new PNTokenCategories
            {
                Channels = new PNTokenCategoryPermissions { Get = true },
                Uuids = new PNTokenCategoryPermissions { Get = true }
            });

            PNTokenContent content = admin.ParseToken(token);
            Assert.That(content.Categories.Channels.Get, Is.True);
            Assert.That(content.Categories.Uuids.Get, Is.True);

            Pubnub client = CreateClient(clientUserId);
            client.SetAuthToken(token);

            await AssertGetAllChannelMetadata(client, true);
            await AssertGetAllUuidMetadata(client, true);
        }

        [Test]
        public async Task ThenTokenWithChannelsCategoryShouldOnlyAllowGetAllChannelMetadata()
        {
            string clientUserId = UniqueId("category-client");
            string token = await GrantTokenAsync(clientUserId, null, new PNTokenCategories
            {
                Channels = new PNTokenCategoryPermissions { Get = true }
            });

            PNTokenContent content = admin.ParseToken(token);
            Assert.That(content.Categories.Channels.Get, Is.True);
            Assert.That(content.Categories.Uuids.Get, Is.False);

            Pubnub client = CreateClient(clientUserId);
            client.SetAuthToken(token);

            await AssertGetAllChannelMetadata(client, true);
            await AssertGetAllUuidMetadata(client, false);
        }

        [Test]
        public async Task ThenGrantWithCategoriesShouldAllowGetAllForAuthKey()
        {
            string authKey = UniqueId("category-auth");

            PNResult<PNAccessManagerGrantResult> grant = await GrantCategoriesAsync(authKey, true);

            Assert.That(grant.Result.Level, Is.EqualTo("category"));
            Assert.That(grant.Result.Categories?.Keys, Is.EquivalentTo(new[] { "channels", "uuids" }));
            Assert.That(grant.Result.Categories["channels"][authKey].GetEnabled, Is.True);
            Assert.That(grant.Result.Categories["uuids"][authKey].GetEnabled, Is.True);

            Pubnub client = CreateClient(UniqueId("category-client"), config => config.AuthKey = authKey);

            await AssertGetAllChannelMetadata(client, true);
            await AssertGetAllUuidMetadata(client, true);
        }

        [Test]
        public async Task ThenRevokingCategoryGrantShouldDenyGetAll()
        {
            string authKey = UniqueId("category-auth");
            await GrantCategoriesAsync(authKey, true);
            Pubnub client = CreateClient(UniqueId("category-client"), config => config.AuthKey = authKey);
            await AssertGetAllChannelMetadata(client, true);
            await AssertGetAllUuidMetadata(client, true);

            PNResult<PNAccessManagerGrantResult> revoke = await GrantCategoriesAsync(authKey, false);

            Assert.That(revoke.Result.Categories?.Keys, Is.EquivalentTo(new[] { "channels", "uuids" }));
            Assert.That(revoke.Result.Categories["channels"][authKey].GetEnabled, Is.False);
            Assert.That(revoke.Result.Categories["uuids"][authKey].GetEnabled, Is.False);

            await AssertGetAllChannelMetadata(client, false);
            await AssertGetAllUuidMetadata(client, false);
        }

        [Test]
        public async Task ThenOneMinuteCategoryGrantShouldBeRejectedByServer()
        {
            PNResult<PNAccessManagerGrantResult> grant = await admin.Grant()
                .AuthKeys(new[] { UniqueId("category-auth") })
                .Categories(new[] { PNGrantCategory.Channels })
                .Get(true)
                .TTL(1)
                .ExecuteAsync();

            Assert.That(grant.Status.Error, Is.True, "One-minute category grants should be rejected");
            Assert.That(grant.Status.StatusCode, Is.EqualTo(400));
        }
    }
}
