using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNWhereNowResultExt : PNCallback<PNWhereNowResult>
    {
        Action<PNWhereNowResult, PNStatus> callbackAction = null;

        public PNWhereNowResultExt(Action<PNWhereNowResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNWhereNowResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
