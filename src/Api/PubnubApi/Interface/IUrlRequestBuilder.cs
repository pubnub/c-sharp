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

        Uri BuildTimeRequest();

        Uri BuildMultiChannelSubscribeRequest(string[] channels, string[] channelGroups, long timetoken, string channelsJsonState, Dictionary<string, string> initialSubscribeUrlParams);

        Uri BuildMultiChannelLeaveRequest(string[] channels, string[] channelGroups, string uuid, string jsonUserState);

        Uri BuildPublishRequest(string channel, object originalMessage, bool storeInHistory, int ttl, Dictionary<string, object> userMetaData, bool usePOST, Dictionary<string, string> additionalUrlParams);

        Uri BuildHereNowRequest(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState);

        Uri BuildHistoryRequest(string channel, long start, long end, int count, bool reverse, bool includeToken);

        Uri BuildDeleteMessageRequest(string channel, long start, long end);

        Uri BuildWhereNowRequest(string uuid);

        Uri BuildGrantAccessRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string authKeysCommaDelimited, bool read, bool write, bool manage, long ttl);

        Uri BuildAuditAccessRequest(string channel, string channelGroup, string authKeysCommaDelimited);

        Uri BuildGetUserStateRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid);

        Uri BuildSetUserStateRequest(string channelsCommaDelimited, string channelGroupsCommaDelimited, string uuid, string jsonUserState);

        Uri BuildAddChannelsToChannelGroupRequest(string channelsCommaDelimited, string nameSpace, string groupName);

        Uri BuildRemoveChannelsFromChannelGroupRequest(string channelsCommaDelimited, string nameSpace, string groupName);

        Uri BuildGetChannelsForChannelGroupRequest(string nameSpace, string groupName, bool limitToChannelGroupScopeOnly);

        Uri BuildGetAllChannelGroupRequest();

        Uri BuildRegisterDevicePushRequest(string channel, PNPushType pushType, string pushToken);

        Uri BuildUnregisterDevicePushRequest(PNPushType pushType, string pushToken);

        Uri BuildRemoveChannelPushRequest(string channel, PNPushType pushType, string pushToken);

        Uri BuildGetChannelsPushRequest(PNPushType pushType, string pushToken);

        Uri BuildPresenceHeartbeatRequest(string[] channels, string[] channelGroups, string jsonUserState);
    }
}
