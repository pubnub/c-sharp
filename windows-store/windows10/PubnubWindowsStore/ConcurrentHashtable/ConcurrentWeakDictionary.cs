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
//using System.Security.Permissions;

namespace TvdP.Collections
{
    /// <summary>
    /// Entry item for ConcurrentWeakDictionary
    /// </summary>
    public struct ConcurrentWeakDictionaryItem
    {
        internal UInt32 _Hash;
        internal WeakReference _Key;
        internal WeakReference _Value;

        internal ConcurrentWeakDictionaryItem(UInt32 hash, WeakReference key, WeakReference value)
        {
            _Hash = hash;
            _Key = key;
            _Value = value;
        }
    }

    /// <summary>
    /// Search key for ConcurrentWeakDictionary
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    public struct ConcurrentWeakDictionaryKey<TKey>
    {
        internal UInt32 _Hash;
        internal TKey _Key;

        internal ConcurrentWeakDictionaryKey(UInt32 hash, TKey key)
        {
            _Hash = hash;
            _Key = key;
        }
    }

    /// <summary>
    /// A dictionary that has weakreferences to it's keys and values. If either a key or its associated value gets garbage collected
    /// then the entry will be removed from the dictionary. 
    /// </summary>
    /// <typeparam name="TKey">Type of the keys. This must be a reference type.</typeparam>
    /// <typeparam name="TValue">Type of the values. This must be a reference type.</typeparam>
    public sealed class ConcurrentWeakDictionary<TKey, TValue> 
        : ConcurrentWeakHashtable<ConcurrentWeakDictionaryItem
            , ConcurrentWeakDictionaryKey<TKey>>

        where TKey : class
        where TValue : class
    {
        #region Constructors

        /// <summary>
        /// Instantiates a ConcurrentWeakDictionary with the default comparer for <typeparamref name="TKey"/>.
        /// </summary>
        public ConcurrentWeakDictionary()
            : this(EqualityComparer<TKey>.Default)
        { }

        /// <summary>
        /// Instatiates a ConcurrentWeakDictionary with an explicit comparer for <typeparamref name="TKey"/>.
        /// </summary>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to comparer keys.</param>
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
        public ConcurrentWeakDictionary(IEqualityComparer<TKey> comparer)
            : base()
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            _Comparer = comparer;

            Initialize();
        }


        #endregion

        #region Traits

        /// <summary>
        /// Get a hashcode for given storeable item.
        /// </summary>
        /// <param name="item">Reference to the item to get a hash value for.</param>
        /// <returns>The hash value as an <see cref="UInt32"/>.</returns>
        /// <remarks>
        /// The hash returned should be properly randomized hash. The standard GetItemHashCode methods are usually not good enough.
        /// A storeable item and a matching search key should return the same hash code.
        /// So the statement <code>ItemEqualsItem(storeableItem, searchKey) ? GetItemHashCode(storeableItem) == GetItemHashCode(searchKey) : true </code> should always be true;
        /// </remarks>
        internal protected override UInt32 GetItemHashCode(ref ConcurrentWeakDictionaryItem item)
        { return item._Hash; }

        /// <summary>
        /// Get a hashcode for given search key.
        /// </summary>
        /// <param name="key">Reference to the key to get a hash value for.</param>
        /// <returns>The hash value as an <see cref="UInt32"/>.</returns>
        /// <remarks>
        /// The hash returned should be properly randomized hash. The standard GetItemHashCode methods are usually not good enough.
        /// A storeable item and a matching search key should return the same hash code.
        /// So the statement <code>ItemEqualsItem(storeableItem, searchKey) ? GetItemHashCode(storeableItem) == GetItemHashCode(searchKey) : true </code> should always be true;
        /// </remarks>
        internal protected override UInt32 GetKeyHashCode(ref ConcurrentWeakDictionaryKey<TKey> key)
        { return key._Hash; }

        /// <summary>
        /// Compares a storeable item to a search key. Should return true if they match.
        /// </summary>
        /// <param name="item">Reference to the storeable item to compare.</param>
        /// <param name="key">Reference to the search key to compare.</param>
        /// <returns>True if the storeable item and search key match; false otherwise.</returns>
        internal protected override bool ItemEqualsKey(ref ConcurrentWeakDictionaryItem item, ref ConcurrentWeakDictionaryKey<TKey> key)
        {
            var key1 = (TKey)item._Key.Target;
            return _Comparer.Equals(key1, key._Key);
        }

