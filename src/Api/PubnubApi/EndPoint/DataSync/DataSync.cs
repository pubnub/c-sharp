using System.Threading.Tasks;

namespace PubnubApi.EndPoint;

/// <summary>
/// Entrypoint for PubNub Data Sync operations
/// </summary>
public class DataSync
{
    private readonly IPubnubUnitTest unit;
    private readonly Pubnub pubnub;
    private readonly TokenManager tokenManager;
    
    public DataSync(Pubnub pubnub, IPubnubUnitTest unit, TokenManager tokenManager)
    {
        this.pubnub = pubnub;
        this.unit = unit;
        this.tokenManager = tokenManager;
    }

    public async Task<PNResult<PNDataSyncEntityResult>> CreateEntity(CreateEntityParameters parameters)
    {
        return await new CreateEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void CreateEntity(CreateEntityParameters parameters, PNDataSyncEntityResultExt callback)
    {
        new CreateEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }
    
    public async Task<PNResult<PNDataSyncEntityResult>> GetEntity(GetEntityParameters parameters)
    {
        return await new GetEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }
    
    public void GetEntity(GetEntityParameters parameters, PNDataSyncEntityResultExt callback)
    {
        new GetEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }
    
    public async Task<PNResult<PNDataSyncEntitiesListResult>> GetEntities(GetEntitiesParameters parameters)
    {
        return await new GetEntitiesOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }
    
    public void GetEntities(GetEntitiesParameters parameters, PNDataSyncEntitiesListResultExt callback)
    {
        new GetEntitiesOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }
    
    public async Task<PNResult<PNDataSyncEntityResult>> UpdateEntity(UpdateEntityParameters parameters)
    {
        return await new UpdateEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }
    
    public void UpdateEntity(UpdateEntityParameters parameters, PNDataSyncEntityResultExt callback)
    {
        new UpdateEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }
    
    public async Task<PNResult<PNDataSyncEntityResult>> PatchEntity(PatchEntityParameters parameters)
    {
        return await new PatchEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }
    
    public void PatchEntity(PatchEntityParameters parameters, PNDataSyncEntityResultExt callback)
    {
        new PatchEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncDeleteEntityResult>> DeleteEntity(DeleteEntityParameters parameters)
    {
        return await new DeleteEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }
    
    public void DeleteEntity(DeleteEntityParameters parameters, PNDataSyncDeleteEntityResultExt callback)
    {
        new DeleteEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncRelationshipResult>> CreateRelationship(CreateRelationshipParameters parameters)
    {
        return await new CreateRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void CreateRelationship(CreateRelationshipParameters parameters, PNDataSyncRelationshipResultExt callback)
    {
        new CreateRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncRelationshipResult>> GetRelationship(GetRelationshipParameters parameters)
    {
        return await new GetRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void GetRelationship(GetRelationshipParameters parameters, PNDataSyncRelationshipResultExt callback)
    {
        new GetRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncRelationshipsListResult>> GetRelationships(GetRelationshipsParameters parameters)
    {
        return await new GetRelationshipsOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void GetRelationships(GetRelationshipsParameters parameters, PNDataSyncRelationshipsListResultExt callback)
    {
        new GetRelationshipsOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncRelationshipResult>> UpdateRelationship(UpdateRelationshipParameters parameters)
    {
        return await new UpdateRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void UpdateRelationship(UpdateRelationshipParameters parameters, PNDataSyncRelationshipResultExt callback)
    {
        new UpdateRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncRelationshipResult>> PatchRelationship(PatchRelationshipParameters parameters)
    {
        return await new PatchRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void PatchRelationship(PatchRelationshipParameters parameters, PNDataSyncRelationshipResultExt callback)
    {
        new PatchRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncDeleteRelationshipResult>> DeleteRelationship(DeleteRelationshipParameters parameters)
    {
        return await new DeleteRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void DeleteRelationship(DeleteRelationshipParameters parameters, PNDataSyncDeleteRelationshipResultExt callback)
    {
        new DeleteRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }
}