using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace PubnubApi
{
    public interface IPubnubUnitTest
    {
        bool EnableStubTest
        {
            get;
            set;
        }

        string TestClassName
        {
            get;
            set;
        }

        string TestCaseName
        {
            get;
            set;
        }

        string GetStubResponse(HttpWebRequest request);
    }
}
