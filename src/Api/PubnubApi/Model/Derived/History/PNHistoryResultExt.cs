using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNHistoryResultExt : PNCallback<PNHistoryResult>
    {
        Action<PNHistoryResult, PNStatus> callbackAction = null;

        public PNHistoryResultExt(Action<PNHistoryResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNHistoryResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
