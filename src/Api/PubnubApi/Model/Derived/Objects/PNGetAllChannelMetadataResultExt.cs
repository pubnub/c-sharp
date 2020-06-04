using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetAllChannelMetadataResultExt : PNCallback<PNGetAllChannelMetadataResult>
    {
        readonly Action<PNGetAllChannelMetadataResult, PNStatus> callbackAction;

        public PNGetAllChannelMetadataResultExt(Action<PNGetAllChannelMetadataResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetAllChannelMetadataResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
