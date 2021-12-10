﻿using PubnubApi;

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
    }
}