        /// <summary>
        /// Compares two storeable items for equality.
        /// </summary>
        /// <param name="item1">Reference to the first storeable item to compare.</param>
        /// <param name="item2">Reference to the second storeable item to compare.</param>
        /// <returns>True if the two soreable items should be regarded as equal.</returns>
        internal protected override bool ItemEqualsItem(ref ConcurrentWeakDictionaryItem item1, ref ConcurrentWeakDictionaryItem item2)
        {
            var key1 = (TKey)item1._Key.Target;
            var key2 = (TKey)item2._Key.Target;

            return key1 == null && key2 == null ? item1._Key == item2._Key : _Comparer.Equals(key1, key2);
        }

        /// <summary>
        /// Indicates if a specific item reference contains a valid item.
        /// </summary>
        /// <param name="item">The storeable item reference to check.</param>
        /// <returns>True if the reference doesn't refer to a valid item; false otherwise.</returns>
        /// <remarks>The statement <code>IsEmpty(default(TStoredI))</code> should always be true.</remarks>
        internal protected override bool IsEmpty(ref ConcurrentWeakDictionaryItem item)
        { return item._Key == null; }

        /// <summary>
        /// Indicates if a specific content item should be treated as garbage and removed.
        /// </summary>
        /// <param name="item">The item to judge.</param>
        /// <returns>A boolean value that is true if the item is not empty and should be treated as garbage; false otherwise.</returns>
        internal protected override bool IsGarbage(ref ConcurrentWeakDictionaryItem item)
        { return item._Key != null && ( item._Key.Target == null || (item._Value != null && item._Value.Target == null) ); }

        protected internal override Type GetKeyType(ref ConcurrentWeakDictionaryItem item)
        {
            if (item._Key == null)
                return null;

            object key = item._Key.Target;

            return key == null ? null : key.GetType();
        }

        #endregion

        IEqualityComparer<TKey> _Comparer;

        /// <summary>
        /// The <see cref="IEqualityComparer{TKey}"/> of TKey used to compare keys for equality.
        /// </summary>
        public IEqualityComparer<TKey> Comparer { get { return _Comparer; } }

        UInt32 GetHashCode(TKey key)
        { return Hasher.Rehash(_Comparer.GetHashCode(key)); }

        #region Public accessors

        /// <summary>
        /// Inserts a key value association into the dictionary.
        /// </summary>
        /// <param name="key">The key to identify the value with.</param>
        /// <param name="value">The value to associate the key with.</param>
        /// <exception cref="ArgumentNullException">Gets raised when the <paramref name="key"/> parameter is null.</exception>
        public void Insert(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var item = new ConcurrentWeakDictionaryItem(GetHashCode(key), new WeakReference(key), value == null ? null : new WeakReference(value));
            ConcurrentWeakDictionaryItem oldItem;
            base.InsertItem(ref item, out oldItem);
        }

        /// <summary>
        /// Retrieves an existing value associated with the specified key or, if it can't be found, associates a specified value with the key and returns that later value.
        /// </summary>
        /// <param name="key">The key to find an existing value with or, if it can't be found, insert a new value with.</param>
        /// <param name="newValue">The new value to insert if an existing associated value can not be found in the dictionary.</param>
        /// <returns>The existing value if it can be found; otherwise the newly inserted value.</returns>
        /// <exception cref="ArgumentNullException">Gets raised when the <paramref name="key"/> parameter is null.</exception>
        public TValue GetOldest(TKey key, TValue newValue)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var item = new ConcurrentWeakDictionaryItem(GetHashCode(key), new WeakReference(key), newValue == null ? null : new WeakReference(newValue));
            ConcurrentWeakDictionaryItem oldItem;
            TValue res;

            do
            {
                if (!base.GetOldestItem(ref item, out oldItem))
                    return newValue;

                if (oldItem._Value == null)
                    return null;

                res = (TValue)oldItem._Value.Target;
            }
            while (res == null);

