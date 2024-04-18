using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Globalization;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class GrantTokenOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private PNTokenResources pubnubResources = new PNTokenResources
        { 
            Channels = new Dictionary<string, PNTokenAuthValues>(),
            Spaces = new Dictionary<string, PNTokenAuthValues>(),
            ChannelGroups = new Dictionary<string, PNTokenAuthValues>(),
            Uuids = new Dictionary<string, PNTokenAuthValues>(),
            Users = new Dictionary<string, PNTokenAuthValues>()
        };
        private PNTokenPatterns pubnubPatterns = new PNTokenPatterns
        {
            Channels = new Dictionary<string, PNTokenAuthValues>(),
            Spaces = new Dictionary<string, PNTokenAuthValues>(),
            ChannelGroups = new Dictionary<string, PNTokenAuthValues>(),
            Uuids = new Dictionary<string, PNTokenAuthValues>(),
            Users = new Dictionary<string, PNTokenAuthValues>()
        };

        private int grantTTL = -1;
        private PNCallback<PNAccessManagerTokenResult> savedCallbackGrantToken;
        private Dictionary<string, object> queryParam;
        private Dictionary<string, object> grantMeta;
        private string pubnubAuthorizedUuid = string.Empty;
        private string pubnubAuthorizedUserId = string.Empty;

        public GrantTokenOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            PubnubInstance = instance;

            InitializeDefaultVariableObjectStates();
        }

        public GrantTokenOperation AuthorizedUuid(string uuid)
        {
            if (!string.IsNullOrEmpty(pubnubAuthorizedUserId))
            {
                throw new ArgumentException("Either UUID or UserId can be used. Not both.");
            }
            pubnubAuthorizedUuid = uuid;
            return this;
        }

        public GrantTokenOperation AuthorizedUserId(UserId user)
        {
            if (!string.IsNullOrEmpty(pubnubAuthorizedUuid))
            {
                throw new ArgumentException("Either UUID or UserId can be used. Not both.");
            }
            pubnubAuthorizedUserId = user;
            return this;
        }

        public GrantTokenOperation Resources(PNTokenResources resources)
        {
            if (pubnubResources != null && resources != null)
            {
                if (resources.Channels != null && resources.Channels.Count > 0 &&
                    resources.Spaces != null && resources.Spaces.Count > 0)
                {
                    throw new ArgumentException("Either Channels or Spaces can be used. Not both.");
                }
                if (resources.Uuids != null && resources.Uuids.Count > 0 &&
                    resources.Users != null && resources.Users.Count > 0)
                {
                    throw new ArgumentException("Either Uuids or Users can be used. Not both.");
                }
                pubnubResources = resources;
                if (pubnubResources.Channels == null)
                {
                    pubnubResources.Channels = new Dictionary<string, PNTokenAuthValues>();
                }
                if (pubnubResources.Spaces == null)
                {
                    pubnubResources.Spaces = new Dictionary<string, PNTokenAuthValues>();
                }
                if (pubnubResources.ChannelGroups == null)
                {
                    pubnubResources.ChannelGroups = new Dictionary<string, PNTokenAuthValues>();
                }
                if (pubnubResources.Uuids == null)
                {
                    pubnubResources.Uuids = new Dictionary<string, PNTokenAuthValues>();
                }
                if (pubnubResources.Users == null)
                {
                    pubnubResources.Users = new Dictionary<string, PNTokenAuthValues>();
                }
            }
            return this;
        }

        public GrantTokenOperation Patterns(PNTokenPatterns patterns)
        {
            if (pubnubPatterns != null && patterns != null)
            {
                if (patterns.Channels != null && patterns.Channels.Count > 0 &&
                    patterns.Spaces != null && patterns.Spaces.Count > 0)
                {
                    throw new ArgumentException("Either Channels or Spaces can be used. Not both.");
                }
                if (patterns.Uuids != null && patterns.Uuids.Count > 0 &&
                    patterns.Users != null && patterns.Users.Count > 0)
                {
                    throw new ArgumentException("Either Uuids or Users can be used. Not both.");
                }

                pubnubPatterns = patterns;
                if (pubnubPatterns.Channels == null)
                {
                    pubnubPatterns.Channels = new Dictionary<string, PNTokenAuthValues>();
                }
                if (pubnubPatterns.Spaces == null)
                {
                    pubnubPatterns.Spaces = new Dictionary<string, PNTokenAuthValues>();
                }
                if (pubnubPatterns.ChannelGroups == null)
                {
                    pubnubPatterns.ChannelGroups = new Dictionary<string, PNTokenAuthValues>();
                }
                if (pubnubPatterns.Uuids == null)
                {
                    pubnubPatterns.Uuids = new Dictionary<string, PNTokenAuthValues>();
                }
                if (pubnubPatterns.Users == null)
                {
                    pubnubPatterns.Users = new Dictionary<string, PNTokenAuthValues>();
                }

            }
            return this;
        }

        public GrantTokenOperation TTL(int ttl)
        {
            this.grantTTL = ttl;
            return this;
        }

        public GrantTokenOperation Meta(Dictionary<string, object> metaObject)
        {
            this.grantMeta = metaObject;
            return this;
        }

        public GrantTokenOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNAccessManagerTokenResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallbackGrantToken = callback;
                GrantAccess(callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallbackGrantToken = callback;
                GrantAccess(callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNAccessManagerTokenResult>> ExecuteAsync()
        {
            return await GrantAccess().ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                GrantAccess(savedCallbackGrantToken);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                GrantAccess(savedCallbackGrantToken);
            })
            { IsBackground = true }.Start();
#endif
        }
        
        internal void GrantAccess(PNCallback<PNAccessManagerTokenResult> callback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            if (this.grantTTL <= 0)
            {
                throw new MissingMemberException("Invalid TTL value");
            }

            RequestState<PNAccessManagerTokenResult> requestState = new RequestState<PNAccessManagerTokenResult>();
            requestState.Channels = pubnubResources.Channels.Keys.ToArray();
            requestState.ChannelGroups = pubnubResources.ChannelGroups.Keys.ToArray();
            requestState.ResponseType = PNOperationType.PNAccessManagerGrantToken;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;


            requestState.UsePostMethod = true;

            bool atleastOnePermission = false;
            Dictionary<string, int> chBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Channels, atleastOnePermission, out chBitmaskPermCollection);

            Dictionary<string, int> chPatternBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Channels, atleastOnePermission, out chPatternBitmaskPermCollection);

            Dictionary<string, int> spBitmaskPermCollection = null;
            Dictionary<string, int> spPatternBitmaskPermCollection = null;
            if (pubnubResources.Channels.Count == 0 && pubnubPatterns.Channels.Count == 0)
            {
                atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Spaces, atleastOnePermission, out spBitmaskPermCollection);
                atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Spaces, atleastOnePermission, out spPatternBitmaskPermCollection);
            }
            else
            {
                spBitmaskPermCollection = new Dictionary<string, int>();
                spPatternBitmaskPermCollection = new Dictionary<string, int>();
            }

            Dictionary<string, int> cgBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.ChannelGroups, atleastOnePermission, out cgBitmaskPermCollection);

            Dictionary<string, int> cgPatternBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.ChannelGroups, atleastOnePermission, out cgPatternBitmaskPermCollection);

            Dictionary<string, int> uuidBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Uuids, atleastOnePermission, out uuidBitmaskPermCollection);

            Dictionary<string, int> uuidPatternBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Uuids, atleastOnePermission, out uuidPatternBitmaskPermCollection);

            Dictionary<string, int> userBitmaskPermCollection = null;
            Dictionary<string, int> userPatternBitmaskPermCollection = null;
            if (pubnubResources.Uuids.Count == 0 && pubnubPatterns.Uuids.Count == 0)
            {
                atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Users, atleastOnePermission, out userBitmaskPermCollection);
                atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Users, atleastOnePermission, out userPatternBitmaskPermCollection);
            }
            else
            {
                userBitmaskPermCollection = new Dictionary<string, int>();
                userPatternBitmaskPermCollection = new Dictionary<string, int>();

            }

            if (!atleastOnePermission)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} At least one permission is needed for at least one or more of uuids/users, channels/spaces or groups",DateTime.Now.ToString(CultureInfo.InvariantCulture)), PNLogVerbosity.BODY);
            }

            Dictionary<string, object> resourcesCollection = new Dictionary<string, object>();
            resourcesCollection.Add("channels", chBitmaskPermCollection);
            resourcesCollection.Add("groups", cgBitmaskPermCollection);
            resourcesCollection.Add("uuids", uuidBitmaskPermCollection);
            resourcesCollection.Add("users", userBitmaskPermCollection);
            resourcesCollection.Add("spaces", spBitmaskPermCollection);

            Dictionary<string, object> patternsCollection = new Dictionary<string, object>();
            patternsCollection.Add("channels", chPatternBitmaskPermCollection);
            patternsCollection.Add("groups", cgPatternBitmaskPermCollection);
            patternsCollection.Add("uuids", uuidPatternBitmaskPermCollection);
            patternsCollection.Add("users", userPatternBitmaskPermCollection);
            patternsCollection.Add("spaces", spPatternBitmaskPermCollection);

            Dictionary<string, object> optimizedMeta = new Dictionary<string, object>();
            if (this.grantMeta != null)
            {
                optimizedMeta = this.grantMeta;
            }

            Dictionary<string, object> permissionCollection = new Dictionary<string, object>();
            permissionCollection.Add("resources", resourcesCollection);
            permissionCollection.Add("patterns", patternsCollection);
            permissionCollection.Add("meta", optimizedMeta);
            if (!string.IsNullOrEmpty(this.pubnubAuthorizedUuid) && this.pubnubAuthorizedUuid.Trim().Length > 0)
            {
                permissionCollection.Add("uuid", this.pubnubAuthorizedUuid);
            }
            else if (!string.IsNullOrEmpty(this.pubnubAuthorizedUserId) && this.pubnubAuthorizedUserId.Trim().Length > 0)
            {
                permissionCollection.Add("uuid", this.pubnubAuthorizedUserId);
            }
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            messageEnvelope.Add("ttl", this.grantTTL);
            messageEnvelope.Add("permissions", permissionCollection);

            string requestMethodName = "POST";
            string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            byte[] postData = Encoding.UTF8.GetBytes(postMessage);
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildGrantV3AccessRequest(requestMethodName, postMessage, this.queryParam);

            UrlProcessRequest(request, requestState, false, postData).ContinueWith(r => 
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);
                    ProcessResponseCallbacks(result, requestState);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        private bool FillPermissionMappingWithMaskValues(Dictionary<string, PNTokenAuthValues> dPerms, bool currentAtleastOnePermission, out Dictionary<string, int> dPermsWithMaskValues)
        {
            dPermsWithMaskValues = new Dictionary<string, int>();
            bool internalAtleastOnePermission = currentAtleastOnePermission;
            foreach (KeyValuePair<string, PNTokenAuthValues> kvp in dPerms)
            {
                PNTokenAuthValues perm = kvp.Value;
                int bitMaskPermissionValue = 0;
                if (!string.IsNullOrEmpty(kvp.Key) && kvp.Key.Trim().Length > 0 && perm != null)
                {
                    bitMaskPermissionValue = CalculateGrantBitMaskValue(perm);
                    if (!internalAtleastOnePermission && bitMaskPermissionValue > 0) { internalAtleastOnePermission = true; }
                }
                dPermsWithMaskValues.Add(kvp.Key, bitMaskPermissionValue);
            }
            return internalAtleastOnePermission;
        }

        internal async Task<PNResult<PNAccessManagerTokenResult>> GrantAccess()
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            if (this.grantTTL <= 0)
            {
                throw new MissingMemberException("Invalid TTL value");
            }

            PNResult<PNAccessManagerTokenResult> ret = new PNResult<PNAccessManagerTokenResult>();

            bool atleastOnePermission = false;
            Dictionary<string, int> chBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Channels, atleastOnePermission, out chBitmaskPermCollection);

            Dictionary<string, int> chPatternBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Channels, atleastOnePermission, out chPatternBitmaskPermCollection);
            Dictionary<string, int> spBitmaskPermCollection = null;
            Dictionary<string, int> spPatternBitmaskPermCollection = null;
            if (pubnubResources.Channels.Count == 0 && pubnubPatterns.Channels.Count == 0)
            {
                atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Spaces, atleastOnePermission, out spBitmaskPermCollection);
                atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Spaces, atleastOnePermission, out spPatternBitmaskPermCollection);
            }
            else
            {
                spBitmaskPermCollection = new Dictionary<string, int>();
                spPatternBitmaskPermCollection = new Dictionary<string, int>();
            }

            Dictionary<string, int> cgBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.ChannelGroups, atleastOnePermission, out cgBitmaskPermCollection);

            Dictionary<string, int> cgPatternBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.ChannelGroups, atleastOnePermission, out cgPatternBitmaskPermCollection);

            Dictionary<string, int> uuidBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Uuids, atleastOnePermission, out uuidBitmaskPermCollection);

            Dictionary<string, int> uuidPatternBitmaskPermCollection = null;
            atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Uuids, atleastOnePermission, out uuidPatternBitmaskPermCollection);
            
            Dictionary<string, int> userBitmaskPermCollection = null;
            Dictionary<string, int> userPatternBitmaskPermCollection = null;
            if (pubnubResources.Uuids.Count == 0 && pubnubPatterns.Uuids.Count == 0)
            {
                atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Users, atleastOnePermission, out userBitmaskPermCollection);
                atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Users, atleastOnePermission, out userPatternBitmaskPermCollection);
            }
            else
            {
                userBitmaskPermCollection = new Dictionary<string, int>();
                userPatternBitmaskPermCollection = new Dictionary<string, int>();

            }

            if (!atleastOnePermission)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} At least one permission is needed for at least one or more of uuids, channels or groups", DateTime.Now.ToString(CultureInfo.InvariantCulture)), PNLogVerbosity.BODY);
            }

            Dictionary<string, object> resourcesCollection = new Dictionary<string, object>();
            resourcesCollection.Add("channels", chBitmaskPermCollection);
            resourcesCollection.Add("groups", cgBitmaskPermCollection);
            resourcesCollection.Add("uuids", uuidBitmaskPermCollection);
            resourcesCollection.Add("users", userBitmaskPermCollection); 
            resourcesCollection.Add("spaces", spBitmaskPermCollection); 

            Dictionary<string, object> patternsCollection = new Dictionary<string, object>();
            patternsCollection.Add("channels", chPatternBitmaskPermCollection);
            patternsCollection.Add("groups", cgPatternBitmaskPermCollection);
            patternsCollection.Add("uuids", uuidPatternBitmaskPermCollection);
            patternsCollection.Add("users", new Dictionary<string, int>()); //Empty object for users for json structure
            patternsCollection.Add("spaces", new Dictionary<string, int>()); //Empty object for spaces for json structure

            Dictionary<string, object> optimizedMeta = new Dictionary<string, object>();
            if (this.grantMeta != null)
            {
                optimizedMeta = this.grantMeta;
            }

            Dictionary<string, object> permissionCollection = new Dictionary<string, object>();
            permissionCollection.Add("resources", resourcesCollection);
            permissionCollection.Add("patterns", patternsCollection);
            permissionCollection.Add("meta", optimizedMeta);
            if (!string.IsNullOrEmpty(this.pubnubAuthorizedUuid) && this.pubnubAuthorizedUuid.Trim().Length > 0)
            {
                permissionCollection.Add("uuid", this.pubnubAuthorizedUuid);
            }

            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            messageEnvelope.Add("ttl", this.grantTTL);
            messageEnvelope.Add("permissions", permissionCollection);

            string requestMethodName = "POST";
            string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            byte[] postData = Encoding.UTF8.GetBytes(postMessage);
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildGrantV3AccessRequest(requestMethodName, postMessage, this.queryParam);

            RequestState<PNAccessManagerTokenResult> requestState = new RequestState<PNAccessManagerTokenResult>();
            requestState.Channels = pubnubResources.Channels.Keys.ToArray();
            requestState.ChannelGroups = pubnubResources.ChannelGroups.Keys.ToArray();
            requestState.ResponseType = PNOperationType.PNAccessManagerGrantToken;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePostMethod = true;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false, postData).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                if (resultList != null && resultList.Count > 0)
                {
                    ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                    PNAccessManagerTokenResult responseResult = responseBuilder.JsonToObject<PNAccessManagerTokenResult>(resultList, true);
                    if (responseResult != null)
                    {
                        ret.Result = responseResult;
                    }
                }
            }

            return ret;
        }

        private static int CalculateGrantBitMaskValue(PNTokenAuthValues perm)
        {
            int result = 0;

            if (perm.Read)
            {
                result = (int)GrantBitFlag.READ;
            }
            if (perm.Write)
            {
                result = result + (int)GrantBitFlag.WRITE;
            }
            if (perm.Manage)
            {
                result = result + (int)GrantBitFlag.MANAGE;
            }
            if (perm.Delete)
            {
                result = result + (int)GrantBitFlag.DELETE;
            }
            if (perm.Create)
            {
                result = result + (int)GrantBitFlag.CREATE;
            }
            if (perm.Get)
            {
                result = result + (int)GrantBitFlag.GET;
            }
            if (perm.Update)
            {
                result = result + (int)GrantBitFlag.UPDATE;
            }
            if (perm.Join)
            {
                result = result + (int)GrantBitFlag.JOIN;
            }

            return result;
        }
    }
}
