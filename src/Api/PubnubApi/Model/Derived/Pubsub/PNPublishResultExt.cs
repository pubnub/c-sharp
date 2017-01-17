using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNPublishResultExt: PNCallback<PNPublishResult>
    {
        Action<PNPublishResult, PNStatus> callbackAction = null;

        public PNPublishResultExt(Action<PNPublishResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNPublishResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
