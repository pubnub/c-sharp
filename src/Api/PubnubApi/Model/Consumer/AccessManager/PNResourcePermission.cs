using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PubnubApi
{
    public class PNResourcePermission
    {
        /// <summary>
        /// Read. Applies to Subscribe, History, Presence, Objects
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Write. Applies to Publish, Objects
        /// </summary>
        public bool Write { get; set; }

        /// <summary>
        /// Manage. Applies to Channel-Groups, Objects
        /// </summary>
        public bool Manage { get; set; }

        /// <summary>
        /// Delete. Applies to History
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Create. Applies to Objects
        /// </summary>
        public bool Create { get; set; }
    }
}
