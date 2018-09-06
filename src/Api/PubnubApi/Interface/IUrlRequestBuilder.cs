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

        Uri BuildTimeRequest(Dictionary<string, object> externalQueryParam);

        Uri BuildMultiChannelSubscribeRequest(string[] channels, string[] channelGroups, long timetoken, string channelsJsonState, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam);

        Uri BuildMultiChannelLeaveRequest(string[] channels, string[] channelGroups, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam);

        Uri BuildPublishRequest(string channel, object originalMessage, bool storeInHistory, int ttl, Dictionary<string, object> userMetaData, bool usePOST, Dictionary<string, string> additionalUrlParams, Dictionary<string, object> externalQueryParam);

        Uri BuildHereNowRequest(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, Dictionary<string, object> externalQueryParam);

        Uri BuildHistoryRequest(string channel, long start, long end, int count, bool reverse, bool includeToken, Dictionary<string, object> externalQueryParam);

        Uri BuildDeleteMessageRequest(string channel, long start, long end, Dictionary<string, object> externalQueryParam);

        Uri BuildWhereNowRequest(string uuid, Dictionary<string, object> externalQueryParam);

        Uri BuildGrantAccessRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string authKeysCommaDelimited, bool read, bool write, bool delete, bool manage, long ttl, Dictionary<string, object> externalQueryParam);

        Uri BuildAuditAccessRequest(string channel, string channelGroup, string authKeysCommaDelimited, Dictionary<string, object> externalQueryParam);

        Uri BuildGetUserStateRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, Dictionary<string, object> externalQueryParam);

        Uri BuildSetUserStateRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam);

        Uri BuildAddChannelsToChannelGroupRequest(string channelsCommaDelimited, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam);

        Uri BuildRemoveChannelsFromChannelGroupRequest(string channelsCommaDelimited, string nameSpace, string groupName, Dictionary<string, object> externalQueryParam);

        Uri BuildGetChannelsForChannelGroupRequest(string nameSpace, string groupName, bool limitToChannelGroupScopeOnly, Dictionary<string, object> externalQueryParam);

        Uri BuildGetAllChannelGroupRequest(Dictionary<string, object> externalQueryParam);

        Uri BuildRegisterDevicePushRequest(string channel, PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam);

        Uri BuildUnregisterDevicePushRequest(PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam);

        Uri BuildRemoveChannelPushRequest(string channel, PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam);

        Uri BuildGetChannelsPushRequest(PNPushType pushType, string pushToken, Dictionary<string, object> externalQueryParam);

        Uri BuildPresenceHeartbeatRequest(string[] channels, string[] channelGroups, string jsonUserState);
    }
}
