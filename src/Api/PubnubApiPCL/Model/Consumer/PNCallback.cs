using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNCallback<T>
    {
        public Action<T> result = null;
        public Action<PubnubClientError> error = null;
    }
}
