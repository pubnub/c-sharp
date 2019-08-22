using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNUserResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }
        public string ProfileUrl { get; set; }
        public string Email { get; set; }
        public Dictionary<string, object> Custom { get; set; }
        public string Created { get; set; }
        public string Updated { get; set; }
    }
}
