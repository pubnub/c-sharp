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
    }
}
