using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    // TODO: deprecate old way of specifying retry configuration.
    public enum PNReconnectionPolicy
    {
        NONE,
        LINEAR,
        EXPONENTIAL
    }
}
