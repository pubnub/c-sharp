using System.Collections.Generic;

namespace PubnubApi
{
    public class PNAccessManagerGrantResult
    {
        public PNAccessManagerGrantResult()
        {
            this.StatusMessage = "";
            this.Service = "";
        }

        public class Data
        {
            public Data()
            {
                this.Level = "";
            }

            public class SubkeyAccess
            {
                public bool read { get; set; }
                public bool write { get; set; }
                public bool manage { get; set; }
            }
            public string Level { get; set; }
            public string SubscribeKey { get; set; }
            public int TTL { get; set; }
            public Dictionary<string, ChannelData> channels { get; set; }
            public Dictionary<string, ChannelGroupData> channelgroups { get; set; }
            public SubkeyAccess Access { get; set; }

            public class ChannelData
            {
                public Dictionary<string, AuthData> auths { get; set; }

                public class ChannelAccess
                {
                    public bool read { get; set; }
                    public bool write { get; set; }
                    public bool manage { get; set; }
                }

                public class AuthData
                {
                    public class AuthAccess
                    {
                        public bool read { get; set; }
                        public bool write { get; set; }
                        public bool manage { get; set; }
                    }

                    public AuthAccess Access { get; set; }
                }

                public ChannelAccess Access { get; set; }
            }

            public class ChannelGroupData
            {
                public Dictionary<string, AuthData> auths { get; set; }

                public class ChannelGroupAccess
                {
                    public bool read { get; set; }
                    public bool write { get; set; }
                    public bool manage { get; set; }
                }

                public class AuthData
                {
                    public class AuthAccess
                    {
                        public bool read { get; set; }
                        public bool write { get; set; }
                        public bool manage { get; set; }
                    }
                    public AuthAccess Access { get; set; }
                }

                public ChannelGroupAccess Access { get; set; }
            }


        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public bool Warning { get; set; }
        public string Service { get; set; }
        public Data Payload { get; set; }
    }
}
