using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetAllUuidMetadataResultExt : PNCallback<PNGetAllUuidMetadataResult>
    {
        readonly Action<PNGetAllUuidMetadataResult, PNStatus> callbackAction;

        public PNGetAllUuidMetadataResultExt(Action<PNGetAllUuidMetadataResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetAllUuidMetadataResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
