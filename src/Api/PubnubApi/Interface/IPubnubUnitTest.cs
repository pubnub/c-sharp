using PubnubApi.PubnubEventEngine;
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

        byte[] IV
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

        bool IncludePnsdk
        {
            get;
            set;
        }

        bool IncludeUuid
        {
            get;
            set;
        }

        List<KeyValuePair<string,string>> EventTypeList 
        {
            get;
            set;
        }

        int Attempts
        {
            get;
            set;
        }

    }
}
