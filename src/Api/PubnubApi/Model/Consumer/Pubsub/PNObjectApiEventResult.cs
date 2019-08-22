using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNObjectApiEventResult
    {
        public PNObjectApiEventResult()
        {
            this.Event = ""; //values = update/delete for User/Space; create/update/delete for UserAssociation/SpaceMember
            this.Type = "";  //values = user/space/membership
            this.UserId = "";
            this.SpaceId = "";
            this.Channel = "";
        }
        public string Event { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
        public PNUserResult User { get; set; } //Populate when Type = user
        public string SpaceId { get; set; }
        public PNSpaceResult Space { get; set; } //Populate when Type = space
        public long Timestamp { get; set; }
        public string Channel { get; set; }
    }
}
