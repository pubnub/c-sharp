using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PubnubApi
{
    public class PNTokenAuthValues
    {
        /// <summary>
        /// Read. Applies to Subscribe, History, Presence
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Write. Applies to Publish
        /// </summary>
        public bool Write { get; set; }

        /// <summary>
        /// Manage. Applies to Channel-Groups, Objects
        /// </summary>
        public bool Manage { get; set; }

        /// <summary>
        /// Delete. Applies to History, Objects
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Create. Applies to Objects v1
        /// </summary>
        public bool Create { get; set; }

        /// <summary>
        /// Get. Applies to Objects v2
        /// </summary>
        public bool Get { get; set; }

        /// <summary>
        /// Update. Applies to Objects v2
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Join. Applies to Objects v2
        /// </summary>
        public bool Join { get; set; }
    }
}
