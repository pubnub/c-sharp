using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal class ResponseBuilder
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLib = null;

        public ResponseBuilder(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            this.config = pubnubConfig;
            this.jsonLib = jsonPluggableLibrary;
        }

        public T JsonToObject<T>(List<object> result, bool internalObject)
        {
            T ret = default(T);

            if (!internalObject)
            {
                ret = jsonLib.DeserializeToObject<T>(result);
            }
            else
            {
                NewtonsoftJsonDotNet jsonLib = new NewtonsoftJsonDotNet();
                ret = jsonLib.DeserializeToObject<T>(result);
            }

            return ret;
        }
    }
}