            return res;
        }

        /// <summary>
        /// Remove any association in the dictionary with the specified key.
        /// </summary>
        /// <param name="key">The key of the association to remove.</param>
        /// <exception cref="ArgumentNullException">Gets raised when the <paramref name="key"/> parameter is null.</exception>
        public void Remove(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var item = new ConcurrentWeakDictionaryKey<TKey>(GetHashCode(key), key);
            ConcurrentWeakDictionaryItem oldItem;

            base.RemoveItem(ref item, out oldItem);
        }

        /// <summary>
        /// Try to find an association with the specified key and return the value.
        /// </summary>
        /// <param name="key">The key to find the association with.</param>
        /// <param name="value">An out reference to assign the value of the found association to. If no association can be found the reference will be set to default(<typeparamref name="TValue"/>).</param>
        /// <returns>True if an association can be found; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Gets raised when the <paramref name="key"/> parameter is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var item = new ConcurrentWeakDictionaryKey<TKey>(GetHashCode(key), key);
            ConcurrentWeakDictionaryItem oldItem;

            if (base.FindItem(ref item, out oldItem))
            {
                if (oldItem._Value == null)
                {
                    value = null;
                    return true;
                }
                else
                {
                    value = (TValue)oldItem._Value.Target;
                    return value != null;
                }
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Try to find an association with the specified key, return it and remove the association from the dictionary.
        /// </summary>
        /// <param name="key">The key to find the association with.</param>
        /// <param name="value">An out reference to assign the value of the found association to. If no association can be found the reference will be set to default(<typeparamref name="TValue"/>).</param>
        /// <returns>True if an association can be found; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Gets raised when the <paramref name="key"/> parameter is null.</exception>
        public bool TryPopValue(TKey key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var item = new ConcurrentWeakDictionaryKey<TKey>(GetHashCode(key), key);
            ConcurrentWeakDictionaryItem oldItem;

            if (base.RemoveItem(ref item, out oldItem))
            {
                if (oldItem._Value == null)
                {
                    value = null;
                    return true;
                }
                else
                {
                    value = (TValue)oldItem._Value.Target;
                    return value != null;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Gets or sets a value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to find the association with or to associate the given value with.</param>
        /// <returns>The value found or default(<typeparamref name="TValue"/>) when getting or the specified value when setting.</returns>
        /// <exception cref="ArgumentNullException">Gets raised when the <paramref name="key"/> parameter is null.</exception>
        public TValue this[TKey key]
        {
            get
            {
                TValue res;
                TryGetValue(key, out res);
                return res;
            }
            set
            { Insert(key, value); }
        }

        /// <summary>
        /// Gives a snapshot of the current value collection.
        /// </summary>
        /// <returns>An array containing the current values.</returns>
        /// <remarks>It is explicitly not guaranteed that any value contained in the returned array is still present
        /// in the ConcurrentWeakDictionaryStrongValues even at the moment this array is returned.</remarks>
        public TValue[] GetCurrentValues()
        {
            lock (SyncRoot)
                return
                    Items
                        .Select(item => item._Value != null ? new KeyValuePair<bool, TValue>(true, (TValue)item._Value.Target) : new KeyValuePair<bool, TValue>(false, null))
                        .Where(kvp => !kvp.Key || kvp.Value != null)
                        .Select(kvp => kvp.Value)
                        .ToArray();
        }

        /// <summary>
        /// Gives a snapshot of the current key collection.
        /// </summary>
        /// <returns>An array containing the current keys.</returns>
        /// <remarks>It is explicitly not guaranteed that any key contained in the returned array is still present
        /// in the ConcurrentWeakDictionaryStrongValues even at the moment this array is returned.</remarks>
        public TKey[] GetCurrentKeys()
        {
            var comparer = _Comparer;
            lock (SyncRoot)
                return
                    Items
                        .Select(item => (TKey)item._Key.Target)
                        .Where(key => !comparer.Equals(key, null))
                        .ToArray();
        }

        /// <summary>
        /// Remove all associations from the dictionary.
        /// </summary>
        /// <remarks>
        /// When multiple threads have simultaneous access to the dictionary it is not guaranteed
        /// that the dictionary will actually be empty when this method returns.</remarks>
        public new void Clear()
        { base.Clear(); }

        #endregion

    }
}
