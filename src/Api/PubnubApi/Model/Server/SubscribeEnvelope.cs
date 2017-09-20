using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PubnubApi
{
    internal class SubscribeEnvelope
    {
        /// <summary>
        /// messages
        /// </summary>
        private List<SubscribeMessage> m { get; set; }

        /// <summary>
        /// subscribeMetadata
        /// </summary>
        private TimetokenMetadata t { get; set; }

        public List<SubscribeMessage> Messages
        {
            get
            {
                return m;
            }
            set
            {
                m = value;
            }
        }

        public TimetokenMetadata TimetokenMeta
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
    }
}
