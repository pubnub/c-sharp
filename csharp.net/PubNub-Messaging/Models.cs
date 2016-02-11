using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubNubMessaging.Core
{
    public class Models
    {
        public class Message<T>
        {
            public T Data { get; set; }
            public DateTime Time { get; set; }
            public string ChannelName { get; set; }

            public override string ToString()
            {
                return Data.ToString();
            }
        }
    }

    public class Ack
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string ChannelName { get; set; }
        public Type Type { get; set; }

        public override string ToString()
        {
            return StatusCode + " : " + StatusMessage + " : " + ChannelName;
        }
    }

    public class JoinOrLeaveAck
    {
        public string Status { get; set; }
        public DateTime HappendWhen { get; set; }
        public string Who { get; set; }
        public int CountOfWho { get; set; }
        public string ChannelName { get; set; }
        public Type Type { get; set; }

        public override string ToString()
        {
            return Who + " " + Status + " " + ChannelName + " at " + HappendWhen.ToString();
        }
    }

    public class ConnectOrDisconnectAck
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string ChannelName { get; set; }
        public Type Type { get; set; }

        public override string ToString()
        {
            return StatusCode + " : " + StatusMessage + " : " + ChannelName;
        }
    }

    public class PublishAck
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string ChannelName { get; set; }
        public Type Type { get; set; }

        public override string ToString()
        {
            return StatusCode + " : " + StatusMessage + " : " + ChannelName;
        }
    }

    public class GrantAck
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string ChannelName { get; set; }
        public Type Type { get; set; }

        public override string ToString()
        {
            return StatusCode + " : " + StatusMessage + " : " + ChannelName;
        }
    }
}
