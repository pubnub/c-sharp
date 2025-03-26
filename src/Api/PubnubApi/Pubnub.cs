using System;
using System.Collections.Generic;
using System.Globalization;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Subscribe;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.Interface;
using PubnubApi.EventEngine.Presence;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using PubnubApi.EventEngine.Common;
using System.Collections.Concurrent;
using PubnubApi.PNSDK;

namespace PubnubApi
{
    public class Pubnub
    {
        private ConcurrentDictionary<string, PNConfiguration> pubnubConfig { get; } =
            new ConcurrentDictionary<string, PNConfiguration>();

        private IPubnubUnitTest pubnubUnitTest;
        private IPubnubLog pubnubLog;
        private ListenerManager listenerManager;
        private readonly TokenManager tokenManager;
        private object savedSubscribeOperation;
        private readonly string savedSdkVerion;
        private SubscribeEventEngineFactory subscribeEventEngineFactory;
        private PresenceEventEngineFactory presenceEventengineFactory;
        private EventEmitter eventEmitter;
        private PubnubLogModule logger;
        private List<SubscribeCallback> subscribeCallbackListenerList { get; set; } = new List<SubscribeCallback>();

#if UNITY
        private static System.Func<UnsubscribeAllOperation<object>> OnCleanupCall;
#endif

#if UNITY
        /// <summary>
        /// Call this function to globally clean up all background threads running in the SDK. Note that this will unsubscribe all channels.
        /// </summary>
        public static void CleanUp()
        {
            OnCleanupCall?.Invoke();
        }
#endif

        #region "PubNub API Channel Methods"

        public ISubscribeOperation<T> Subscribe<T>()
        {
            PresenceOperation<T> presenceOperation = null;
            if (pubnubConfig[InstanceId].EnableEventEngine)
            {
                if (pubnubConfig[InstanceId].PresenceInterval > 0)
                {
                    presenceOperation = new PresenceOperation<T>(this, InstanceId,
                        pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, tokenManager,
                        pubnubUnitTest, presenceEventengineFactory);
                }

                SubscribeEndpoint<T> subscribeOperation = new SubscribeEndpoint<T>(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest, tokenManager, subscribeEventEngineFactory, presenceOperation, InstanceId,
                    this);
                subscribeOperation.EventEmitter = eventEmitter;
                subscribeOperation.SubscribeListenerList = subscribeCallbackListenerList;
                savedSubscribeOperation = subscribeOperation;
                return subscribeOperation;
            }
            else
            {
                SubscribeOperation<T> subscribeOperation = new SubscribeOperation<T>(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, this);
                savedSubscribeOperation = subscribeOperation;
                return subscribeOperation;
            }
        }

        public IUnsubscribeOperation<T> Unsubscribe<T>()
        {
            if (pubnubConfig[InstanceId].EnableEventEngine)
            {
                UnsubscribeEndpoint<T> unsubscribeOperation = new UnsubscribeEndpoint<T>(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, subscribeEventEngineFactory, presenceEventengineFactory,
                    this);
                return unsubscribeOperation;
            }
            else
            {
                UnsubscribeOperation<T> unsubscribeOperation = new UnsubscribeOperation<T>(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, this);
                unsubscribeOperation.CurrentPubnubInstance(this);
                return unsubscribeOperation;
            }
        }

        public UnsubscribeAllOperation<T> UnsubscribeAll<T>()
        {
            if (pubnubConfig[InstanceId].EnableEventEngine)
            {
                UnsubscribeAllEndpoint<T> unsubscribeAllEndpoint = new UnsubscribeAllEndpoint<T>(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, subscribeEventEngineFactory, presenceEventengineFactory,
                    this);
                return unsubscribeAllEndpoint;
            }
            else
            {
                UnsubscribeAllOperation<T> unSubscribeAllOperation =
                    new UnsubscribeAllOperation<T>(
                        pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                        pubnubUnitTest,  tokenManager, this);
                return unSubscribeAllOperation;
            }
        }

        public PublishOperation Publish()
        {
            PublishOperation publishOperation = new PublishOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            publishOperation.CurrentPubnubInstance(this);
            return publishOperation;
        }

        public FireOperation Fire()
        {
            FireOperation fireOperation =
                new FireOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            fireOperation.CurrentPubnubInstance(this);
            return fireOperation;
        }

        public SignalOperation Signal()
        {
            SignalOperation signalOperation = new SignalOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return signalOperation;
        }

        public HistoryOperation History()
        {
            HistoryOperation historyOperaton = new HistoryOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return historyOperaton;
        }

        public FetchHistoryOperation FetchHistory()
        {
            FetchHistoryOperation historyOperaton = new FetchHistoryOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return historyOperaton;
        }

        public DeleteMessageOperation DeleteMessages()
        {
            DeleteMessageOperation deleteMessageOperaton = new DeleteMessageOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return deleteMessageOperaton;
        }

        public MessageCountsOperation MessageCounts()
        {
            MessageCountsOperation messageCount = new MessageCountsOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            messageCount.CurrentPubnubInstance(this);
            return messageCount;
        }

