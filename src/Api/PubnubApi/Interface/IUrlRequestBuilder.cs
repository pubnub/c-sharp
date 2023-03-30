using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public interface IUrlRequestBuilder
    {
        Uri BuildTimeRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam);

        Uri BuildMultiChannelSubscribeRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, long timetoken, int region, string channelsJsonState, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam);

        Uri BuildMultiChannelLeaveRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, string jsonUserState, Dictionary<string, object> externalQueryParam);

        Uri BuildPublishRequest(string requestMethod, string requestBody, string channel, object originalMessage, bool storeInHistory, int ttl, Dictionary<string, object> userMetaData, Dictionary<string, string> additionalUrlParams, Dictionary<string, object> externalQueryParam);

        Uri BuildSignalRequest(string requestMethod, string requestBody, string channel, object originalMessage, Dictionary<string, object> userMetaData, Dictionary<string, object> externalQueryParam);

        Uri BuildHereNowRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, Dictionary<string, object> externalQueryParam);

        Uri BuildHistoryRequest(string requestMethod, string requestBody, string channel, long start, long end, int count, bool reverse, bool includeToken, bool includeMeta, Dictionary<string, object> externalQueryParam);

        Uri BuildFetchRequest(string requestMethod, string requestBody, string[] channels, long start, long end, int count, bool reverse, bool includeMeta, bool includeMessageActions, bool includeUuid, bool includeMessageType, Dictionary<string, object> externalQueryParam);

        Uri BuildDeleteMessageRequest(string requestMethod, string requestBody, string channel, long start, long end, Dictionary<string, object> externalQueryParam);

        Uri BuildWhereNowRequest(string requestMethod, string requestBody, string uuid, Dictionary<string, object> externalQueryParam);

        Uri BuildGrantV2AccessRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string targetUuidsCommaDelimited, string authKeysCommaDelimited, bool read, bool write, bool delete, bool manage, bool get, bool update, bool join, long ttl, Dictionary<string, object> externalQueryParam);

        Uri BuildGrantV3AccessRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam);

        Uri BuildRevokeV3AccessRequest(string requestMethod, string requestBody, string token, Dictionary<string, object> externalQueryParam);

        Uri BuildAuditAccessRequest(string requestMethod, string requestBody, string channel, string channelGroup, string authKeysCommaDelimited, Dictionary<string, object> externalQueryParam);

        Uri BuildGetUserStateRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, Dictionary<string, object> externalQueryParam);

        Uri BuildSetUserStateRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam);

        Uri BuildAddChannelsToChannelGroupRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam);

        Uri BuildRemoveChannelsFromChannelGroupRequest(string requestMethod, string requestBody, string channelsCommaDelimited, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam);

        Uri BuildGetChannelsForChannelGroupRequest(string requestMethod, string requestBody, string nameSpace, string groupName, bool limitToChannelGroupScopeOnly, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllChannelGroupRequest(string requestMethod, string requestBody, Dictionary<string, object> externalQueryParam);

        Uri BuildRegisterDevicePushRequest(string requestMethod, string requestBody, string channel, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam);

        Uri BuildUnregisterDevicePushRequest(string requestMethod, string requestBody, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam);

        Uri BuildRemoveChannelPushRequest(string requestMethod, string requestBody, string channel, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam);

        Uri BuildGetChannelsPushRequest(string requestMethod, string requestBody, PNPushType pushType, string pushToken, PushEnvironment environment, string deviceTopic, Dictionary<string, object> externalQueryParam);

        Uri BuildPresenceHeartbeatRequest(string requestMethod, string requestBody, string[] channels, string[] channelGroups, string jsonUserState);

        Uri BuildMessageCountsRequest(string requestMethod, string requestBody, string[] channels, long[] timetokens, Dictionary<string, object> externalQueryParam);

        Uri BuildSetUuidMetadataRequest(string requestMethod, string requestBody, string uuid, bool includeCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildDeleteUuidMetadataRequest(string requestMethod, string requestBody, string uuid, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllUuidMetadataRequest(string requestMethod, string requestBody, string start, string end, int limit, bool includeCount, bool includeCustom, string filter, List<string> sort, Dictionary<string, object> externalQueryParam);

        Uri BuildGetSingleUuidMetadataRequest(string requestMethod, string requestBody, string uuid, bool includeCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildSetChannelMetadataRequest(string requestMethod, string requestBody, string channel, bool includeCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildDeleteChannelMetadataRequest(string requestMethod, string requestBody, string channel, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllChannelMetadataRequest(string requestMethod, string requestBody, string start, string end, int limit, bool includeCount, bool includeCustom, string filter, List<string> sort, Dictionary<string, object> externalQueryParam);

        Uri BuildGetSingleChannelMetadataRequest(string requestMethod, string requestBody, string channel, bool includeCustom, Dictionary<string, object> externalQueryParam);

        Uri BuildMembershipSetRemoveManageUserRequest(PNOperationType type, string requestMethod, string requestBody, string uuid, string start, string end, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam);

        Uri BuildMemberAddUpdateRemoveChannelRequest(string requestMethod, string requestBody, string channel, string start, string end, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllMembershipsRequest(string requestMethod, string requestBody, string uuid, string start, string end, int limit, bool includeCount, string includeOptions, string filter, List<string> sort, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllMembersRequest(string requestMethod, string requestBody, string channel, string start, string end, int limit, bool includeCount, string includeOptions, string filter, List<string> sort, Dictionary<string, object> externalQueryParam);

        Uri BuildAddMessageActionRequest(string requestMethod, string requestBody, string channel, long messageTimetoken, Dictionary<string, object> externalQueryParam);

        Uri BuildRemoveMessageActionRequest(string requestMethod, string requestBody, string channel, long messageTimetoken, long actionTimetoken, string messageActionUuid, Dictionary<string, object> externalQueryParam);

        Uri BuildGetMessageActionsRequest(string requestMethod, string requestBody, string channel, long start, long end, int limit, Dictionary<string, object> externalQueryParam);

        Uri BuildGenerateFileUploadUrlRequest(string requestMethod, string requestBody, string channel, Dictionary<string, object> externalQueryParam);

        Uri BuildPublishFileMessageRequest(string requestMethod, string requestBody, string channel, object originalMessage, bool storeInHistory, int ttl, Dictionary<string, object> userMetaData, Dictionary<string, string> additionalUrlParams, Dictionary<string, object> externalQueryParam);

        Uri BuildGetFileUrlOrDeleteReqest(string requestMethod, string requestBody, string channel, string fileId, string fileName, Dictionary<string, object> externalQueryParam, PNOperationType operationType);

        Uri BuildListFilesReqest(string requestMethod, string requestBody, string channel, int limit, string nextToken, Dictionary<string, object> externalQueryParam, PNOperationType operationType);
    }
}
