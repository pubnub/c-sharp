using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.Interface
{
    public interface IUrlRequestBuilder
    {
        Uri BuildTimeRequest();
    }
}
