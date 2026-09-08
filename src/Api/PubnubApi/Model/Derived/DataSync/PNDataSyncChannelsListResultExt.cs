using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncChannelsListResultExt : PNCallback<PNDataSyncChannelsListResult>
{
    readonly Action<PNDataSyncChannelsListResult, PNStatus> callbackAction;

    public PNDataSyncChannelsListResultExt(Action<PNDataSyncChannelsListResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncChannelsListResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
