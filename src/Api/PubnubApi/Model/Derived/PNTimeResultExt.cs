using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNTimeResultExt : PNCallback<PNTimeResult>
    {
        Action<PNTimeResult, PNStatus> callbackAction = null;

        public PNTimeResultExt(Action<PNTimeResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNTimeResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
