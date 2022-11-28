using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal static class PNPublishFileMessageJsonDataParse
    {
        internal static PNPublishFileMessageResult GetObject(List<object> listObject)
        {
            PNPublishFileMessageResult result = null;

            if (listObject.Count >= 2)
            {
                long publishTimetoken;
                var _ = Int64.TryParse(listObject[2].ToString(), out publishTimetoken);
                result = new PNPublishFileMessageResult
                {
                    Timetoken = publishTimetoken
                };
            }

            return result;
        }
    }
}
