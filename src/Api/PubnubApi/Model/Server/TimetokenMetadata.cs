using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    internal class TimetokenMetadata
    {
        private long t { get; set; } //timetoken;
        private string r { get; set; } //region;

        internal TimetokenMetadata() { }

        //internal TimetokenMetadata(long timetoken, string region)
        //{
        //    t = timetoken;
        //    r = region;
        //}

        public long Timetoken
        {
            get
            {
                return t;
            }
            set
            {
                t = value;
            }
        }
        public string Region
        {
            get
            {
                return r;
            }
            set
            {
                r = value;
            }
        }
    }
}
