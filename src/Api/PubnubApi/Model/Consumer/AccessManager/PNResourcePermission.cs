using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNResourcePermission
    {
        public bool Read { get; set; }

        public bool Write { get; set; }

        public bool Manage { get; set; }

        public bool Delete { get; set; }

        public bool Create { get; set; }
    }
}