        public HereNowOperation HereNow()
        {
            HereNowOperation hereNowOperation = new HereNowOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            hereNowOperation.CurrentPubnubInstance(this);
            return hereNowOperation;
        }

        public WhereNowOperation WhereNow()
        {
            WhereNowOperation whereNowOperation = new WhereNowOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            whereNowOperation.CurrentPubnubInstance(this);
            return whereNowOperation;
        }

        public TimeOperation Time()
        {
            TimeOperation timeOperation =
                new TimeOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  this);
            timeOperation.CurrentPubnubInstance(this);
            return timeOperation;
        }

        public AuditOperation Audit()
        {
            AuditOperation auditOperation =
                new AuditOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  this);
            auditOperation.CurrentPubnubInstance(this);
            return auditOperation;
        }

        public GrantTokenOperation GrantToken()
        {
            GrantTokenOperation grantOperation = new GrantTokenOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return grantOperation;
        }

        public RevokeTokenOperation RevokeToken()
        {
            RevokeTokenOperation revokeTokenOperation = new RevokeTokenOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return revokeTokenOperation;
        }

        public GrantOperation Grant()
        {
            GrantOperation grantOperation =
                new GrantOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  this);
            grantOperation.CurrentPubnubInstance(this);
            return grantOperation;
        }

        public SetStateOperation SetPresenceState()
        {
            SetStateOperation setStateOperation = new SetStateOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return setStateOperation;
        }

        public GetStateOperation GetPresenceState()
        {
            GetStateOperation getStateOperation = new GetStateOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            getStateOperation.CurrentPubnubInstance(this);
            return getStateOperation;
        }

        public AddPushChannelOperation AddPushNotificationsOnChannels()
        {
            AddPushChannelOperation addPushChannelOperation = new AddPushChannelOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return addPushChannelOperation;
        }

        public RemovePushChannelOperation RemovePushNotificationsFromChannels()
        {
            RemovePushChannelOperation removePushChannelOperation =
                new RemovePushChannelOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return removePushChannelOperation;
        }

        public RemoveAllPushChannelsOperation RemoveAllPushNotificationsFromDeviceWithPushToken()
        {
            RemoveAllPushChannelsOperation removeAllPushChannelsOperation =
                new RemoveAllPushChannelsOperation(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, this);
            removeAllPushChannelsOperation.CurrentPubnubInstance(this);
            return removeAllPushChannelsOperation;
        }

        public AuditPushChannelOperation AuditPushChannelProvisions()
        {
            AuditPushChannelOperation auditPushChannelOperation = new AuditPushChannelOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            auditPushChannelOperation.CurrentPubnubInstance(this);
            return auditPushChannelOperation;
        }

        public SetUuidMetadataOperation SetUuidMetadata()
        {
            SetUuidMetadataOperation setUuidMetadataOperation = new SetUuidMetadataOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return setUuidMetadataOperation;
        }

        public RemoveUuidMetadataOperation RemoveUuidMetadata()
        {
            RemoveUuidMetadataOperation removeUuidMetadataOperation =
                new RemoveUuidMetadataOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return removeUuidMetadataOperation;
        }

        public GetAllUuidMetadataOperation GetAllUuidMetadata()
        {
            GetAllUuidMetadataOperation getAllUuidMetadataOperation =
                new GetAllUuidMetadataOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return getAllUuidMetadataOperation;
        }

        public GetUuidMetadataOperation GetUuidMetadata()
        {
            GetUuidMetadataOperation getUuidMetadataOperation = new GetUuidMetadataOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return getUuidMetadataOperation;
        }

        public SetChannelMetadataOperation SetChannelMetadata()
        {
            SetChannelMetadataOperation setChannelMetadataOperation =
                new SetChannelMetadataOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return setChannelMetadataOperation;
        }

        public RemoveChannelMetadataOperation RemoveChannelMetadata()
        {
            RemoveChannelMetadataOperation removeChannelMetadataOperation =
                new RemoveChannelMetadataOperation(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, this);
            return removeChannelMetadataOperation;
        }

        public GetAllChannelMetadataOperation GetAllChannelMetadata()
        {
            GetAllChannelMetadataOperation getAllChannelMetadataOperation =
                new GetAllChannelMetadataOperation(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, this);
            return getAllChannelMetadataOperation;
        }

        public GetChannelMetadataOperation GetChannelMetadata()
        {
            GetChannelMetadataOperation getSingleSpaceOperation =
                new GetChannelMetadataOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return getSingleSpaceOperation;
        }

        public GetMembershipsOperation GetMemberships()
        {
            GetMembershipsOperation getMembershipOperation = new GetMembershipsOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return getMembershipOperation;
        }

        public SetMembershipsOperation SetMemberships()
        {
            SetMembershipsOperation setMembershipsOperation = new SetMembershipsOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return setMembershipsOperation;
        }

        public RemoveMembershipsOperation RemoveMemberships()
        {
            RemoveMembershipsOperation removeMembershipsOperation =
                new RemoveMembershipsOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return removeMembershipsOperation;
        }

        public ManageMembershipsOperation ManageMemberships()
        {
            ManageMembershipsOperation manageMembershipsOperation =
                new ManageMembershipsOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return manageMembershipsOperation;
        }

        public GetChannelMembersOperation GetChannelMembers()
        {
            GetChannelMembersOperation getChannelMembersOperation =
                new GetChannelMembersOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return getChannelMembersOperation;
        }

        public SetChannelMembersOperation SetChannelMembers()
        {
            SetChannelMembersOperation setChannelMembersOperation =
                new SetChannelMembersOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return setChannelMembersOperation;
        }

        public RemoveChannelMembersOperation RemoveChannelMembers()
        {
            RemoveChannelMembersOperation removeChannelMembersOperation =
                new RemoveChannelMembersOperation(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, this);
            return removeChannelMembersOperation;
        }

        public ManageChannelMembersOperation ManageChannelMembers()
        {
            ManageChannelMembersOperation channelMembersOperation =
                new ManageChannelMembersOperation(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, this);
            return channelMembersOperation;
        }

        public AddMessageActionOperation AddMessageAction()
        {
            AddMessageActionOperation addMessageActionOperation = new AddMessageActionOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return addMessageActionOperation;
        }

        public RemoveMessageActionOperation RemoveMessageAction()
        {
            RemoveMessageActionOperation removeMessageActionOperation =
                new RemoveMessageActionOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return removeMessageActionOperation;
        }

        public GetMessageActionsOperation GetMessageActions()
        {
            GetMessageActionsOperation getMessageActionsOperation =
                new GetMessageActionsOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return getMessageActionsOperation;
        }

        #endregion

        #region "PubNub API Channel Group Methods"

        public AddChannelsToChannelGroupOperation AddChannelsToChannelGroup()
        {
            AddChannelsToChannelGroupOperation addChannelToChannelGroupOperation =
                new AddChannelsToChannelGroupOperation(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, this);
            addChannelToChannelGroupOperation.CurrentPubnubInstance(this);
            return addChannelToChannelGroupOperation;
        }

        public RemoveChannelsFromChannelGroupOperation RemoveChannelsFromChannelGroup()
        {
            RemoveChannelsFromChannelGroupOperation removeChannelsFromChannelGroupOperation =
                new RemoveChannelsFromChannelGroupOperation(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, this);
            removeChannelsFromChannelGroupOperation.CurrentPubnubInstance(this);
            return removeChannelsFromChannelGroupOperation;
        }

        public DeleteChannelGroupOperation DeleteChannelGroup()
        {
            DeleteChannelGroupOperation deleteChannelGroupOperation =
                new DeleteChannelGroupOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            deleteChannelGroupOperation.CurrentPubnubInstance(this);
            return deleteChannelGroupOperation;
        }

        public ListChannelsForChannelGroupOperation ListChannelsForChannelGroup()
        {
            ListChannelsForChannelGroupOperation listChannelsForChannelGroupOperation =
                new ListChannelsForChannelGroupOperation(
                    pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                    pubnubUnitTest,  tokenManager, this);
            listChannelsForChannelGroupOperation.CurrentPubnubInstance(this);
            return listChannelsForChannelGroupOperation;
        }

        public ListAllChannelGroupOperation ListChannelGroups()
        {
            ListAllChannelGroupOperation listAllChannelGroupOperation =
                new ListAllChannelGroupOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            listAllChannelGroupOperation.CurrentPubnubInstance(this);
            return listAllChannelGroupOperation;
        }

        public bool AddListener(SubscribeCallback listener)
        {
            if (pubnubConfig[InstanceId].EnableEventEngine)
            {
                subscribeCallbackListenerList.Add(listener);
                return true;
            }
            else
            {
                if (listenerManager == null)
                {
                    listenerManager =
                        new ListenerManager(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                            JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
                }

                return listenerManager.AddListener(listener);
            }
        }

        public bool RemoveListener(SubscribeCallback listener)
        {
            bool ret = false;
            if (subscribeCallbackListenerList != null)
            {
                ret = subscribeCallbackListenerList.Remove(listener);
            }

            if (listenerManager != null)
            {
                var removeFromListenerManager = listenerManager.RemoveListener(listener);
                if (!ret) ret = ret || removeFromListenerManager;
            }

            return ret;
        }

        #endregion

        public SendFileOperation SendFile()
        {
            SendFileOperation uploadFileOperation = new SendFileOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return uploadFileOperation;
        }

        public GetFileUrlOperation GetFileUrl()
        {
            GetFileUrlOperation getFileUrlOperation = new GetFileUrlOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return getFileUrlOperation;
        }

        public DownloadFileOperation DownloadFile()
        {
            DownloadFileOperation downloadFileOperation = new DownloadFileOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return downloadFileOperation;
        }

        public ListFilesOperation ListFiles()
        {
            ListFilesOperation listFilesOperation = new ListFilesOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return listFilesOperation;
        }

        public DeleteFileOperation DeleteFile()
        {
            DeleteFileOperation deleteFileOperation = new DeleteFileOperation(
                pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null, JsonPluggableLibrary,
                pubnubUnitTest,  tokenManager, this);
            return deleteFileOperation;
        }

        public PublishFileMessageOperation PublishFileMessage()
        {
            PublishFileMessageOperation publshFileMessageOperation =
                new PublishFileMessageOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            return publshFileMessageOperation;
        }

        #region "PubNub API Other Methods"

        public void TerminateCurrentSubscriberRequest()
        {
            OtherOperation endpoint =
                new OtherOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            endpoint.CurrentPubnubInstance(this);
            endpoint.TerminateCurrentSubscriberRequest();
        }

        public void EnableMachineSleepModeForTestingOnly()
        {
            PubnubCoreBase.EnableMachineSleepModeForTestingOnly();
        }

        public void DisableMachineSleepModeForTestingOnly()
        {
            PubnubCoreBase.DisableMachineSleepModeForTestingOnly();
        }

        public Guid GenerateGuid()
        {
            return Guid.NewGuid();
        }

        [Obsolete("ChangeUUID is deprecated, please use ChangeUserId instead.")]
        public void ChangeUUID(string newUUID)
        {
            ChangeUserId(new UserId(newUUID));
        }

        public void ChangeUserId(UserId newUserId)
        {
            if (newUserId == null || string.IsNullOrEmpty(newUserId.ToString().Trim()))
            {
                if (pubnubLog != null && pubnubConfig != null)
                {
                    logger?.Debug($" Error: UserId cannot be null/empty.");
                }
                throw new MissingMemberException("UserId cannot be null/empty");
            }

            PNConfig.UserId = newUserId;
            OtherOperation endPoint =
                new OtherOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            endPoint.CurrentPubnubInstance(this);
        }

        public static long TranslateDateTimeToPubnubUnixNanoSeconds(DateTime dotNetUTCDateTime)
        {
            return OtherOperation.TranslateDateTimeToPubnubUnixNanoSeconds(dotNetUTCDateTime);
        }

        public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(long unixNanoSecondTime)
        {
            return OtherOperation.TranslatePubnubUnixNanoSecondsToDateTime(unixNanoSecondTime);
        }

        public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(string unixNanoSecondTime)
        {
            return OtherOperation.TranslatePubnubUnixNanoSecondsToDateTime(unixNanoSecondTime);
        }

        public UserId GetCurrentUserId()
        {
            OtherOperation endPoint =
                new OtherOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            endPoint.CurrentPubnubInstance(this);
            return endPoint.GetCurrentUserId();
        }

        public List<string> GetSubscribedChannels()
        {
            if (pubnubConfig[InstanceId].EnableEventEngine)
            {
                return this.subscribeEventEngineFactory.GetEventEngine(InstanceId).Channels;
            }

            OtherOperation endpoint =
                new OtherOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            endpoint.CurrentPubnubInstance(this);
            return endpoint.GetSubscribedChannels();
        }

        public List<string> GetSubscribedChannelGroups()
        {
            if (pubnubConfig[InstanceId].EnableEventEngine)
            {
                return this.subscribeEventEngineFactory.GetEventEngine(InstanceId).ChannelGroups;
            }

            OtherOperation endpoint =
                new OtherOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            endpoint.CurrentPubnubInstance(this);
            return endpoint.GetSubscribedChannelGroups();
        }

        public void Destroy()
        {
            savedSubscribeOperation = null;
            OtherOperation endpoint =
                new OtherOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                    JsonPluggableLibrary, pubnubUnitTest,  tokenManager, this);
            endpoint.CurrentPubnubInstance(this);
            endpoint.EndPendingRequests();
        }

        /// <summary>
        /// Parses the token and provides token details. This is client only method (works without secret key)
        /// </summary>
        /// <param name="token">string</param>
        /// <returns>PNTokenContent</returns>
        public PNTokenContent ParseToken(string token)
        {
            PNTokenContent result = null;
            if (tokenManager != null)
            {
                result = tokenManager.ParseToken(token);
            }

            return result;
        }

        /// <summary>
        /// Sets the auth token.  This is client only method (works without secret key)
        /// </summary>
        /// <param name="token"></param>
        public void SetAuthToken(string token)
        {
            if (tokenManager != null)
            {
                tokenManager.SetAuthToken(token);
            }
        }

        public bool Reconnect<T>()
        {
            bool ret = false;
            if (pubnubConfig[InstanceId].EnableEventEngine)
            {
                if (subscribeEventEngineFactory.HasEventEngine(InstanceId))
                {
                    var subscribeEventEngine = subscribeEventEngineFactory.GetEventEngine(InstanceId);

                    subscribeEventEngine.EventQueue.Enqueue(new ReconnectEvent()
                    {
                        Channels = (subscribeEventEngine.CurrentState as SubscriptionState).Channels,
                        ChannelGroups = (subscribeEventEngine.CurrentState as SubscriptionState).ChannelGroups,
                        Cursor = (subscribeEventEngine.CurrentState as SubscriptionState).Cursor
                    });
                }

                if (presenceEventengineFactory.HasEventEngine(InstanceId))
                {
                    var presenceEventEngine = presenceEventengineFactory.GetEventEngine(InstanceId);

                    presenceEventEngine.EventQueue.Enqueue(new EventEngine.Presence.Events.ReconnectEvent()
                    {
                        Input = new EventEngine.Presence.Common.PresenceInput()
                        {
                            Channels = (presenceEventEngine.CurrentState as EventEngine.Presence.States.APresenceState)
                                .Input.Channels,
                            ChannelGroups =
                                (presenceEventEngine.CurrentState as EventEngine.Presence.States.APresenceState).Input
                                .ChannelGroups
                        }
                    });
                }
            }
            else
            {
                if (savedSubscribeOperation is SubscribeOperation<T>)
                {
                    SubscribeOperation<T> subscibeOperationInstance = savedSubscribeOperation as SubscribeOperation<T>;
                    if (subscibeOperationInstance != null)
                    {
                        ret = subscibeOperationInstance.Retry(true, false);
                    }
                }
            }

            return ret;
        }

        public bool Reconnect<T>(bool resetSubscribeTimetoken)
        {
            bool ret = false;
            if (pubnubConfig[InstanceId].EnableEventEngine)
            {
                if (subscribeEventEngineFactory.HasEventEngine(InstanceId))
                {
                    var subscribeEventEngine = subscribeEventEngineFactory.GetEventEngine(InstanceId);
                    subscribeEventEngine.EventQueue.Enqueue(new ReconnectEvent()
                    {
                        Channels = (subscribeEventEngine.CurrentState as SubscriptionState).Channels,
                        ChannelGroups = (subscribeEventEngine.CurrentState as SubscriptionState).ChannelGroups,
                        Cursor = resetSubscribeTimetoken
                            ? null
                            : (subscribeEventEngine.CurrentState as SubscriptionState).Cursor
                    });
                }

                if (presenceEventengineFactory.HasEventEngine(InstanceId))
                {
                    var presenceEventEngine = presenceEventengineFactory.GetEventEngine(InstanceId);

                    presenceEventEngine.EventQueue.Enqueue(new EventEngine.Presence.Events.ReconnectEvent()
                    {
                        Input = new EventEngine.Presence.Common.PresenceInput()
                        {
                            Channels = (presenceEventEngine.CurrentState as EventEngine.Presence.States.APresenceState)
                                .Input.Channels,
                            ChannelGroups =
                                (presenceEventEngine.CurrentState as EventEngine.Presence.States.APresenceState).Input
                                .ChannelGroups
                        }
                    });
                }
            }
            else
            {
                if (savedSubscribeOperation is SubscribeOperation<T>)
                {
                    SubscribeOperation<T> subscibeOperationInstance = savedSubscribeOperation as SubscribeOperation<T>;
                    if (subscibeOperationInstance != null)
                    {
                        ret = subscibeOperationInstance.Retry(true, resetSubscribeTimetoken);
                    }
                }
            }

            return ret;
        }

        public bool Disconnect<T>()
        {
            bool ret = false;
            if (pubnubConfig[InstanceId].EnableEventEngine)
            {
                if (subscribeEventEngineFactory.HasEventEngine(InstanceId))
                {
                    var subscribeEventEngine = subscribeEventEngineFactory.GetEventEngine(InstanceId);
                    subscribeEventEngine.EventQueue.Enqueue(new DisconnectEvent()
                    {
                        Channels = (subscribeEventEngine.CurrentState as SubscriptionState).Channels,
                        ChannelGroups = (subscribeEventEngine.CurrentState as SubscriptionState).ChannelGroups
                    });
                }

                if (presenceEventengineFactory.HasEventEngine(InstanceId))
                {
                    var presenceEventEngine = presenceEventengineFactory.GetEventEngine(InstanceId);

                    presenceEventEngine.EventQueue.Enqueue(new EventEngine.Presence.Events.DisconnectEvent());
                }
            }
            else
            {
                if (savedSubscribeOperation is SubscribeOperation<T>)
                {
                    SubscribeOperation<T> subscibeOperationInstance = savedSubscribeOperation as SubscribeOperation<T>;
                    if (subscibeOperationInstance != null)
                    {
                        ret = subscibeOperationInstance.Retry(false);
                    }
                }
            }

            return ret;
        }

        public string Decrypt(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("inputString is not valid");
            }

            if (pubnubConfig.ContainsKey(InstanceId))
            {
                if (pubnubConfig[InstanceId] == null || (string.IsNullOrEmpty(pubnubConfig[InstanceId].CipherKey) &&
                                                         pubnubConfig[InstanceId].CryptoModule == null))
                {
                    throw new ArgumentException("CryptoModule missing");
                }

                pubnubConfig[InstanceId].CryptoModule ??= new CryptoModule(
                    new LegacyCryptor(pubnubConfig[InstanceId].CipherKey,
                        pubnubConfig[InstanceId].UseRandomInitializationVector), null);
                return pubnubConfig[InstanceId].CryptoModule.Decrypt(inputString);
            }
            else
            {
                throw new ArgumentException("CryptoModule missing");
            }
        }

        public string Decrypt(string inputString, string cipherKey)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("inputString is not valid");
            }

            return new CryptoModule(new LegacyCryptor(cipherKey, true), null).Decrypt(inputString);
        }

        public string Encrypt(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("inputString is not valid");
            }

            if (pubnubConfig.ContainsKey(InstanceId))
            {
                if (pubnubConfig[InstanceId] == null || (string.IsNullOrEmpty(pubnubConfig[InstanceId].CipherKey) &&
                                                         pubnubConfig[InstanceId].CryptoModule == null))
                {
                    throw new MissingMemberException("CryptoModule missing");
                }

                pubnubConfig[InstanceId].CryptoModule ??= new CryptoModule(
                    new LegacyCryptor(pubnubConfig[InstanceId].CipherKey,
                        pubnubConfig[InstanceId].UseRandomInitializationVector), null);
                return pubnubConfig[InstanceId].CryptoModule.Encrypt(inputString);
            }
            else
            {
                throw new MissingMemberException("CryptoModule missing");
            }
        }

        public string Encrypt(string inputString, string cipherKey)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("inputString is not valid");
            }

            return new CryptoModule(new LegacyCryptor(cipherKey, true), null).Encrypt(inputString);
        }

        public byte[] EncryptFile(byte[] inputBytes)
        {
            if (inputBytes == null)
            {
                throw new ArgumentException("inputBytes is not valid");
            }

            if (pubnubConfig.ContainsKey(InstanceId))
            {
                if (pubnubConfig[InstanceId] == null || (string.IsNullOrEmpty(pubnubConfig[InstanceId].CipherKey) &&
                                                         pubnubConfig[InstanceId].CryptoModule == null))
                {
                    throw new MissingMemberException("CryptoModule missing");
                }

                pubnubConfig[InstanceId].CryptoModule ??=
                    new CryptoModule(new LegacyCryptor(pubnubConfig[InstanceId].CipherKey, true), null);
                return pubnubConfig[InstanceId].CryptoModule.Encrypt(inputBytes);
            }
            else
            {
                throw new ArgumentException("CryptoModule missing");
            }
        }

        public byte[] EncryptFile(byte[] inputBytes, string cipherKey)
        {
            if (inputBytes == null)
            {
                throw new ArgumentException("inputBytes is not valid");
            }

            return new CryptoModule(new LegacyCryptor(cipherKey, true), null).Encrypt(inputBytes);
        }

        public void EncryptFile(string sourceFile, string destinationFile)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("sourceFile is not valid");
            }

            if (pubnubConfig.ContainsKey(InstanceId))
            {
                if (pubnubConfig[InstanceId] == null || (string.IsNullOrEmpty(pubnubConfig[InstanceId].CipherKey) &&
                                                         pubnubConfig[InstanceId].CryptoModule == null))
                {
                    throw new MissingMemberException("CryptoModule missing");
                }
#if !NETSTANDARD10 && !NETSTANDARD11
                bool validSource = System.IO.File.Exists(sourceFile);
                if (!validSource)
                {
                    throw new ArgumentException("sourceFile is not valid");
                }

                string destDirectory = System.IO.Path.GetDirectoryName(destinationFile);
                bool validDest = System.IO.Directory.Exists(destDirectory);
                if (!string.IsNullOrEmpty(destDirectory) && !validDest)
                {
                    throw new ArgumentException("destination path is not valid");
                }

                byte[] inputBytes = System.IO.File.ReadAllBytes(sourceFile);
                byte[] outputBytes = EncryptFile(inputBytes);
                System.IO.File.WriteAllBytes(destinationFile, outputBytes);
#else
                throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
#endif
            }
            else
            {
                throw new ArgumentException("CryptoModule missing");
            }
        }

        public void EncryptFile(string sourceFile, string destinationFile, string cipherKey)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("sourceFile is not valid");
            }

            bool validSource = System.IO.File.Exists(sourceFile);
            if (!validSource)
            {
                throw new ArgumentException("sourceFile is not valid");
            }

            string destDirectory = System.IO.Path.GetDirectoryName(destinationFile);
            bool validDest = System.IO.Directory.Exists(destDirectory);
            if (!string.IsNullOrEmpty(destDirectory) && !validDest)
            {
                throw new ArgumentException("destination path is not valid");
            }

            byte[] inputBytes = System.IO.File.ReadAllBytes(sourceFile);
            byte[] outputBytes = EncryptFile(inputBytes, cipherKey);
            System.IO.File.WriteAllBytes(destinationFile, outputBytes);
        }

        public byte[] DecryptFile(byte[] inputBytes)
        {
            if (inputBytes == null)
            {
                throw new ArgumentException("inputBytes is not valid");
            }

            if (pubnubConfig.ContainsKey(InstanceId))
            {
                if (pubnubConfig[InstanceId] == null || (string.IsNullOrEmpty(pubnubConfig[InstanceId].CipherKey) &&
                                                         pubnubConfig[InstanceId].CryptoModule == null))
                {
                    throw new ArgumentException("CryptoModule missing");
                }

                pubnubConfig[InstanceId].CryptoModule ??= new CryptoModule(
                    new LegacyCryptor(pubnubConfig[InstanceId].CipherKey,
                        pubnubConfig[InstanceId].UseRandomInitializationVector), null);
                return pubnubConfig[InstanceId].CryptoModule.Decrypt(inputBytes);
            }
            else
            {
                throw new ArgumentException("CryptoModule missing");
            }
        }

        public void DecryptFile(string sourceFile, string destinationFile)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("sourceFile is not valid");
            }

            if (pubnubConfig.ContainsKey(InstanceId))
            {
                if (pubnubConfig[InstanceId] == null || (string.IsNullOrEmpty(pubnubConfig[InstanceId].CipherKey) &&
                                                         pubnubConfig[InstanceId].CryptoModule == null))
                {
                    throw new ArgumentException("CryptoModule missing");
                }

                bool validSource = System.IO.File.Exists(sourceFile);
                if (!validSource)
                {
                    throw new ArgumentException("sourceFile is not valid");
                }

                string destDirectory = System.IO.Path.GetDirectoryName(destinationFile);
                bool validDest = System.IO.Directory.Exists(destDirectory);
                if (!string.IsNullOrEmpty(destDirectory) && !validDest)
                {
                    throw new ArgumentException("destination path is not valid");
                }

                byte[] inputBytes = System.IO.File.ReadAllBytes(sourceFile);
                byte[] outputBytes = DecryptFile(inputBytes);
                System.IO.File.WriteAllBytes(destinationFile, outputBytes);
            }
            else
            {
                throw new ArgumentException("CryptoModule missing");
            }
        }

        public byte[] DecryptFile(byte[] inputBytes, string cipherKey)
        {
            if (inputBytes == null)
            {
                throw new ArgumentException("inputBytes is not valid");
            }

            return new CryptoModule(new LegacyCryptor(cipherKey, true), null).Decrypt(inputBytes);
        }

        public void DecryptFile(string sourceFile, string destinationFile, string cipherKey)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("inputFile is not valid");
            }

            bool validSource = System.IO.File.Exists(sourceFile);
            if (!validSource)
            {
                throw new ArgumentException("sourceFile is not valid");
            }

            string destDirectory = System.IO.Path.GetDirectoryName(destinationFile);
            bool validDest = System.IO.Directory.Exists(destDirectory);
            if (!string.IsNullOrEmpty(destDirectory) && !validDest)
            {
                throw new ArgumentException("destination path is not valid");
            }

            byte[] inputBytes = System.IO.File.ReadAllBytes(sourceFile);
            byte[] outputBytes = DecryptFile(inputBytes, cipherKey);
            System.IO.File.WriteAllBytes(destinationFile, outputBytes);
        }
        
        /// <summary>
        /// Configures a custom logger.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public void SetLogger(IPubnubLogger logger)
        {
            this.logger?.AddLogger(logger);
            logger?.Debug(GetConfigurationLogString(pubnubConfig[InstanceId]));
        }

        public void RemoveLogger(IPubnubLogger logger) => this.logger?.RemoveLogger(logger);

        #endregion

        #region "Properties"

        public IPubnubUnitTest PubnubUnitTest
        {
            get => pubnubUnitTest;
            set
            {
                pubnubUnitTest = value;
                Version = pubnubUnitTest != null ? pubnubUnitTest.SdkVersion : savedSdkVerion;
            }
        }

        public PNConfiguration PNConfig => pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null;

        public IJsonPluggableLibrary JsonPluggableLibrary { get; internal set; }

        public void SetJsonPluggableLibrary(IJsonPluggableLibrary customJson)
        {
            JsonPluggableLibrary = customJson;
        }

        public string Version { get; private set; }

        internal readonly ITransportMiddleware transportMiddleware;

        public string InstanceId { get; private set; }

        public Channel Channel(string name) => new Channel(name, this, eventEmitter);
        public ChannelGroup ChannelGroup(string name) => new ChannelGroup(name, this, eventEmitter);
        public ChannelMetadata ChannelMetadata(string id) => new ChannelMetadata(id, this, eventEmitter);
        public UserMetadata UserMetadata(string id) => new UserMetadata(id, this, eventEmitter);

        public SubscriptionSet SubscriptionSet(string[] channels, string[] channelGroups = null,
            SubscriptionOptions? options = null) => new SubscriptionSet(channels, channelGroups ?? new string[] { },
            options, this, eventEmitter);

        #endregion

        #region "Constructors"

        public Pubnub(PNConfiguration config, IHttpClientService httpTransportService = default,
            ITransportMiddleware middleware = default, IPNSDKSource ipnsdkSource = default)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
