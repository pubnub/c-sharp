﻿using System.Collections.Generic;

namespace PubnubApi
{
    internal class ResponseBuilder
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly EventDeserializer eventDeserializer;

        public ResponseBuilder(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLib = jsonPluggableLibrary;
            eventDeserializer = new EventDeserializer(jsonLib);
        }

        public T JsonToObject<T>(List<object> result, bool internalObject)
        {
            var ret = jsonLib.DeserializeToObject<T>(result);
            return ret;
        }

        public T GetEventResultObject<T>(IDictionary<string, object> jsonFields)
        {
            return eventDeserializer.Deserialize<T>(jsonFields);
        }
    }
}