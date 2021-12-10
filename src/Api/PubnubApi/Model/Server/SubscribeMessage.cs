using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PubnubApi
{
    internal class SubscribeMessage
    {
        /// <summary>
        /// shard
        /// </summary>
        private string a { get; set; } = "";

        /// <summary>
        /// subscriptionMatch
        /// </summary>
        private string b { get; set; } = "";

        /// <summary>
        /// channel
        /// </summary>
        private string c { get; set; } = "";

        /// <summary>
        /// payload
        /// </summary>
        private object d { get; set; } = "";

        /// <summary>
        /// message type indicator
        /// </summary>
        private int e { get; set; }

        /// <summary>
        /// flags
        /// </summary>
        private string f { get; set; } = "";

        /// <summary>
        /// issuingClientId
        /// </summary>
        private string i { get; set; } = "";

        /// <summary>
        /// subscribeKey
        /// </summary>
        private string k { get; set; } = "";

        /// <summary>
        /// sequenceNumber
        /// </summary>
        private long s { get; set; }

        /// <summary>
        /// originatingTimetoken
        /// </summary>
        private TimetokenMetadata o { get; set; }

        /// <summary>
        /// publishMetadata
        /// </summary>
        private TimetokenMetadata p { get; set; }

        /// <summary>
        /// userMetadata
        /// </summary>
        private object u { get; set; }

        public string Shard
        {
            get
            {
                return a;
            }
            internal set
            {
                a = value;
            }
        }

        public string SubscriptionMatch
        {
            get
            {
                return b;
            }
            internal set
            {
                b = value;
            }
        }

        public int MessageType
        {
            get
            {
                return e;
            }
            internal set
            {
                e = value;
            }
        }

        public string Channel
        {
            get
            {
                return c;
            }
            internal set
            {
                c = value;
            }
        }

        public object Payload
        {
            get
            {
                return d;
            }
            internal set
            {
                d = value;
            }
        }

        public string Flags
        {
            get
            {
                return f;
            }
            internal set
            {
                f = value;
            }
        }

        public string IssuingClientId
        {
            get
            {
                return i;
            }
            internal set
            {
                i = value;
            }
        }

        public string SubscribeKey
        {
            get
            {
                return k;
            }
            internal set
            {
                k = value;
            }
        }

        public long SequenceNumber
        {
            get
            {
                return s;
            }
            internal set
            {
                s = value;
            }
        }

        public TimetokenMetadata OriginatingTimetoken
        {
            get
            {
                return o;
            }
            internal set
            {
                o = value;
            }
        }

        public TimetokenMetadata PublishTimetokenMetadata
        {
            get
            {
                return p;
            }
            internal set
            {
                p = value;
            }
        }

        public object UserMetadata
        {
            get
            {
                return u;
            }
            internal set
            {
                u = value;
            }
        }

    }
}