#if UNITY
            OnCleanupCall += this.UnsubscribeAll<object>;
#endif
            logger = InitializeLogger(config);
            config.Logger = logger;
            pubnubLog = config.PubnubLog;
            savedSdkVerion = Version;
            InstanceId = Guid.NewGuid().ToString();
            subscribeEventEngineFactory = new SubscribeEventEngineFactory();
            presenceEventengineFactory = new PresenceEventEngineFactory();
            pubnubConfig.AddOrUpdate(InstanceId, config, (k, o) => config);

            CheckAndInitializeEmptyStringValues(config);
            tokenManager = new TokenManager(pubnubConfig[InstanceId], JsonPluggableLibrary,  InstanceId);

            JsonPluggableLibrary = new NewtonsoftJsonDotNet(config);

            if (config.PresenceTimeout < 20)
            {
                config.PresenceTimeout = 20;

                config.Logger?.Warn(
                    $"The PresenceTimeout cannot be less than 20, defaulting the value to 20. Please update the settings in your code.");
            }

            CheckRequiredUserId(config);
            eventEmitter = new EventEmitter(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
                subscribeCallbackListenerList, JsonPluggableLibrary, tokenManager, this);
            CheckCryptoModuleUsageForLogging(config);
            //Defaulting to DotNet PNSDK source if no custom one is specified
            Version = (ipnsdkSource == default) ? new DotNetPNSDKSource().GetPNSDK() : ipnsdkSource.GetPNSDK();
            IHttpClientService httpClientService =
                httpTransportService ?? new HttpClientService(proxy: config.Proxy, configuration: config);
            transportMiddleware = middleware ?? new Middleware(httpClientService, config, this, tokenManager);
            logger?.Debug(GetConfigurationLogString(config));
        }

