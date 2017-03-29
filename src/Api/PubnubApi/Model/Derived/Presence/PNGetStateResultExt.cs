using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNGetStateResultExt : PNCallback<PNGetStateResult>
    {
        Action<PNGetStateResult, PNStatus> callbackAction = null;

        public PNGetStateResultExt(Action<PNGetStateResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetStateResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
