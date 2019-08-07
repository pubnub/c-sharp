using System;
using System.Collections.Generic;

namespace PubnubApi
{
    internal static class PNStatusCodeHelper
    {
        public static int GetHttpStatusCode(string error)
        {
            int ret = 0;
            switch (error.ToLowerInvariant())
            {
                case "badrequest":
                    ret = 400;
                    break;
                case "notfound":
                case "not_found":
                    ret = 404;
                    break;
                case "conflict":
                    ret = 409;
                    break;
                case "internal":
                    ret = 500;
                    break;
                default:
                    break;
            }
            return ret;
        }

    }
}
