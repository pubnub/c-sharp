using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncDeleteRelationshipResultExt : PNCallback<PNDataSyncDeleteRelationshipResult>
{
    readonly Action<PNDataSyncDeleteRelationshipResult, PNStatus> callbackAction;

    public PNDataSyncDeleteRelationshipResultExt(Action<PNDataSyncDeleteRelationshipResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncDeleteRelationshipResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
