using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNDeleteMessageResultExt : PNCallback<PNDeleteMessageResult>
    {
        Action<PNDeleteMessageResult, PNStatus> callbackAction = null;

        public PNDeleteMessageResultExt(Action<PNDeleteMessageResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNDeleteMessageResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
