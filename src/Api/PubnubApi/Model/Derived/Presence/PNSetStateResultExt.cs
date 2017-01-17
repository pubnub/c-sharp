using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNSetStateResultExt : PNCallback<PNSetStateResult>
    {
        Action<PNSetStateResult, PNStatus> callbackAction = null;

        public PNSetStateResultExt(Action<PNSetStateResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNSetStateResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
