using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNHistoryResultExt : PNCallback<PNHistoryResult>
    {
        readonly Action<PNHistoryResult, PNStatus> callbackAction;

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
