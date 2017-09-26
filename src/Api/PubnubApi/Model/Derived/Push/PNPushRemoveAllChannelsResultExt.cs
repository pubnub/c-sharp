using System;

namespace PubnubApi
{
    public class PNPushRemoveAllChannelsResultExt : PNCallback<PNPushRemoveAllChannelsResult>
    {
        Action<PNPushRemoveAllChannelsResult, PNStatus> callbackAction = null;

        public PNPushRemoveAllChannelsResultExt(Action<PNPushRemoveAllChannelsResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNPushRemoveAllChannelsResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
