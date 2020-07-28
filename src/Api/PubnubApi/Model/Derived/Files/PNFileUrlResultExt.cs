using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNFileUrlResultExt : PNCallback<PNFileUrlResult>
    {
        readonly Action<PNFileUrlResult, PNStatus> callbackAction;

        public PNFileUrlResultExt(Action<PNFileUrlResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNFileUrlResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
