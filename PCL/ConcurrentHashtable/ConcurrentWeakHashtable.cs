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
using System.Threading;

namespace TvdP.Collections
{
    /// <summary>
    /// Base class for weak concurrent hashtable implementations. 
    /// </summary>
    /// <typeparam name="TStored">Type of the items stored in the hashtable.</typeparam>
    /// <typeparam name="TSearch">Type of the key to search with.</typeparam>
    /// <remarks>
    /// This class is an extention of <see cref="ConcurrentHashtable{TStored,TSearch}"/>. It will detect
    /// a run of the garbage collector and subsequently check all items in the table if they are marked as garbage.
    /// Each item that is marked as garbage will be removed from the table. 
    /// </remarks>
    public abstract class ConcurrentWeakHashtable<TStored, TSearch> : ConcurrentHashtable<TStored, TSearch>, IMaintainable
    {
        /// <summary>
        /// Table maintenance, removes all items marked as Garbage.
        /// </summary>
        /// <returns>
        /// A boolean value indicating if the maintenance run could be performed without delay; false if there was a lock on the SyncRoot object
        /// and the maintenance run could not be performed.
        /// </returns>
        /// <remarks>
        /// This method is called regularly in sync with the garbage collector on a high-priority thread.
        /// </remarks>
        public virtual bool DoMaintenance()
        {
            var syncRoot = SyncRoot;

            if (!Monitor.TryEnter(syncRoot))
                return false;

            try
            { SweepGarbage(); }
            finally
            { Monitor.Exit(syncRoot); }

            return true;
        }

        private void SweepGarbage()
        {
            foreach (var segment in EnumerateAmorphLockedSegments(false))
                ((WeakSegment<TStored, TSearch>)segment).DisposeGarbage(this);
        }

        /// <summary>
        /// This method is called when a re-segmentation is expected to be needed. It checks if it actually is needed and, if so, performs the re-segementation.
        /// </summary>
        /// <remarks>
        /// This override first sweeps the table for garbage in an attempt to prevent re-segmentation.
        /// </remarks>
        protected override void AssessSegmentation()
        {
            lock (SyncRoot)
                SweepGarbage();

            base.AssessSegmentation();
        }

        /// <summary>
        /// Initialize the newly created ConcurrentHashtable. Invoke in final (sealed) constructor
        /// or Create method.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            //registers this table with the ConcurrentWeakHashtableHelper.
            //that class will invoke our DoMaintenance() method when a GC is detected.
            ConcurrentWeakHashtableHelper.Register(this);
        }

        /// <summary>
        /// Indicates if a specific content item should be treated as garbage and removed.
        /// </summary>
        /// <param name="item">The item to judge.</param>
        /// <returns>A boolean value that is true if the item is not empty and should be treated as garbage; false otherwise.</returns>
        internal protected abstract bool IsGarbage(ref TStored item);

        /// <summary>
        /// CreateSegmentRange override that returns a WeakSegmentrange
        /// </summary>
        /// <param name="segmentCount"></param>
        /// <param name="initialSegmentSize"></param>
        /// <returns></returns>
        internal override Segmentrange<TStored, TSearch> CreateSegmentRange(int segmentCount, int initialSegmentSize)
        { return WeakSegmentrange<TStored, TSearch>.Create(segmentCount, initialSegmentSize); }

        /// <summary>
        /// Looks for an existing item in the table contents using an alternative copy. If it can be found it will be returned. 
        /// If not then the alternative copy will be added to the table contents and the alternative copy will be returned.
        /// </summary>
        /// <param name="searchKey">A copy to search an already existing instance with</param>
        /// <param name="item">Out reference to receive the found item or the alternative copy</param>
        /// <returns>A boolean that will be true if an existing copy was found and false otherwise.</returns>
        /// <remarks>This override does not return garbage items.</remarks>
        protected sealed override bool GetOldestItem(ref TStored searchKey, out TStored item)
        {
            var segment = GetSegmentLockedForWriting(this.GetItemHashCode(ref searchKey));

            try
            {
                var res = segment.GetOldestItem(ref searchKey, out item, this);

                if (res && IsGarbage(ref item))
                {
                    //found old item that turned out to be garbage.. replace it.
                    TStored garbage;
                    segment.InsertItem(ref searchKey, out garbage, this);
                    item = searchKey;
                    res = false;
                }

                return res;            
            }
            finally
            { segment.ReleaseForWriting(); }
        }
    }
}