#if UNITY
        ~Pubnub()
        {
            OnCleanupCall -= this.UnsubscribeAll<object>;
        }
#endif
        private string GetConfigurationLogString(PNConfiguration config) =>
            $"Pubnub instance initialised with\n" +
            $"UserId {config.UserId}\n" +
            $"SubscribeKey {config.SubscribeKey}\n" +
            $"PublishKey {config.PublishKey}\n" +
            $"LogLevel {config.LogLevel}\n" +
            $"ReconnectionPolicy {config.RetryConfiguration.RetryPolicy}\n" +
            $"PresenceTimeout {config.PresenceTimeout}\n" +
            $"SubscribeTimeout {config.SubscribeTimeout}\n" +
            $"Origin {config?.Origin}\n" +
            $"Is CryptoModule initialised {(config?.CryptoModule == null ? bool.FalseString : bool.TrueString)}\n" +
            $"Is secretKey provided {(string.IsNullOrEmpty(config?.SecretKey) ? bool.FalseString : bool.TrueString)}\n" +
            $"Is AuthKey provided {(string.IsNullOrEmpty(config?.AuthKey) ? bool.FalseString : bool.TrueString)}\n" +
            $"Proxy {config.Proxy}\n" +
            $"FilterExpression {config.FilterExpression}\n" +
            $"EnableEventEngine {config.EnableEventEngine}\n" +
            $"MaintainPresenceState {config.MaintainPresenceState}\n";

        private PubnubLogModule InitializeLogger(PNConfiguration configuration)
        {
            var defaultLogger = new PubnubDefaultLogger($"{GetHashCode()}",
                (configuration.LogVerbosity == PNLogVerbosity.BODY && configuration.PubnubLog != null
                    ? configuration.PubnubLog
                    : null));
            return new PubnubLogModule(configuration.LogLevel, [defaultLogger]);
        }

        private void CheckRequiredUserId(PNConfiguration config)
        {
            if (config.UserId == null || string.IsNullOrEmpty(config.UserId.ToString()))
            {
                if (pubnubLog != null)
                {
                    config.Logger?.Warn($"PNConfiguration.Uuid or PNConfiguration.UserId is required to use the SDK.");
                }

                throw new MissingMemberException("PNConfiguration.UserId is required to use the SDK");
            }

            config.ResetUuidSetFromConstructor();
        }

        private void CheckCryptoModuleUsageForLogging(PNConfiguration config)
        {
            if (config.CryptoModule != null && !string.IsNullOrEmpty(config.CipherKey) && config.CipherKey.Length > 0)
            {
                config.Logger?.Debug($"CryptoModule takes precedence over CipherKey.");
            }
        }

        private void CheckAndInitializeEmptyStringValues(PNConfiguration config)
        {
            config.SubscribeKey = string.IsNullOrEmpty(config.SubscribeKey) ? string.Empty : config.SubscribeKey;
            config.PublishKey = string.IsNullOrEmpty(config.PublishKey) ? string.Empty : config.PublishKey;
            config.SecretKey = string.IsNullOrEmpty(config.SecretKey) ? string.Empty : config.SecretKey;
            config.CipherKey = string.IsNullOrEmpty(config.CipherKey) ? string.Empty : config.CipherKey;
        }

        #endregion
    }
}