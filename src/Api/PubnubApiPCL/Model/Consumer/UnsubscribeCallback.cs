using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class UnsubscribeCallback
    {
        public Action<PubnubClientError> Error { get; set; }
    }
}
