using System.Collections.Generic;

namespace PubnubApi
{
    public class HereNowAck
    {
        public HereNowAck()
        {
            this.StatusMessage = "";
            this.Service = "";
            this.ChannelName = "";
            //this.UUID = new string[0];
        }

        public class Data
        {
            public Data()
            {

            }
            public class ChannelData
            {
                public class UuidData
                {
                    public string uuid { get; set; }
                    public Dictionary<string, object> state { get; set; }
                }

                public int occupancy { get; set; }
                public UuidData[] uuids { get; set; }
            }
            public Dictionary<string, ChannelData> channels;
            public int total_channels { get; set; }
            public int total_occupancy { get; set; }
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Data Payload { get; set; }
        public string ChannelName { get; set; }

    }
}
