using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    internal class TimetokenMetadata
    {
        /// <summary>
        /// timetoken
        /// </summary>
        private long t { get; set; }

        /// <summary>
        /// region
        /// </summary>
        private string r { get; set; }

        internal TimetokenMetadata() { }

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
