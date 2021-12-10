using System;

namespace PubnubApi
{
    public class PNErrorData
    {
        public string Information { get; internal set; }
        public Exception Throwable { get; internal set; }

        public PNErrorData(string information, Exception throwable)
        {
            this.Information = information;
            this.Throwable = throwable;
        }
    }
}
