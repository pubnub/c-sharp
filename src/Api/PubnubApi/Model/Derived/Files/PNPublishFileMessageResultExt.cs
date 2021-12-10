using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNPublishFileMessageResultExt : PNCallback<PNPublishFileMessageResult>
    {
        readonly Action<PNPublishFileMessageResult, PNStatus> callbackAction;

        public PNPublishFileMessageResultExt(Action<PNPublishFileMessageResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNPublishFileMessageResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
