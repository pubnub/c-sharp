using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncEntitiesListResultExt : PNCallback<PNDataSyncEntitiesListResult>
{
    readonly Action<PNDataSyncEntitiesListResult, PNStatus> callbackAction;

    public PNDataSyncEntitiesListResultExt(Action<PNDataSyncEntitiesListResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncEntitiesListResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}