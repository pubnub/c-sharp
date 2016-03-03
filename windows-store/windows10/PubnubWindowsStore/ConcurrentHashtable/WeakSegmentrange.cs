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

namespace TvdP.Collections
{
    internal class WeakSegmentrange<TStored, TSearch> : Segmentrange<TStored, TSearch> 
    {
        protected WeakSegmentrange()
        {}

        public new static Segmentrange<TStored, TSearch> Create(int segmentCount, int initialSegmentSize)
        {
            var instance = new WeakSegmentrange<TStored, TSearch>();
            instance.Initialize(segmentCount, initialSegmentSize);
            return instance;
        }

        protected override Segment<TStored, TSearch> CreateSegment(int initialSegmentSize)
        { return WeakSegment<TStored, TSearch>.Create(initialSegmentSize); }
    }
}
