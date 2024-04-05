using System;
using System.Collections.Generic;

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

        List<KeyValuePair<string,string>> PresenceActivityList 
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
