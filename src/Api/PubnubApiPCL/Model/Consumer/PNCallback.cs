using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNCallback<T>
    {
        public Action<T> result { get; set; }
        public Action<PubnubClientError> error { get; set; }
    }
}
