using System;

namespace PubNubMessaging.Core
{
    public class Pubnub
    {

        #region "PubNub API Channel Methods"

        public void Subscribe<T> (string channel, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Subscribe<T> (channel, userCallback, connectCallback, errorCallback);
        }

        public void Subscribe (string channel, Action<object> userCallback, Action<object> connectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Subscribe (channel, userCallback, connectCallback, errorCallback);
        }

        public bool Publish(string channel, object message, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.Publish(channel, message, true, userCallback, errorCallback);
        }

        public bool Publish<T>(string channel, object message, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.Publish<T>(channel, message, true, userCallback, errorCallback);
        }

        public bool Publish(string channel, object message, bool storeInHistory, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.Publish(channel, message, storeInHistory, userCallback, errorCallback);
        }

        public bool Publish<T>(string channel, object message, bool storeInHistory, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.Publish<T>(channel, message, storeInHistory, userCallback, errorCallback);
        }

        public void Presence<T> (string channel, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Presence<T> (channel, userCallback, connectCallback, errorCallback);
        }

        public void Presence (string channel, Action<object> userCallback, Action<object> connectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Presence (channel, userCallback, connectCallback, errorCallback);
        }

        public bool DetailedHistory (string channel, long start, long end, int count, bool reverse, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.DetailedHistory (channel, start, end, count, reverse, userCallback, errorCallback);
        }

        public bool DetailedHistory<T> (string channel, long start, long end, int count, bool reverse, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.DetailedHistory<T> (channel, start, end, count, reverse, userCallback, errorCallback);
        }

        public bool DetailedHistory (string channel, long start, Action<object> userCallback, Action<PubnubClientError> errorCallback, bool reverse)
        {
            return DetailedHistory<object> (channel, start, -1, -1, reverse, userCallback, errorCallback);
        }

        public bool DetailedHistory<T> (string channel, long start, Action<T> userCallback, Action<PubnubClientError> errorCallback, bool reverse)
        {
            return DetailedHistory<T> (channel, start, -1, -1, reverse, userCallback, errorCallback);
        }

        public bool DetailedHistory (string channel, int count, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return DetailedHistory<object> (channel, -1, -1, count, false, userCallback, errorCallback);
        }

        public bool DetailedHistory<T> (string channel, int count, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return DetailedHistory<T> (channel, -1, -1, count, false, userCallback, errorCallback);
        }

        public bool HereNow (string channel, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.HereNow (channel, userCallback, errorCallback);
        }

        public bool HereNow (string channel, bool showUUIDList, bool includeUserState, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.HereNow (channel, showUUIDList, includeUserState, userCallback, errorCallback);
        }

        public bool HereNow<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.HereNow<T> (channel, userCallback, errorCallback);
        }

        public bool HereNow<T> (string channel, bool showUUIDList, bool includeUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.HereNow<T> (channel, showUUIDList, includeUserState, userCallback, errorCallback);
        }

        public void GlobalHereNow (bool showUUIDList, bool includeUserState, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GlobalHereNow (showUUIDList, includeUserState, userCallback, errorCallback);
        }

        public bool GlobalHereNow<T> (bool showUUIDList, bool includeUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GlobalHereNow<T> (showUUIDList, includeUserState, userCallback, errorCallback);
        }

        public void WhereNow (string uuid, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.WhereNow (uuid, userCallback, errorCallback);
        }

        public void WhereNow<T> (string uuid, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.WhereNow<T> (uuid, userCallback, errorCallback);
        }

        public void Unsubscribe<T> (string channel, Action<T> userCallback, Action<T> connectCallback, Action<T> disconnectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Unsubscribe<T> (channel, userCallback, connectCallback, disconnectCallback, errorCallback);
        }

        public void Unsubscribe (string channel, Action<object> userCallback, Action<object> connectCallback, Action<object> disconnectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Unsubscribe (channel, userCallback, connectCallback, disconnectCallback, errorCallback);
        }

        public void PresenceUnsubscribe (string channel, Action<object> userCallback, Action<object> connectCallback, Action<object> disconnectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.PresenceUnsubscribe (channel, userCallback, connectCallback, disconnectCallback, errorCallback);
        }

