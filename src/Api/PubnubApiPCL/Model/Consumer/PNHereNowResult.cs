using System.Collections.Generic;

namespace PubnubApi
{
    public class PNHereNowResult
    {
        public PNHereNowResult()
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
                    public string Uuid { get; set; }
                    public Dictionary<string, object> State { get; set; }
                }

                public int Occupancy { get; set; }
                public UuidData[] Uuids { get; set; }
            }
            public Dictionary<string, ChannelData> Channels { get; set; }
            public int Total_channels { get; set; }
            public int Total_occupancy { get; set; }
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Service { get; set; }
        public Data Payload { get; set; }
        public string ChannelName { get; set; }

    }
}
