using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal class ResponseBuilder
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly IPubnubLog pubnubLog;

        public ResponseBuilder(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log)
        {
            this.config = pubnubConfig;
            this.jsonLib = jsonPluggableLibrary;
            this.pubnubLog = log;
        }

        public T JsonToObject<T>(List<object> result, bool internalObject)
        {
            T ret;

            if (!internalObject)
            {
                ret = jsonLib.DeserializeToObject<T>(result);
            }
            else
            {
                NewtonsoftJsonDotNet jsonLib = new NewtonsoftJsonDotNet(this.config, this.pubnubLog);
                ret = jsonLib.DeserializeToObject<T>(result);
            }

            return ret;
        }
    }
}
