using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
// using PeterO.Cbor;
using System.Globalization;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class TokenManager : IDisposable
    {
        private readonly PNConfiguration pubnubConfig;
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly string pubnubInstanceId;
        private readonly PubnubLogModule logger;
        private static ConcurrentDictionary<string, string> dToken
        {
            get;
            set;
        } = new ConcurrentDictionary<string, string>();

        public TokenManager(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, string instanceId)
        {
            pubnubConfig = config;
            jsonLib = jsonPluggableLibrary;
            pubnubInstanceId = instanceId;
            logger = config.Logger;
            if (!dToken.ContainsKey(instanceId))
            {
                dToken.TryAdd(instanceId, null);
            }
            
        }

        public string AuthToken 
        {
            get 
            { 
                string tkn;
                dToken.TryGetValue(pubnubInstanceId, out tkn); 
                return tkn; 
            } 
        }
        
        private static string GetDisplayableBytes(byte[] currentBytes)
        {
            StringBuilder outBuilder = new StringBuilder("{ ");
            for (int di = 0; di < currentBytes.Length; di++)
            {
                outBuilder.Append(currentBytes[di]);
                if (di < currentBytes.Length - 1)
                {
                    outBuilder.Append(", ");
                }
            }
            outBuilder.Append(" }");
            return outBuilder.ToString();
        }

        public PNTokenContent ParseToken(string token)
        {
            PNTokenContent result = null;
            return result;
        }

        private string ReplaceBoundaryQuotes(string key)
        {
            if (key != null && !string.IsNullOrEmpty(key) && key.Length >= 2 
                && string.Equals(key.Substring(0, 1), "\"", StringComparison.OrdinalIgnoreCase)
                && string.Equals(key.Substring(key.Length - 1, 1), "\"", StringComparison.OrdinalIgnoreCase))
            {
                key = key.Remove(key.Length - 1, 1).Remove(0, 1);
                key = Regex.Unescape(key);
            }
            return key;
        }
        public void SetAuthToken(string token)
        {
            dToken.AddOrUpdate(pubnubInstanceId, token, (k, o) => token);
        }

        private static PNTokenAuthValues GetResourcePermission(int combinedVal)
        {
            PNTokenAuthValues rp = new PNTokenAuthValues();
            if (combinedVal > 0)
            {
                rp.Read = (combinedVal & (int)GrantBitFlag.READ) != 0;
                rp.Write = (combinedVal & (int)GrantBitFlag.WRITE) != 0;
                rp.Manage = (combinedVal & (int)GrantBitFlag.MANAGE) != 0;
                rp.Create = (combinedVal & (int)GrantBitFlag.CREATE) != 0;
                rp.Delete = (combinedVal & (int)GrantBitFlag.DELETE) != 0;
                rp.Get = (combinedVal & (int)GrantBitFlag.GET) != 0;
                rp.Update = (combinedVal & (int)GrantBitFlag.UPDATE) != 0;
                rp.Join = (combinedVal & (int)GrantBitFlag.JOIN) != 0;
            }
            return rp;
        }

        internal void Destroy()
        {
            if (!string.IsNullOrEmpty(pubnubInstanceId) && dToken != null && dToken.ContainsKey(pubnubInstanceId))
            {
                dToken[pubnubInstanceId] = null;
            }
            
        }

#region IDisposable Support
        private bool disposedValue;

        protected virtual void DisposeInternal(bool disposing)
        {
            if (!disposedValue)
            {
                if (!string.IsNullOrEmpty(pubnubInstanceId) && dToken != null && dToken.ContainsKey(pubnubInstanceId))
                {
                    dToken[pubnubInstanceId] = null;
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            DisposeInternal(true);
        }
#endregion

    }
}
