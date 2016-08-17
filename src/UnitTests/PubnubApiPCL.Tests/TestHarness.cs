using PubnubApi;
using System;

namespace PubNubMessaging.Tests
{
    public class TestHarness
    {
        protected Pubnub createPubNubInstance(PNConfiguration pnConfiguration)
        {
            pnConfiguration.Origin = PubnubCommon.StubOrign;
            return new MockedTimePubNub(pnConfiguration);
        }

        class MockedTimePubNub : Pubnub
        {
            public MockedTimePubNub(PNConfiguration initialConfig) : base(initialConfig)
            {
            }

            //override public TimeSpan GetTimeStamp()
            //{
            //    return new TimeSpan(new DateTime(2013, 01, 01).Ticks);
            //}
        }
    }
}
