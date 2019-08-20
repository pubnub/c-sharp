using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNPage
    {
        public PNPage()
        {
            Next = "";
            Prev = "";
        }

        public string Next { get; set; }
        public string Prev { get; set; }
    }
}
