using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncDeleteEntityResultExt : PNCallback<PNDataSyncDeleteEntityResult>
{
    readonly Action<PNDataSyncDeleteEntityResult, PNStatus> callbackAction;

    public PNDataSyncDeleteEntityResultExt(Action<PNDataSyncDeleteEntityResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncDeleteEntityResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}