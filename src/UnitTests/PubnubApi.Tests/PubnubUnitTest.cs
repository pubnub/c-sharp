using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PubnubApi;
using System.IO;
using System.Net;


namespace PubNubMessaging.Tests
{
    public class PubnubUnitTest : IPubnubUnitTest
    {
        private long pubnubTimetoken;
        private string requestId;
        private bool internetAvailable;
        private string sdkVersion;

        long IPubnubUnitTest.Timetoken
        {
            get
            {
                return pubnubTimetoken;
            }

            set
            {
                pubnubTimetoken = value;
            }
        }

        string IPubnubUnitTest.RequestId
        {
            get
            {
                return requestId;
            }
            set
            {
                requestId = value;
            }
        }

        bool IPubnubUnitTest.InternetAvailable
        {
            get
            {
                return internetAvailable;
            }

            set
            {
                internetAvailable = value;
            }
        }

        string IPubnubUnitTest.SdkVersion
        {
            get
            {
                return sdkVersion;
            }

            set
            {
                sdkVersion = value;
            }
        }
    }
}
