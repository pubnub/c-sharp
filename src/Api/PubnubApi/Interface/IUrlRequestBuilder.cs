using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.Interface
{
    public interface IUrlRequestBuilder
    {
        string PubnubInstanceId
        {
            get;
            set;
        }

        Uri BuildTimeRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam);

        Uri BuildMultiChannelSubscribeRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, long timetoken, string channelsJsonState, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam);

        Uri BuildMultiChannelLeaveRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam);

        Uri BuildPublishRequest(string requestMethod, string requestBody, string channel, object originalMessage, bool storeInHistory, int ttl, Dictionary<string, object> userMetaData, Dictionary<string, string> additionalUrlParams, Dictionary<string, object> externalQueryParam);

        Uri BuildSignalRequest(string requestMethod, string requestBody, string channel, object originalMessage, Dictionary<string, object> userMetaData, Dictionary<string, object> externalQueryParam);

        Uri BuildHereNowRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, Dictionary<string, object> externalQueryParam);

        Uri BuildHistoryRequest(string requestMethod, string requestBody, string channel, long start, long end, int count, bool reverse, bool includeToken, Dictionary<string, object> externalQueryParam);

        Uri BuildDeleteMessageRequest(string requestMethod, string requestBody, string channel, long start, long end, Dictionary<string, object> externalQueryParam);

        Uri BuildWhereNowRequest(string requestMethod, string requestBody, string uuid, Dictionary<string, object> externalQueryParam);

        Uri BuildGrantV2AccessRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string authKeysCommaDelimited, bool read, bool write, bool delete, bool manage, long ttl, Dictionary<string, object> externalQueryParam);

        Uri BuildGrantV3AccessRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam);

        Uri BuildAuditAccessRequest(string requestMethod, string requestBody, string channel, string channelGroup, string authKeysCommaDelimited, Dictionary<string, object> externalQueryParam);

        Uri BuildGetUserStateRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, Dictionary<string, object> externalQueryParam);

        Uri BuildSetUserStateRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam);

        Uri BuildAddChannelsToChannelGroupRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam);

        Uri BuildRemoveChannelsFromChannelGroupRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam);

        Uri BuildGetChannelsForChannelGroupRequest(string requestMethod, string requestBody, string nameSpace, string groupName, bool limitToChannelGroupScopeOnly, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllChannelGroupRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam);

        Uri BuildRegisterDevicePushRequest(string requestMethod, string requestBody, string channel, PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam);

        Uri BuildUnregisterDevicePushRequest(string requestMethod, string requestBody, PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam);

        Uri BuildRemoveChannelPushRequest(string requestMethod, string requestBody, string channel, PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam);

        Uri BuildGetChannelsPushRequest(string requestMethod, string requestBody, PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam);

        Uri BuildPresenceHeartbeatRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, string jsonUserState);

        Uri BuildMessageCountsRequest(string requestMethod, string requestBody, string[] channels, long[] timetokens, Dictionary<string, object> externalQueryParam);

        Uri BuildCreateUserRequest(string requestMethod, string requestBody, Dictionary<string, object> userCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildUpdateUserRequest(string requestMethod, string requestBody, string userId, Dictionary<string, object> userCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildDeleteUserRequest(string requestMethod, string requestBody, string userId, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllUsersRequest(string requestMethod, string requestBody, string start, string end, int limit, bool includeCount, bool includeCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildGetSingleUserRequest(string requestMethod, string requestBody, string userId, bool includeCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildCreateSpaceRequest(string requestMethod, string requestBody, Dictionary<string, object> spaceCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildUpdateSpaceRequest(string requestMethod, string requestBody, string spaceId, Dictionary<string, object> spaceCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildDeleteSpaceRequest(string requestMethod, string requestBody, string spaceId, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllSpacesRequest(string requestMethod, string requestBody, string start, string end, int limit, bool includeCount, bool includeCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildGetSingleSpaceRequest(string requestMethod, string requestBody, string spaceId, bool includeCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildUpdateSpaceMembershipsWithUserRequest(string requestMethod, string requestBody, string userId, string start, string end, int limit, bool includeCount, string includeOptions, Dictionary<string, object> externalQueryParam);

        Uri BuildMembersAddUpdateRemoveRequest(string requestMethod, string requestBody, string spaceId, string start, string end, int limit, bool includeCount, string includeOptions, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllMembershipsRequest(string requestMethod, string requestBody, string userId, string start, string end, int limit, bool includeCount, string includeOptions, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllMembersRequest(string requestMethod, string requestBody, string spaceId, string start, string end, int limit, bool includeCount, string includeOptions, Dictionary<string, object> externalQueryParam);
    }
}
