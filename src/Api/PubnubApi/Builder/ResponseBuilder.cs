using System.Collections.Generic;

namespace PubnubApi
{
    internal class ResponseBuilder
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly IPubnubLog pubnubLog;

        public ResponseBuilder(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log)
        {
            config = pubnubConfig;
            jsonLib = jsonPluggableLibrary;
            pubnubLog = log;
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
                NewtonsoftJsonDotNet jsonNewtonLib = new NewtonsoftJsonDotNet(config, pubnubLog);
                ret = jsonNewtonLib.DeserializeToObject<T>(result);
            }

            return ret;
        }
    }
}
