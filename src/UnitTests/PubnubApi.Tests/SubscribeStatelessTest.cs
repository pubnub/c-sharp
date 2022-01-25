using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi;
using System.Threading.Tasks;
using System.Threading;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class SubscribeStatelessTest: TestHarness
    {
        static Pubnub pubnub = null;

        [SetUp]
        public static void Init()
        {
            pubnub = new Pubnub(null);
        }

        [TearDown]
        public static void Exit()
        {
        }

        [Test]
        public static async Task RunSubscribeStatelessTest()
        {
            //Hard coded data
            List<string> channelList = new List<string>() { "ch1", "ch2" };
            List<string> channelgroupList = new List<string>() { "cg1", "cg2" };

            PubnubApi.EndPoint.StatelessSubscribeOperation statelessSubscribeOp = new PubnubApi.EndPoint.StatelessSubscribeOperation();
            statelessSubscribeOp.HandshakeCompleted += StatelessSubscribeOp_HandshakeCompleted;
            CancellationTokenSource cancellationToken = await statelessSubscribeOp.Handshake(channelList.ToArray(), channelgroupList.ToArray(), statelessSubscribeOp.HandshakeDataCallback);

            //CancellationTokenSource cancellationToken = await statelessSubscribeOp.Execute();
        }

        private static void StatelessSubscribeOp_HandshakeCompleted(object sender, PubnubApi.EndPoint.HandshakeCompletedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("HandshakeReceived: timetoken={0}; region={1}", e.Timetoken, e.Region));

            CallReceiveMessages((string[])e.HandshakeInputs["channels"], (string[])e.HandshakeInputs["channel-groups"], e.Timetoken, e.Region);
        }

        public static void CallReceiveMessages(string[] rawChannels, string[] rawChannelGroups, long timetoken, int region)
        {
            PubnubApi.EndPoint.StatelessSubscribeOperation statelessSubscribeOp = new PubnubApi.EndPoint.StatelessSubscribeOperation();
            statelessSubscribeOp.MessageReceiveCompleted += StatelessSubscribeOp_MessageReceiveCompleted;
            CancellationTokenSource cancellationToken = statelessSubscribeOp.ReceiveMessages(rawChannels, rawChannelGroups, timetoken, region, statelessSubscribeOp.ReceiveMessagesCallback).Result;
        }

        private static void StatelessSubscribeOp_MessageReceiveCompleted(object sender, PubnubApi.EndPoint.MessageReceivedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("MessageReceiveCompleted: timetoken={0}; region={1}", e.Timetoken, e.Region));

            CallReceiveMessages((string[])e.ReceiveMessageInputs["channels"], (string[])e.ReceiveMessageInputs["channel-groups"], e.Timetoken, e.Region);
        }
    }
}
