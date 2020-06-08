using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNPageObject
    {
        public PNPageObject()
        {
            Next = "";
            Prev = "";
        }

        public string Next { get; set; }
        public string Prev { get; set; }
    }
}
