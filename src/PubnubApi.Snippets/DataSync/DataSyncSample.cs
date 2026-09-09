// snippet.using
using PubnubApi;
using PubnubApi.EndPoint;

// snippet.end

public class DataSyncSample
{
    private static Pubnub pubnub;

    static void Init()
    {
        // snippet.pubnub_init
        // Configuration
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"))
        {
            SubscribeKey = "demo",
            PublishKey = "demo",
            Secure = true
        };

        // Initialize PubNub
        Pubnub pubnub = new Pubnub(pnConfiguration);

        // snippet.end
    }

    // Users

    public static async Task CreateUserBasicUsage()
    {
        // snippet.create_user_basic_usage
        try
        {
            PNResult<PNDataSyncUserResult> response = await pubnub.DataSync.CreateUser(new CreateUserParameters
            {
                Id = "user-alice",
                EntityClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "name", "Alice" },
                    { "type", "shopper" },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetUserBasicUsage()
    {
        // snippet.get_user_basic_usage
        try
        {
            PNResult<PNDataSyncUserResult> response = await pubnub.DataSync.GetUser(new GetUserParameters
            {
                Id = "user-alice",
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetUsersBasicUsage()
    {
        // snippet.get_users_basic_usage
        try
        {
            PNResult<PNDataSyncUsersListResult> response = await pubnub.DataSync.GetUsers(new GetUsersParameters
            {
                Limit = 20,
                Sort = "createdAt",
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Data.Count);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task SetUserBasicUsage()
    {
        // snippet.set_user_basic_usage
        try
        {
            PNResult<PNDataSyncUserResult> response = await pubnub.DataSync.SetUser(new SetUserParameters
            {
                Id = "user-alice",
                EntityClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "name", "Alice B." },
                    { "type", "shopper" },
                },
                IfMatch = "AbQdEfGhIjKlMn",
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task UpdateUserBasicUsage()
    {
        // snippet.update_user_basic_usage
        try
        {
            PNResult<PNDataSyncUserResult> response = await pubnub.DataSync.UpdateUser(new UpdateUserParameters
            {
                Id = "user-alice",
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/payload/name", Value = "Alice B." },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task RemoveUserBasicUsage()
    {
        // snippet.remove_user_basic_usage
        try
        {
            PNResult<PNDataSyncDeleteUserResult> response = await pubnub.DataSync.DeleteUser(new DeleteUserParameters
            {
                Id = "user-alice",
            });

            Console.WriteLine(response.Status.StatusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    // Channels

    public static async Task CreateChannelBasicUsage()
    {
        // snippet.create_channel_basic_usage
        try
        {
            PNResult<PNDataSyncChannelResult> response = await pubnub.DataSync.CreateChannel(new CreateChannelParameters
            {
                Id = "channel-summer-sale",
                EntityClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "name", "Summer Sale" },
                    { "type", "promotion" },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetChannelBasicUsage()
    {
        // snippet.get_channel_basic_usage
        try
        {
            PNResult<PNDataSyncChannelResult> response = await pubnub.DataSync.GetChannel(new GetChannelParameters
            {
                Id = "channel-summer-sale",
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetChannelsBasicUsage()
    {
        // snippet.get_channels_basic_usage
        try
        {
            PNResult<PNDataSyncChannelsListResult> response = await pubnub.DataSync.GetChannels(new GetChannelsParameters
            {
                Limit = 20,
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Data.Count);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task SetChannelBasicUsage()
    {
        // snippet.set_channel_basic_usage
        try
        {
            PNResult<PNDataSyncChannelResult> response = await pubnub.DataSync.SetChannel(new SetChannelParameters
            {
                Id = "channel-summer-sale",
                EntityClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "name", "Summer Sale 2026" },
                    { "type", "promotion" },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task UpdateChannelBasicUsage()
    {
        // snippet.update_channel_basic_usage
        try
        {
            PNResult<PNDataSyncChannelResult> response = await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
            {
                Id = "channel-summer-sale",
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/payload/name", Value = "Summer Sale 2026" },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task RemoveChannelBasicUsage()
    {
        // snippet.remove_channel_basic_usage
        try
        {
            PNResult<PNDataSyncDeleteChannelResult> response = await pubnub.DataSync.DeleteChannel(new DeleteChannelParameters
            {
                Id = "channel-summer-sale",
            });

            Console.WriteLine(response.Status.StatusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    // Memberships

    public static async Task CreateMembershipBasicUsage()
    {
        // snippet.create_membership_basic_usage
        try
        {
            PNResult<PNDataSyncMembershipResult> response = await pubnub.DataSync.CreateMembership(new CreateMembershipParameters
            {
                ChannelId = "channel-summer-sale",
                UserId = "user-alice",
                MembershipClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "role", "viewer" },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetMembershipBasicUsage()
    {
        // snippet.get_membership_basic_usage
        try
        {
            PNResult<PNDataSyncMembershipResult> response = await pubnub.DataSync.GetMembership(new GetMembershipParameters
            {
                Id = "membership-alice-summer-sale",
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetMembershipsBasicUsage()
    {
        // snippet.get_memberships_basic_usage
        try
        {
            PNResult<PNDataSyncMembershipsListResult> response = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
            {
                UserId = "user-alice",
                Limit = 20,
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Data.Count);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task SetMembershipBasicUsage()
    {
        // snippet.set_membership_basic_usage
        try
        {
            PNResult<PNDataSyncMembershipResult> response = await pubnub.DataSync.SetMembership(new UpdateMembershipParameters
            {
                Id = "membership-alice-summer-sale",
                RelationshipClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "role", "moderator" },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task UpdateMembershipBasicUsage()
    {
        // snippet.update_membership_basic_usage
        try
        {
            PNResult<PNDataSyncMembershipResult> response = await pubnub.DataSync.UpdateMembership(new PatchMembershipParameters
            {
                Id = "membership-alice-summer-sale",
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/payload/role", Value = "moderator" },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task RemoveMembershipBasicUsage()
    {
        // snippet.remove_membership_basic_usage
        try
        {
            PNResult<PNDataSyncDeleteMembershipResult> response = await pubnub.DataSync.DeleteMembership(new DeleteMembershipParameters
            {
                Id = "membership-alice-summer-sale",
            });

            Console.WriteLine(response.Status.StatusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    // Entities

    public static async Task CreateEntityBasicUsage()
    {
        // snippet.create_entity_basic_usage
        try
        {
            PNResult<PNDataSyncEntityResult> response = await pubnub.DataSync.CreateEntity(new CreateEntityParameters
            {
                Id = "product-sneaker-42",
                EntityClass = "product",
                EntityClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "name", "Retro Sneaker" },
                    { "price", 89.99 },
                    { "stock", 12 },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetEntityBasicUsage()
    {
        // snippet.get_entity_basic_usage
        try
        {
            PNResult<PNDataSyncEntityResult> response = await pubnub.DataSync.GetEntity(new GetEntityParameters
            {
                Id = "product-sneaker-42",
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetEntitiesBasicUsage()
    {
        // snippet.get_entities_basic_usage
        try
        {
            PNResult<PNDataSyncEntitiesListResult> response = await pubnub.DataSync.GetEntities(new GetEntitiesParameters
            {
                EntityClass = "product",
                Sort = "price:desc",
                Limit = 20,
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Data.Count);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task SetEntityBasicUsage()
    {
        // snippet.set_entity_basic_usage
        try
        {
            PNResult<PNDataSyncEntityResult> response = await pubnub.DataSync.SetEntity(new SetEntityParameters
            {
                Id = "product-sneaker-42",
                EntityClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "name", "Retro Sneaker" },
                    { "price", 79.99 },
                    { "stock", 8 },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task UpdateEntityBasicUsage()
    {
        // snippet.update_entity_basic_usage
        try
        {
            PNResult<PNDataSyncEntityResult> response = await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
            {
                Id = "product-sneaker-42",
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/payload/price", Value = 79.99 },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task RemoveEntityBasicUsage()
    {
        // snippet.remove_entity_basic_usage
        try
        {
            PNResult<PNDataSyncDeleteEntityResult> response = await pubnub.DataSync.DeleteEntity(new DeleteEntityParameters
            {
                Id = "product-sneaker-42",
            });

            Console.WriteLine(response.Status.StatusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    // Relationships

    public static async Task CreateRelationshipBasicUsage()
    {
        // snippet.create_relationship_basic_usage
        try
        {
            PNResult<PNDataSyncRelationshipResult> response = await pubnub.DataSync.CreateRelationship(new CreateRelationshipParameters
            {
                EntityAId = "seller-bob",
                EntityBId = "product-sneaker-42",
                RelationshipClass = "ProductOwner",
                RelationshipClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "since", "2026-07-13" },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetRelationshipBasicUsage()
    {
        // snippet.get_relationship_basic_usage
        try
        {
            PNResult<PNDataSyncRelationshipResult> response = await pubnub.DataSync.GetRelationship(new GetRelationshipParameters
            {
                Id = "rel-bob-owns-sneaker-42",
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task GetRelationshipsBasicUsage()
    {
        // snippet.get_relationships_basic_usage
        try
        {
            PNResult<PNDataSyncRelationshipsListResult> response = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
            {
                RelationshipClass = "ProductOwner",
                EntityAId = "seller-bob",
                Limit = 20,
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Data.Count);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task SetRelationshipBasicUsage()
    {
        // snippet.set_relationship_basic_usage
        try
        {
            PNResult<PNDataSyncRelationshipResult> response = await pubnub.DataSync.SetRelationship(new SetRelationshipParameters
            {
                Id = "rel-bob-owns-sneaker-42",
                RelationshipClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "since", "2026-07-13" },
                    { "tier", "gold" },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task UpdateRelationshipBasicUsage()
    {
        // snippet.update_relationship_basic_usage
        try
        {
            PNResult<PNDataSyncRelationshipResult> response = await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
            {
                Id = "rel-bob-owns-sneaker-42",
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/payload/tier", Value = "platinum" },
                },
            });

            if (!response.Status.Error)
            {
                Console.WriteLine(response.Result.Id);
            }
            else
            {
                Console.WriteLine($"Request can't be executed due to error: {response.Status.ErrorData.Information}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task RemoveRelationshipBasicUsage()
    {
        // snippet.remove_relationship_basic_usage
        try
        {
            PNResult<PNDataSyncDeleteRelationshipResult> response = await pubnub.DataSync.DeleteRelationship(new DeleteRelationshipParameters
            {
                Id = "rel-bob-owns-sneaker-42",
            });

            Console.WriteLine(response.Status.StatusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request can't be executed due to error: {ex.Message}");
        }
        // snippet.end
    }
}
