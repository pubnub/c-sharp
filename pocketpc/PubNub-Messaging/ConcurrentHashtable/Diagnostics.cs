/*  
 Copyright 2008 The 'A Concurrent Hashtable' development team  
 (http://www.codeplex.com/CH/People/ProjectPeople.aspx)

 This library is licensed under the GNU Library General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.codeplex.com/CH/license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TvdP.Collections
{
#if !SILVERLIGHT
    internal static class Diagnostics
    {
        internal static TraceSwitch ConcurrentHashtableSwitch = new TraceSwitch("ConcurrentHashtable", "ConcurrentHashtable diagnostics");
        internal static Dictionary<Type, bool> TypeBadHashReportMap = new Dictionary<Type, bool>();

        static Diagnostics()
        {        
            if( ConcurrentHashtableSwitch.TraceVerbose )
                Trace.TraceInformation("ConcurrentHashtable diagnostics initialized.");
        }

        internal static void JustToWakeUp()
        { }
    }
#endif
}
