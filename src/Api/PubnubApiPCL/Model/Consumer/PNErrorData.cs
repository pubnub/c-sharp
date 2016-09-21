using System;

namespace PubnubApi
{
    public class PNErrorData
    {
        public string Information;
        public Exception Throwable;

        //public PNErrorData()
        //{
        //    this.Information = "";
        //    this.Throwable = null;
        //}

        public PNErrorData(string information, Exception throwable)
        {
            this.Information = information;
            this.Throwable = throwable;
        }
    }
}
