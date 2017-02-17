using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PubnubApi
{
    internal class SubscribeMessage
    {
        private string a { get; set; } = "";//shard;
        private string b { get; set; } = "";//subscriptionMatch
        private string c { get; set; } = ""; //channel
        private object d { get; set; } = "";//payload
        //private bool ear { get; set;} //eat after reading
        private string f { get; set; } = "";//flags
        private string i { get; set; } = "";//issuingClientId
        private string k { get; set; } = "";//subscribeKey
        private long s { get; set; } //sequenceNumber
        private TimetokenMetadata o { get; set; } //originatingTimetoken
        private TimetokenMetadata p { get; set; } //publishMetadata
        //private string r { get; set;} //replicationMap
        private object u { get; set; } //userMetadata
                                       //private string w { get; set;} //waypointList

        public SubscribeMessage() { }

        public string Shard
        {
            get
            {
                return a;
            }
            set
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
            set
            {
                b = value;
            }
        }

        public string Channel
        {
            get
            {
                return c;
            }
            set
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
            set
            {
                d = value;
            }
        }

        /*public bool EatAfterReading{
            get{
                return ear;
            }
        }*/

        public string Flags
        {
            get
            {
                return f;
            }
            set
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
            set
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
            set
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
            set
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
            set
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
            set
            {
                p = value;
            }
        }

        /*public object ReplicationMap{
            get{
                return r;
            }
        }*/

        public object UserMetadata
        {
            get
            {
                return u;
            }
            set
            {
                u = value;
            }
        }

        /*public string WaypointList{
            get{
                return w;
            }
        }*/


    }
}
