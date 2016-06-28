
using System.IO;
using System.Net;

namespace PubnubApi
{
    public class PubnubWebResponse : PubnubWebResponseBase
    {
        public PubnubWebResponse(WebResponse response) : base(response)
        {
        }

        public PubnubWebResponse(WebResponse response, HttpStatusCode statusCode) : base(response, statusCode)
        {
        }

        public PubnubWebResponse(Stream responseStream) : base(responseStream)
        {
        }

        public PubnubWebResponse(Stream responseStream, HttpStatusCode statusCode) : base(responseStream, statusCode)
        {
        }

#if !NETFX_CORE
        public override void Close()
        {
            if (response != null)
            {
                response.Close();
            }
        }
#endif

    }
}
