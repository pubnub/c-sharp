using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncEntityResultExt : PNCallback<PNDataSyncEntityResult>
{
    readonly Action<PNDataSyncEntityResult, PNStatus> callbackAction;

    public PNDataSyncEntityResultExt(Action<PNDataSyncEntityResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncEntityResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}