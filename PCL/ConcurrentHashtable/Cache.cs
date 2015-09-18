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
//using System.Runtime.Serialization;

namespace TvdP.Collections
{
    struct KeyStruct<TKey>
    {
        public TKey _Key;
        public uint _Hash;
    }
    class Slot<TKey>
    {
        public object _Item;
        public KeyStruct<TKey> _Key;
        public int _GC2WhenLastUsed;
    }

    sealed class Level1CacheClass<TKey> : ConcurrentWeakHashtable<Slot<TKey>, TKey>
    {
        public Level1CacheClass(IEqualityComparer<TKey> keyComparer)
        {
            _KeyComparer = keyComparer;
#if !SILVERLIGHT
            _GC2Count = GC.CollectionCount(2);
#else
            _GC2Count = (int)(System.DateTime.Now.Ticks / 100000000L); //10 second intervals
#endif
            base.Initialize();
        }

        int _GC2Count;
        IEqualityComparer<TKey> _KeyComparer;

        /// <summary>
        /// Table maintenance, removes all items marked as Garbage.
        /// </summary>
        /// <returns>
        /// A boolean value indicating if the maintenance run could be performed without delay; false if there was a lock on the SyncRoot object
        /// and the maintenance run could not be performed.
        /// </returns>
        /// <remarks>
        /// This method is called regularly in sync with the garbage collector on a high-priority thread.
        /// 
        /// This override keeps track of GC.CollectionCount(2) to assess which items have been accessed recently.
        /// </remarks>
        public override bool DoMaintenance()
        {
#if !SILVERLIGHT
            _GC2Count = GC.CollectionCount(2);
#else
            _GC2Count = (int)( System.DateTime.Now.Ticks / 100000000L ); //10 second intervals
#endif
            return base.DoMaintenance();
        }

        protected internal override bool IsGarbage(ref Slot<TKey> item)
        {
            unchecked
            {
                return item != null && item._GC2WhenLastUsed - _GC2Count < -1;
            }
        }

        protected internal override uint GetItemHashCode(ref Slot<TKey> item)
        {
            return item._Key._Hash;
        }

        protected internal override uint GetKeyHashCode(ref TKey key)
        {
            return Hasher.Rehash(_KeyComparer.GetHashCode(key));
        }

        protected internal override bool ItemEqualsKey(ref Slot<TKey> item, ref TKey key)
        {
            return _KeyComparer.Equals(item._Key._Key, key);
        }

        protected internal override bool ItemEqualsItem(ref Slot<TKey> item1, ref Slot<TKey> item2)
        {
            return _KeyComparer.Equals(item1._Key._Key, item2._Key._Key);
        }

        protected internal override bool IsEmpty(ref Slot<TKey> item)
        {
            return item == null;
        }

        protected internal override Type GetKeyType(ref Slot<TKey> item)
        {
            if (item == null)
                return null;

            object key = item._Key._Key;

            return key == null ? null : key.GetType();
        }

        public bool TryGetItem(TKey key, out object item)
        {
            Slot<TKey> slot;

            if (base.FindItem(ref key, out slot))
            {
                item = slot._Item;
                slot._GC2WhenLastUsed = _GC2Count;
                return true;
            }

            item = null;
            return false;
        }

        public bool GetOldestItem(TKey key, ref object item)
        {
            Slot<TKey> searchSlot = new Slot<TKey> { _Item = item, _GC2WhenLastUsed = _GC2Count, _Key = new KeyStruct<TKey> { _Key = key, _Hash = GetKeyHashCode(ref key) } };
            Slot<TKey> foundSlot;

            bool res = base.GetOldestItem(ref searchSlot, out foundSlot);

            foundSlot._GC2WhenLastUsed = _GC2Count;

            item = foundSlot._Item;

            return res;
        }

        public new void Clear()
        { base.Clear(); }
    }


    class ObjectComparerClass<TKey> : IEqualityComparer<object>
    {
        public IEqualityComparer<TKey> _KeyComparer;

        #region IEqualityComparer<object> Members

        public new bool Equals(object x, object y)
        { return _KeyComparer.Equals(((TKey)x), ((TKey)y)); }

        public int GetHashCode(object obj)
        { return _KeyComparer.GetHashCode(((TKey)obj)); }

        #endregion
    }

    /// <summary>
    /// Cache; Retains values longer than WeakDictionary
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TValue">Type of value</typeparam>
    /// <remarks>
    /// Use only for expensive values.
    /// </remarks>
    public sealed class Cache<TKey, TValue> 
    {   
        Level1CacheClass<TKey> _Level1Cache;

        ConcurrentWeakDictionary<object, object> _Level2Cache;

        /// <summary>
        /// Constructs a new instance of <see cref="Cache{TKey,TValue}"/> using an explicit <see cref="IEqualityComparer{TKey}"/> of TKey to comparer keys.
        /// </summary>
        /// <param name="keyComparer">The <see cref="IEqualityComparer{TKey}"/> of TKey to compare keys with.</param>
        public Cache(IEqualityComparer<TKey> keyComparer)
        {
            _Level1Cache = new Level1CacheClass<TKey>(keyComparer);
            _Level2Cache = new ConcurrentWeakDictionary<object, object>(new ObjectComparerClass<TKey> { _KeyComparer = keyComparer });
        }

        /// <summary>
        /// Constructs a new instance of <see cref="Cache{TKey,TValue}"/> with the default key comparer.
        /// </summary>
        public Cache()
            : this(EqualityComparer<TKey>.Default)
        { }

        /// <summary>
        /// Try to retrieve an item from the cache
        /// </summary>
        /// <param name="key">Key to find the item with.</param>
        /// <param name="item">Out parameter that will receive the found item.</param>
        /// <returns>A boolean value indicating if an item has been found.</returns>
        public bool TryGetItem(TKey key, out TValue item)
        {
            object storedItem;

            bool found = _Level1Cache.TryGetItem(key, out storedItem);

            if (!found && _Level2Cache.TryGetValue((object)key, out storedItem))
            {
                found = true;
                _Level1Cache.GetOldestItem(key, ref storedItem);
            }

            if (found)
            {
                item = (TValue)storedItem;
                return true;
            }

            item = default(TValue);
            return false;
        }

        /// <summary>
        /// Tries to find an item in the cache but if it can not be found a new given item will be inserted and returned.
        /// </summary>
        /// <param name="key">The key to find an existing item or insert a new item with.</param>
        /// <param name="newItem">The new item to insert if an existing item can not be found.</param>
        /// <returns>The found item or the newly inserted item.</returns>
        public TValue GetOldest(TKey key, TValue newItem)
        {
            object item = newItem;

            if (!_Level1Cache.GetOldestItem(key, ref item)) //return false.. no existing item was found
            {
                _Level2Cache.Insert((object)key, item);
                return (TValue)item;
            }

            return newItem;
        }

        /// <summary>
        /// Clears all entries from the cache.
        /// </summary>
        public void Clear()
        {
            _Level2Cache.Clear();
            _Level1Cache.Clear();
        }
			
    }
}
