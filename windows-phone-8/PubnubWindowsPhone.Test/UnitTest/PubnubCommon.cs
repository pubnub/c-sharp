using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubWindowsPhone.Test.UnitTest
{
    public static class PubnubCommon
    {
        public const bool PAMEnabled = true;
        public const bool EnableStubTest = true;

        public static readonly string PublishKey = "demo-36";
        public static readonly string SubscribeKey = "demo-36";
        public static readonly string SecretKey = "demo-36";


        static PubnubCommon()
        {
            if (PAMEnabled && !EnableStubTest)
            {
                PublishKey = "pub-c-38994634-9e05-4967-bc66-2ac2cef65ed9";
                SubscribeKey = "sub-c-c9710928-1b7a-11e3-a0c8-02ee2ddab7fe";
                SecretKey = "sec-c-ZDkzZTBkOTEtNTQxZS00MmQ3LTljMWUtMTNiNGZjNWUwMTVk";
            }
        }
    }
}
