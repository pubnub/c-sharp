using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNManageMembersItemResult
    {
        public string UserId { get; set; }
        public Dictionary<string, object> Custom { get; set; }
        public PNUserResult User { get; set; }
        public string Created { get; set; }
        public string Updated { get; set; }
    }
}
