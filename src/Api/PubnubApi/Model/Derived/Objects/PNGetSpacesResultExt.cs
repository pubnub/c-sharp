using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetSpacesResultExt : PNCallback<PNGetSpacesResult>
    {
        readonly Action<PNGetSpacesResult, PNStatus> callbackAction;

        public PNGetSpacesResultExt(Action<PNGetSpacesResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetSpacesResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
