using PubnubApi;
using System;
using System.Collections.Generic;

namespace PubNubMessaging.Tests
{
    public class PubnubUnitTest : IPubnubUnitTest
    {
        long IPubnubUnitTest.Timetoken
        {
            get;
            set;
        }

        string IPubnubUnitTest.RequestId
        {
            get;
            set;
        }

        byte[] IPubnubUnitTest.IV
        {
            get;
            set;
        }

        bool IPubnubUnitTest.InternetAvailable
        {
            get;
            set;
        }

        string IPubnubUnitTest.SdkVersion
        {
            get;
            set;
        }

        bool IPubnubUnitTest.IncludePnsdk
        {
            get;
            set;
        }

        bool IPubnubUnitTest.IncludeUuid
        {
            get;
            set;
        }
        List<KeyValuePair<string, string>> IPubnubUnitTest.EventTypeList
        {
            get;
            set;
        }
        int IPubnubUnitTest.Attempts
        {
            get;
            set;
        }
    }
}
