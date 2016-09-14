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
                pubnub = new Pubnub(pnConfiguration, unitTest);
                ////pubnub.PubnubUnitTest = unitTest;
                //// We need to change this method in PubnubWebRequestBase.cs #line 187
                //// to return only request.BeginGetResponse(callback, state);
                ////
                ////public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
                ////{
                ////    if (pubnubUnitTest is IPubnubUnitTest && pubnubUnitTest.EnableStubTest)
                ////    {
                ////        return new PubnubWebAsyncResult(callback, state);
                ////    }
                ////    else if (machineSuspendMode)
                ////    {
                ////        return new PubnubWebAsyncResult(callback, state);
                ////    }
                ////    else
                ////    {
                ////        return request.BeginGetResponse(callback, state);
                ////    }
                ////}


            }
            else
            {
                pubnub = new Pubnub(pnConfiguration);
            }
            return pubnub;
        }
    }
}
