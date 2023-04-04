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

        internal object this[string key] {
            get {
                switch (key) {
                    case "a": return a;
                    case "b": return b;
                    case "c": return c;
                    case "d": return d;
                    case "e": return e;
                    case "f": return f;
                    case "i": return i;
                    case "k": return k;
                    case "s": return s;
                    case "u": return u;
                    case "o": return o;
                    case "p": return p;
                }

                return null;
            }
            set {
                switch (key) {
                    case "a": a = value.ToString(); break;
                    case "b": b = value.ToString(); break;
                    case "c": c = value.ToString(); break;
                    case "d": d = value; break;
                    case "e": 
                        int _;
                        if (Int32.TryParse(value.ToString(), out _)) {
                            e = _;
                        }
                        break;
                    case "f": f = value.ToString(); break;
                    case "i": i = value.ToString(); break;
                    case "k": k = value.ToString(); break;
                    case "s":
                        long __;
                        if (long.TryParse(value.ToString(), out __)) {
                            s = __;
                        }
                        break;
                    case "u": u = value; break;
                    case "o": ParseTimetoken(value as Dictionary<string, object>, ___ => o = ___); break;
                    case "p": ParseTimetoken(value as Dictionary<string, object>, ___ => p = ___); break;
                }
            }
        }

        private void ParseTimetoken(Dictionary<string, object> ttMetaData, System.Action<TimetokenMetadata> assign) {
            if (ttMetaData != null && ttMetaData.Count > 0) {
                TimetokenMetadata ttMeta = new TimetokenMetadata();

                foreach (string metaKey in ttMetaData.Keys) {
                    if (metaKey.ToLowerInvariant().Equals("t", StringComparison.OrdinalIgnoreCase)) {
                        long timetoken;
                        _ = Int64.TryParse(ttMetaData[metaKey].ToString(), out timetoken);
                        ttMeta.Timetoken = timetoken;
                    } else if (metaKey.ToLowerInvariant().Equals("r", StringComparison.OrdinalIgnoreCase)) {
                        ttMeta.Region = ttMetaData[metaKey].ToString();
                    }
                }

                assign?.Invoke(ttMeta);
            }
        }

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
