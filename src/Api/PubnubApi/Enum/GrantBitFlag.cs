using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public enum GrantBitFlag
    {
        READ = 1,
        WRITE = 2,
        MANAGE = 4,
        DELETE = 8,
        CREATE = 16
    }
}
