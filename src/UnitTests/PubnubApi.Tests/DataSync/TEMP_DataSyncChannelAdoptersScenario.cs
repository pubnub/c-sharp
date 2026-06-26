using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.EndPoint;
using PubNubMessaging.Tests;

namespace PubnubApi.Tests.DataSync
{
    // =====================================================================================
    // Flow:
    //   1. GetChannel(adopters_channel)
    //   2. CreateChannel(adopters_channel)
    //   3. wait a few seconds
    //   4. GetChannel(adopters_channel)
    //   5. CreateMembership(adopters_channel, MembershipUserId)
    //
    // Every operation logs PASS / FAIL (with HTTP status, category and reason).
    // Run with detailed output, e.g.:
    //   dotnet test --filter "FullyQualifiedName~TEMP_DataSyncChannelAdoptersScenario" -l "console;verbosity=detailed"
    // =====================================================================================
    [TestFixture]
    [Category("TemporaryPamDebug")]
    [Explicit("Temporary manual DataSync channel-adoption harness.")]
    public class TEMP_DataSyncChannelAdoptersScenario : TestHarness
    {
        // ----------------------------------------------------------------------------------
        // Manual inputs - paste the values to test here.
        // ----------------------------------------------------------------------------------
        private const string AuthToken = "qEF2AkF0Gmo-b-tDdHRsGQFoQ3Jlc6VEY2hhbqBDZ3JwoENzcGOgQ3VzcqBEdXVpZKBDcGF0qERjaGFuoWIuKhj_Q2dycKBDc3BjoEN1c3KhYi4qGP9EdXVpZKBRZGF0YXN5bmM6ZW50aXRpZXOhYi4qGP9WZGF0YXN5bmM6cmVsYXRpb25zaGlwc6FiLioY_1RkYXRhc3luYzptZW1iZXJzaGlwc6FiLioY_0RtZXRhoER1dWlkbWNhcHliYXJhX2dhbWVDc2lnWCBrVNk7Ai8TPj22MJMhvFCPLVKFmb6fVQ9E_-nsnr7JpA==";
        private const string MembershipUserId = "KYLER";

        // UserId used for the client PNConfiguration. The auth token defines the actual
        // authorized user; this only needs to be a valid non-empty identifier.
        private const string ClientUserId = "capybara_game";

        private const string ChannelId = "adopters_channel";
        private const int EntityClassVersion = 1;
        private const int RelationshipClassVersion = 1;

        private Pubnub client;

        [SetUp]
        public void Init()
        {
            client = new Pubnub(new PNConfiguration(new UserId(ClientUserId))
            {
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
                PublishKey = PubnubCommon.DataSyncPublishKey,
                Origin = PubnubCommon.DataSyncOrigin
            });
            client.SetAuthToken(AuthToken);
        }

        [TearDown]
        public void Cleanup()
        {
            if (client != null)
            {
                try { client.Destroy(); } catch { /* ignore */ }
                client = null;
            }
        }

        [Test]
        public async Task ChannelAdoptionFlow()
        {
            Log("===== Channel adoption flow =====");
            Log($"  ChannelId={ChannelId}, MembershipUserId={Show(MembershipUserId)}");

            await RunOp("GetChannel (before create)", () => client.DataSync.GetChannel(new GetChannelParameters
            {
                Id = ChannelId
            }),
            res => $"Id={Show(res.Id)}, Status={Show(res.Status)}");

            await RunOp("CreateChannel", () => client.DataSync.CreateChannel(new CreateChannelParameters
            {
                Id = ChannelId,
                EntityClassVersion = EntityClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "name", "adopters-channel" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            }),
            res => $"Id={Show(res.Id)}, Status={Show(res.Status)}");

            Log("  Waiting 5s for propagation...");
            await Task.Delay(5000);

            await RunOp("GetChannel (after create)", () => client.DataSync.GetChannel(new GetChannelParameters
            {
                Id = ChannelId
            }),
            res => $"Id={Show(res.Id)}, Status={Show(res.Status)}");

            await RunOp("CreateMembership", () => client.DataSync.CreateMembership(new CreateMembershipParameters
            {
                ChannelId = ChannelId,
                UserId = MembershipUserId,
                RelationshipClassVersion = RelationshipClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "role", "member" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            }),
            res => $"Id={Show(res.Id)}, UserId={Show(res.UserId)}, ChannelId={Show(res.ChannelId)}");
        }

        // ----------------------------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------------------------

        private static async Task RunOp<T>(string name, Func<Task<PNResult<T>>> action,
            Func<T, string> resultDetails = null)
        {
            try
            {
                var res = await action();
                var status = res?.Status;
                if (status != null && !status.Error)
                {
                    var details = resultDetails != null && res.Result != null
                        ? $"; {resultDetails(res.Result)}"
                        : string.Empty;
                    Log($"  [PASS] {name} -> HTTP {status.StatusCode}{details}");
                }
                else
                {
                    Log($"  [FAIL] {name} -> HTTP {status?.StatusCode}; category={status?.Category}; " +
                        $"reason={status?.ErrorData?.Information}");
                }
            }
            catch (Exception ex)
            {
                Log($"  [ERROR] {name} -> {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static string Show(string value) => value == null ? "<null>" : $"\"{value}\"";

        private static void Log(string message)
        {
            // NUnit captures Console output into the same per-test buffer that
            // TestContext writes to, so use only one sink to avoid duplicate lines.
            TestContext.WriteLine(message);
        }
    }
}
