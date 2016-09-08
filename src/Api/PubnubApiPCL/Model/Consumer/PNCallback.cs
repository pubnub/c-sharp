using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNCallback<T>
    {
        public Action<T> Result { get; set; }
        public Action<PubnubClientError> Error { get; set; }
    }
}
