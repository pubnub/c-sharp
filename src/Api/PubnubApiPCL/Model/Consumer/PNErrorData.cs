using System;

namespace PubnubApi
{
    public class PNErrorData
    {
        public string Information { get; set; }
        public Exception Throwable { get; set; }

        public PNErrorData(string information, Exception throwable)
        {
            this.Information = information;
            this.Throwable = throwable;
        }
    }
}
