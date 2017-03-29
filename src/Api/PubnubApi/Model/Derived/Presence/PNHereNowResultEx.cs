using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNHereNowResultEx : PNCallback<PNHereNowResult>
    {
        Action<PNHereNowResult, PNStatus> callbackAction = null;

        public PNHereNowResultEx(Action<PNHereNowResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNHereNowResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
