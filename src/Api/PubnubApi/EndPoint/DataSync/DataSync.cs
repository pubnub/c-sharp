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
    
    public async Task<PNResult<PNDataSyncEntityResult>> SetEntity(SetEntityParameters parameters)
    {
        return await new SetEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }
    
    public void SetEntity(SetEntityParameters parameters, PNDataSyncEntityResultExt callback)
    {
        new SetEntityOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
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

    public async Task<PNResult<PNDataSyncRelationshipResult>> SetRelationship(SetRelationshipParameters parameters)
    {
        return await new SetRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void SetRelationship(SetRelationshipParameters parameters, PNDataSyncRelationshipResultExt callback)
    {
        new SetRelationshipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
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

    // User operations

    public async Task<PNResult<PNDataSyncUserResult>> CreateUser(CreateUserParameters parameters)
    {
        return await new CreateUserOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void CreateUser(CreateUserParameters parameters, PNDataSyncUserResultExt callback)
    {
        new CreateUserOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncUserResult>> GetUser(GetUserParameters parameters)
    {
        return await new GetUserOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void GetUser(GetUserParameters parameters, PNDataSyncUserResultExt callback)
    {
        new GetUserOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncUsersListResult>> GetUsers(GetUsersParameters parameters)
    {
        return await new GetUsersOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void GetUsers(GetUsersParameters parameters, PNDataSyncUsersListResultExt callback)
    {
        new GetUsersOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncUserResult>> SetUser(SetUserParameters parameters)
    {
        return await new SetUserOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void SetUser(SetUserParameters parameters, PNDataSyncUserResultExt callback)
    {
        new SetUserOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncUserResult>> UpdateUser(UpdateUserParameters parameters)
    {
        return await new UpdateUserOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void UpdateUser(UpdateUserParameters parameters, PNDataSyncUserResultExt callback)
    {
        new UpdateUserOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncDeleteUserResult>> DeleteUser(DeleteUserParameters parameters)
    {
        return await new DeleteUserOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void DeleteUser(DeleteUserParameters parameters, PNDataSyncDeleteUserResultExt callback)
    {
        new DeleteUserOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    // Channel operations

    public async Task<PNResult<PNDataSyncChannelResult>> CreateChannel(CreateChannelParameters parameters)
    {
        return await new CreateChannelOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void CreateChannel(CreateChannelParameters parameters, PNDataSyncChannelResultExt callback)
    {
        new CreateChannelOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncChannelResult>> GetChannel(GetChannelParameters parameters)
    {
        return await new GetChannelOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void GetChannel(GetChannelParameters parameters, PNDataSyncChannelResultExt callback)
    {
        new GetChannelOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncChannelsListResult>> GetChannels(GetChannelsParameters parameters)
    {
        return await new GetChannelsOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void GetChannels(GetChannelsParameters parameters, PNDataSyncChannelsListResultExt callback)
    {
        new GetChannelsOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncChannelResult>> SetChannel(SetChannelParameters parameters)
    {
        return await new SetChannelOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void SetChannel(SetChannelParameters parameters, PNDataSyncChannelResultExt callback)
    {
        new SetChannelOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncChannelResult>> UpdateChannel(UpdateChannelParameters parameters)
    {
        return await new UpdateChannelOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void UpdateChannel(UpdateChannelParameters parameters, PNDataSyncChannelResultExt callback)
    {
        new UpdateChannelOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncDeleteChannelResult>> DeleteChannel(DeleteChannelParameters parameters)
    {
        return await new DeleteChannelOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void DeleteChannel(DeleteChannelParameters parameters, PNDataSyncDeleteChannelResultExt callback)
    {
        new DeleteChannelOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    // Membership operations

    public async Task<PNResult<PNDataSyncMembershipResult>> CreateMembership(CreateMembershipParameters parameters)
    {
        return await new CreateMembershipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void CreateMembership(CreateMembershipParameters parameters, PNDataSyncMembershipResultExt callback)
    {
        new CreateMembershipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncMembershipResult>> GetMembership(GetMembershipParameters parameters)
    {
        return await new GetMembershipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void GetMembership(GetMembershipParameters parameters, PNDataSyncMembershipResultExt callback)
    {
        new GetMembershipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncMembershipsListResult>> GetMemberships(GetMembershipsParameters parameters)
    {
        return await new GetDataSyncMembershipsOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void GetMemberships(GetMembershipsParameters parameters, PNDataSyncMembershipsListResultExt callback)
    {
        new GetDataSyncMembershipsOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncMembershipResult>> SetMembership(UpdateMembershipParameters parameters)
    {
        return await new SetMembershipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void SetMembership(UpdateMembershipParameters parameters, PNDataSyncMembershipResultExt callback)
    {
        new SetMembershipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncMembershipResult>> UpdateMembership(PatchMembershipParameters parameters)
    {
        return await new UpdateMembershipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void UpdateMembership(PatchMembershipParameters parameters, PNDataSyncMembershipResultExt callback)
    {
        new UpdateMembershipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }

    public async Task<PNResult<PNDataSyncDeleteMembershipResult>> DeleteMembership(DeleteMembershipParameters parameters)
    {
        return await new DeleteMembershipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).ExecuteAsync();
    }

    public void DeleteMembership(DeleteMembershipParameters parameters, PNDataSyncDeleteMembershipResultExt callback)
    {
        new DeleteMembershipOperation(pubnub.PNConfig, pubnub.JsonPluggableLibrary, unit, tokenManager, pubnub,
            parameters).Execute(callback);
    }
}