using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncDeleteChannelResultExt : PNCallback<PNDataSyncDeleteChannelResult>
{
    readonly Action<PNDataSyncDeleteChannelResult, PNStatus> callbackAction;

    public PNDataSyncDeleteChannelResultExt(Action<PNDataSyncDeleteChannelResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncDeleteChannelResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
