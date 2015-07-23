// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.Text;

namespace NUnitLite
{
    public class Env
    {
        // Define NewLine to be used for this system
        // NOTE: Since this is done at compile time for .NET CF,
        // these binaries are not yet currently portable.
#if PocketPC || WindowsCE || NETCF
        public static readonly string NewLine = "\r\n";
#else
        public static readonly string NewLine = Environment.NewLine;
#endif
    }
}
