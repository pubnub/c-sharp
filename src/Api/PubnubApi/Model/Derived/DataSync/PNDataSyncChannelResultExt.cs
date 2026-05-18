using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncChannelResultExt : PNCallback<PNDataSyncChannelResult>
{
    readonly Action<PNDataSyncChannelResult, PNStatus> callbackAction;

    public PNDataSyncChannelResultExt(Action<PNDataSyncChannelResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncChannelResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
