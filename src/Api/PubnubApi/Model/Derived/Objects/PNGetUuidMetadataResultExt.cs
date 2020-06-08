using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetUuidMetadataResultExt : PNCallback<PNGetUuidMetadataResult>
    {
        readonly Action<PNGetUuidMetadataResult, PNStatus> callbackAction;

        public PNGetUuidMetadataResultExt(Action<PNGetUuidMetadataResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetUuidMetadataResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
