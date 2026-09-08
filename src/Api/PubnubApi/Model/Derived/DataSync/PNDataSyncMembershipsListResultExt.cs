using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncMembershipsListResultExt : PNCallback<PNDataSyncMembershipsListResult>
{
    readonly Action<PNDataSyncMembershipsListResult, PNStatus> callbackAction;

    public PNDataSyncMembershipsListResultExt(Action<PNDataSyncMembershipsListResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncMembershipsListResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
