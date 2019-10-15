using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNFetchHistoryResultExt : PNCallback<PNFetchHistoryResult>
    {
        readonly Action<PNFetchHistoryResult, PNStatus> callbackAction;

        public PNFetchHistoryResultExt(Action<PNFetchHistoryResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNFetchHistoryResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
