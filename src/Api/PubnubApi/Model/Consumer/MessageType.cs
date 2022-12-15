using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class MessageType
    {
        private PNMessageType pnMessageType { get; set; }
        private string userMessageType { get; set; }
        public MessageType(string userMessageType)
        {
            this.userMessageType = userMessageType;
        }
        public MessageType(PNMessageType pnMessageType, string userMessageType)
        {
            this.pnMessageType = pnMessageType;
            this.userMessageType = userMessageType;
        }

        public static implicit operator string(MessageType mt)
        {
            if (mt.userMessageType != null) { return mt.userMessageType; }

            string retStr = mt.pnMessageType.ToString("g");
            string outStr = retStr.Insert(0, retStr[0].ToString().ToLowerInvariant()).Remove(1, 1); //First char to lower for camelCase
            return outStr;
        }

        public override string ToString()
        {
            if (this.userMessageType != null) { return this.userMessageType; }

            string retStr = pnMessageType.ToString("g");
            string outStr = retStr.Insert(0, retStr[0].ToString().ToLowerInvariant()).Remove(1, 1); //First char to lower for camelCase
            return outStr;
        }
    }
}
