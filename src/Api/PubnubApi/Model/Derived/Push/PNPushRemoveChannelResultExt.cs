using System;

namespace PubnubApi
{
    public class PNPushRemoveChannelResultExt : PNCallback<PNPushRemoveChannelResult>
    {
        Action<PNPushRemoveChannelResult, PNStatus> callbackAction = null;

        public PNPushRemoveChannelResultExt(Action<PNPushRemoveChannelResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNPushRemoveChannelResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
