using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApiPCL.Silverlight50.Tests
{
    public static class PubnubCommon
    {
        /// <summary>
        /// Thread.Sleep Timeout in millisecond
        /// </summary>
        public const int TIMEOUT = 500;
		public const bool PAMEnabled = true;
		public const bool EnableStubTest = false;

        public static readonly string PublishKey = "Demo-36";
        public static readonly string SubscribeKey = "Demo-36";
        public static readonly string SecretKey = "Demo-36";

        static PubnubCommon()
        {
            if (PAMEnabled && !EnableStubTest)
            {
                PublishKey = "pam";
                SubscribeKey = "pam";
                SecretKey = "pam";
            }
        }
    }
}
