﻿using System.Collections.Generic;

namespace PubnubApi
{
    internal class ResponseBuilder
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly IPubnubLog pubnubLog;
        private readonly NewtonsoftJsonDotNet newtonsoftJsonDotNet;
        private readonly EventDeserializer eventDeserializer;

        public ResponseBuilder(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log)
        {
            config = pubnubConfig;
            jsonLib = jsonPluggableLibrary;
            pubnubLog = log;
            newtonsoftJsonDotNet = new NewtonsoftJsonDotNet(config, pubnubLog);
            eventDeserializer = new EventDeserializer(jsonLib, newtonsoftJsonDotNet);
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
                ret = newtonsoftJsonDotNet.DeserializeToObject<T>(result);
            }

            return ret;
        }

        public T GetEventResultObject<T>(IDictionary<string, object> jsonFields)
        {
            return eventDeserializer.Deserialize<T>(jsonFields);
        }
    }
}
