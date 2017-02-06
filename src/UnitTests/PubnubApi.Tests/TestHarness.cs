using PubnubApi;
using System;

namespace PubNubMessaging.Tests
{
    public class TestHarness
    {
        protected Pubnub createPubNubInstance(PNConfiguration pnConfiguration)
        {
            Pubnub pubnub = null;
            if (PubnubCommon.EnableStubTest)
            {
                #pragma warning disable CS0162 // Unreachable code detected
                pnConfiguration.Origin = PubnubCommon.StubOrign;
                #pragma warning restore CS0162 // Unreachable code detected

                IPubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.Timetoken = 1356998400;
                unitTest.RequestId = "myRequestId";
                unitTest.InternetAvailable = true;
                unitTest.SdkVersion = "PubNub CSharp";

                pubnub = new Pubnub(pnConfiguration);

                pubnub.PubnubUnitTest = unitTest;
            }
            else
            {
                pubnub = new Pubnub(pnConfiguration);
            }
            return pubnub;
        }
    }
}