        public void PresenceUnsubscribe<T> (string channel, Action<T> userCallback, Action<T> connectCallback, Action<T> disconnectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.PresenceUnsubscribe<T> (channel, userCallback, connectCallback, disconnectCallback, errorCallback);
        }

        public bool Time (Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.Time (userCallback, errorCallback);
        }

        public bool Time<T> (Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.Time<T> (userCallback, errorCallback);
        }

        public void AuditAccess<T> (string channel, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AuditAccess (channel, authenticationKey, userCallback, errorCallback);
        }

        public void AuditAccess<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AuditAccess (channel, userCallback, errorCallback);
        }

        public void AuditAccess<T> (Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AuditAccess<T> (userCallback, errorCallback);
        }

        public void AuditPresenceAccess<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AuditPresenceAccess<T> (channel, userCallback, errorCallback);
        }

        public void AuditPresenceAccess<T> (string channel, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AuditPresenceAccess<T> (channel, authenticationKey, userCallback, errorCallback);
        }

        public bool GrantAccess<T> (string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantAccess<T> (channel, read, write, ttl, userCallback, errorCallback);
        }

        public bool GrantAccess<T> (string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantAccess<T> (channel, read, write, userCallback, errorCallback);
        }

        public bool GrantAccess<T> (string channel, string authenticationKey, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantAccess<T> (channel, authenticationKey, read, write, ttl, userCallback, errorCallback);
        }

        public bool GrantAccess<T> (string channel, string authenticationKey, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantAccess<T> (channel, authenticationKey, read, write, userCallback, errorCallback);
        }

        public bool GrantPresenceAccess<T> (string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantPresenceAccess<T> (channel, read, write, userCallback, errorCallback);
        }

        public bool GrantPresenceAccess<T> (string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantPresenceAccess (channel, read, write, ttl, userCallback, errorCallback);
        }

        public bool GrantPresenceAccess<T> (string channel, string authenticationKey, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantPresenceAccess<T> (channel, authenticationKey, read, write, userCallback, errorCallback);
        }

        public bool GrantPresenceAccess<T> (string channel, string authenticationKey, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantPresenceAccess (channel, authenticationKey, read, write, ttl, userCallback, errorCallback);
        }

        public void SetUserState<T> (string channel, string uuid, string jsonUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.SetUserState<T> (channel, uuid, jsonUserState, userCallback, errorCallback);
        }

        public void SetUserState<T> (string channel, string jsonUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.SetUserState<T> (channel, "", jsonUserState, userCallback, errorCallback);
        }

        public void SetUserState<T> (string channel, string uuid, System.Collections.Generic.KeyValuePair<string, object> keyValuePair, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.SetUserState<T> (channel, uuid, keyValuePair, userCallback, errorCallback);
        }

        public void SetUserState<T> (string channel, System.Collections.Generic.KeyValuePair<string, object> keyValuePair, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.SetUserState<T> (channel, "", keyValuePair, userCallback, errorCallback);
        }

        public void GetUserState<T> (string channel, string uuid, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetUserState<T> (channel, uuid, userCallback, errorCallback);
        }

        public void GetUserState<T> (string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetUserState<T> (channel, "", userCallback, errorCallback);
        }

        #endregion

        #region "PubNub API Other Methods"

        public void TerminateCurrentSubscriberRequest ()
        {
            pubnub.TerminateCurrentSubscriberRequest ();
        }

        public void EnableSimulateNetworkFailForTestingOnly ()
        {
            pubnub.EnableSimulateNetworkFailForTestingOnly ();
        }

        public void DisableSimulateNetworkFailForTestingOnly ()
        {
            pubnub.DisableSimulateNetworkFailForTestingOnly ();
        }

        public void EnableMachineSleepModeForTestingOnly ()
        {
            pubnub.EnableMachineSleepModeForTestingOnly ();
        }

        public void DisableMachineSleepModeForTestingOnly ()
        {
            pubnub.DisableMachineSleepModeForTestingOnly ();
        }

        public void EndPendingRequests ()
        {
            pubnub.EndPendingRequests ();
        }

        public Guid GenerateGuid ()
        {
            return pubnub.GenerateGuid ();
        }

        public void ChangeUUID (string newUUID)
        {
            pubnub.ChangeUUID (newUUID);
        }

        public static long TranslateDateTimeToPubnubUnixNanoSeconds (DateTime dotNetUTCDateTime)
        {
            return PubnubUnity.TranslateDateTimeToPubnubUnixNanoSeconds (dotNetUTCDateTime);
        }

        public static DateTime TranslatePubnubUnixNanoSecondsToDateTime (long unixNanoSecondTime)
        {
            return PubnubUnity.TranslatePubnubUnixNanoSecondsToDateTime (unixNanoSecondTime);
        }

        #endregion

        #region "Properties"

        public string AuthenticationKey {
            get { return pubnub.AuthenticationKey; }
            set { pubnub.AuthenticationKey = value; }
        }

        public int LocalClientHeartbeatInterval {
            get { return pubnub.LocalClientHeartbeatInterval; }
            set { pubnub.LocalClientHeartbeatInterval = value; }
        }

        public int NetworkCheckRetryInterval {
            get { return pubnub.NetworkCheckRetryInterval; }
            set { pubnub.NetworkCheckRetryInterval = value; }
        }

        public int NetworkCheckMaxRetries {
            get { return pubnub.NetworkCheckMaxRetries; }
            set { pubnub.NetworkCheckMaxRetries = value; }
        }

        public int NonSubscribeTimeout {
            get { return pubnub.NonSubscribeTimeout; }
            set { pubnub.NonSubscribeTimeout = value; }
        }

        public int SubscribeTimeout {
            get { return pubnub.SubscribeTimeout; }
            set { pubnub.SubscribeTimeout = value; }
        }

        public bool EnableResumeOnReconnect {
            get { return pubnub.EnableResumeOnReconnect; }
            set { pubnub.EnableResumeOnReconnect = value; }
        }

        public string SessionUUID {
            get { return pubnub.SessionUUID; }
            set { pubnub.SessionUUID = value; }
        }

        public string Origin {
            get { return pubnub.Origin; }
            set { pubnub.Origin = value; }
        }

        public int PresenceHeartbeat {
            get {
                return pubnub.PresenceHeartbeat;
            }
            set {
                pubnub.PresenceHeartbeat = value;
            }
        }

        public int PresenceHeartbeatInterval {
            get {
                return pubnub.PresenceHeartbeatInterval;
            }
            set {
                pubnub.PresenceHeartbeatInterval = value;
            }
        }

        public IPubnubUnitTest PubnubUnitTest {
            get {
                return pubnub.PubnubUnitTest;
            }
            set {
                pubnub.PubnubUnitTest = value;
            }
        }

        public bool EnableJsonEncodingForPublish {
            get {
                return pubnub.EnableJsonEncodingForPublish;
            }
            set {
                pubnub.EnableJsonEncodingForPublish = value;
            }
        }

        public PubnubProxy Proxy {
            get {
                return pubnub.Proxy;
            }
            set {
                pubnub.Proxy = value;
            }
        }

        public IJsonPluggableLibrary JsonPluggableLibrary {
            get {
                return pubnub.JsonPluggableLibrary;
            }
            set {
                pubnub.JsonPluggableLibrary = value;
            }
        }

        #endregion

        #region "Constructors"

        PubnubUnity pubnub;

        public Pubnub (string publishKey, string subscribeKey, string secretKey, string cipherKey, bool sslOn)
        {
            pubnub = new PubnubUnity (publishKey, subscribeKey, secretKey, cipherKey, sslOn);
        }

        public Pubnub (string publishKey, string subscribeKey, string secretKey)
        {
            pubnub = new PubnubUnity (publishKey, subscribeKey, secretKey);
        }

        public Pubnub (string publishKey, string subscribeKey)
        {
            pubnub = new PubnubUnity (publishKey, subscribeKey);
        }

        #endregion

    }
}