using PubnubApi;
using System;

namespace PubNubMessaging.Tests
{
    public class TestHarness
    {
        protected static Pubnub createPubNubInstance(PNConfiguration pnConfiguration)
        {
            Pubnub pubnub = null;
            if (PubnubCommon.EnableStubTest)
            {
                #pragma warning disable CS0162 // Unreachable code detected
                pnConfiguration.Origin = PubnubCommon.StubOrign;
                #pragma warning restore CS0162 // Unreachable code detected

                IPubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.Timetoken = 1567581283;
                unitTest.RequestId = "myRequestId";
                unitTest.InternetAvailable = true;
                unitTest.SdkVersion = "PubNub-Go/4.2.7";
                unitTest.IncludePnsdk = true;
                unitTest.IncludeUuid = true;

                pubnub = new Pubnub(pnConfiguration);

                pubnub.PubnubUnitTest = unitTest;
            }
            else
            {
                pnConfiguration.Origin = "ps.pndsn.com";// "ingress.bronze.aws-pdx-1.ps.pn";// "ps.pndsn.com";
                //pnConfiguration.Secure = false;
                pubnub = new Pubnub(pnConfiguration);
            }
            return pubnub;
        }
    }
}
