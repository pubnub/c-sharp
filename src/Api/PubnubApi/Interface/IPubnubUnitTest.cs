using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace PubnubApi
{
    public interface IPubnubUnitTest
    {
        long Timetoken
        {
            get;
            set;
        }

        string RequestId
        {
            get;
            set;
        }

        bool InternetAvailable
        {
            get;
            set;
        }

        string SdkVersion
        {
            get;
            set;
        }
    }
}
