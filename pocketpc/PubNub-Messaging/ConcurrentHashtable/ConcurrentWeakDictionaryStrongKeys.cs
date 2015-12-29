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
using System.Security.Permissions;

namespace TvdP.Collections
{
    /// <summary>
    /// Entry item type for <see cref="ConcurrentWeakDictionaryStrongKeys{TKey,TValue}"/>. 
    /// </summary>
    /// <typeparam name="TKey">Type of keys of the <see cref="ConcurrentWeakDictionaryStrongKeys{TKey,TValue}"/>.</typeparam>
    public struct ConcurrentWeakDictionaryStrongKeysItem<TKey>
    {
        internal UInt32 _Hash;
        internal TKey _Key;
        internal WeakReference _Value;

        internal ConcurrentWeakDictionaryStrongKeysItem(UInt32 hash, TKey key, WeakReference value)
        {
            _Hash = hash;
            _Key = key;
            _Value = value;
        }
    }

    /// <summary>
    /// Search key for <see cref="ConcurrentWeakDictionaryStrongKeys{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of keys of the <see cref="ConcurrentWeakDictionaryStrongKeys{TKey,TValue}"/>.</typeparam>
    public struct ConcurrentWeakDictionaryStrongKeysKey<TKey>
    {
        internal UInt32 _Hash;
        internal TKey _Key;

        internal ConcurrentWeakDictionaryStrongKeysKey(UInt32 hash, TKey key)
        {
            _Hash = hash;
            _Key = key;
        }
    }


    /// <summary>
    /// A dictionary that has weakreferences to it's values. If a value gets garbage collected
    /// then the entry will be removed from the dictionary. 
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values. This must be a reference type.</typeparam>
#if !SILVERLIGHT
    [Serializable]
#endif
    public sealed class ConcurrentWeakDictionaryStrongKeys<TKey, TValue> 
        : ConcurrentWeakHashtable<ConcurrentWeakDictionaryStrongKeysItem<TKey>
            , ConcurrentWeakDictionaryStrongKeysKey<TKey>>
#if !SILVERLIGHT
            , ISerializable
#endif
        where TValue : class
    {
        #region Constructors

        /// <summary>
        /// Contructs a <see cref="ConcurrentWeakDictionaryStrongKeys{TKey,TValue}"/> instance, using the default <see cref="IEqualityComparer{TKey}"/> to compare keys.
        /// </summary>
        public ConcurrentWeakDictionaryStrongKeys()
            : this(EqualityComparer<TKey>.Default)
        { }

        /// <summary>
        /// Contructs a <see cref="ConcurrentWeakDictionaryStrongKeys{TKey,TValue}"/> instance, using a specified <see cref="IEqualityComparer{TKey}"/> to compare keys.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
        public ConcurrentWeakDictionaryStrongKeys(IEqualityComparer<TKey> comparer)
            : base()
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            _Comparer = comparer;

            Initialize();
        }

#if !SILVERLIGHT
        ConcurrentWeakDictionaryStrongKeys(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            _Comparer = (IEqualityComparer<TKey>)serializationInfo.GetValue("Comparer", typeof(IEqualityComparer<TKey>));
            var items = (List<KeyValuePair<TKey, TValue>>)serializationInfo.GetValue("Items", typeof(List<KeyValuePair<TKey, TValue>>));

            if (_Comparer == null || items == null)
                throw new SerializationException();

            Initialize();

            foreach (var kvp in items)
                Insert(kvp.Key, kvp.Value);
        }
#endif

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
        internal protected override UInt32 GetItemHashCode(ref ConcurrentWeakDictionaryStrongKeysItem<TKey> item)
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
        internal protected override UInt32 GetKeyHashCode(ref ConcurrentWeakDictionaryStrongKeysKey<TKey> key)
        { return key._Hash; }

        /// <summary>
        /// Compares a storeable item to a search key. Should return true if they match.
        /// </summary>
        /// <param name="item">Reference to the storeable item to compare.</param>
        /// <param name="key">Reference to the search key to compare.</param>
        /// <returns>True if the storeable item and search key match; false otherwise.</returns>
        internal protected override bool ItemEqualsKey(ref ConcurrentWeakDictionaryStrongKeysItem<TKey> item, ref ConcurrentWeakDictionaryStrongKeysKey<TKey> key)
        { return _Comparer.Equals(item._Key, key._Key); }

        /// <summary>
        /// Compares two storeable items for equality.
        /// </summary>
        /// <param name="item1">Reference to the first storeable item to compare.</param>
        /// <param name="item2">Reference to the second storeable item to compare.</param>
        /// <returns>True if the two soreable items should be regarded as equal.</returns>
        internal protected override bool ItemEqualsItem(ref ConcurrentWeakDictionaryStrongKeysItem<TKey> item1, ref ConcurrentWeakDictionaryStrongKeysItem<TKey> item2)
        { return _Comparer.Equals(item1._Key, item2._Key); }

        /// <summary>
        /// Indicates if a specific item reference contains a valid item.
        /// </summary>
        /// <param name="item">The storeable item reference to check.</param>
        /// <returns>True if the reference doesn't refer to a valid item; false otherwise.</returns>
        /// <remarks>The statement <code>IsEmpty(default(TStoredI))</code> should always be true.</remarks>
        internal protected override bool IsEmpty(ref ConcurrentWeakDictionaryStrongKeysItem<TKey> item)
        { return item._Value == null; }

        /// <summary>
        /// Indicates if a specific content item should be treated as garbage and removed.
        /// </summary>
        /// <param name="item">The item to judge.</param>
        /// <returns>A boolean value that is true if the item is not empty and should be treated as garbage; false otherwise.</returns>
        internal protected override bool IsGarbage(ref ConcurrentWeakDictionaryStrongKeysItem<TKey> item)
        { return item._Value != null && item._Value.Target == null; }

        protected internal override Type GetKeyType(ref ConcurrentWeakDictionaryStrongKeysItem<TKey> item)
        { return item._Key == null ? null : item._Key.GetType(); }

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
        /// <exception cref="ArgumentNullException">Gets raised when the <paramref name="value"/> parameter is null.</exception>
        public void Insert(TKey key, TValue value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var item = new ConcurrentWeakDictionaryStrongKeysItem<TKey>(GetHashCode(key), key, new WeakReference(value));
            ConcurrentWeakDictionaryStrongKeysItem<TKey> oldItem;
            base.InsertItem(ref item, out oldItem);
        }

        /// <summary>
        /// Retrieves an existing value associated with the specified key or, if it can't be found, associates a specified value with the key and returns that later value.
        /// </summary>
        /// <param name="key">The key to find an existing value with or, if it can't be found, insert a new value with.</param>
        /// <param name="newValue">The new value to insert if an existing associated value can not be found in the dictionary.</param>
        /// <returns>The existing value if it can be found; otherwise the newly inserted value.</returns>
        /// <exception cref="ArgumentNullException">Gets raised when the <paramref name="newValue"/> parameter is null.</exception>
        public TValue GetOldest(TKey key, TValue newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException("newValue");

            var item = new ConcurrentWeakDictionaryStrongKeysItem<TKey>(GetHashCode(key), key, new WeakReference(newValue));
            ConcurrentWeakDictionaryStrongKeysItem<TKey> oldItem;
            TValue res;

            do
            {
                if (!base.GetOldestItem(ref item, out oldItem))
                    return newValue;

                res = (TValue)oldItem._Value.Target;
            }
            while (res == null);

            return res;
        }

        /// <summary>
        /// Remove any association in the dictionary with the specified key.
        /// </summary>
        /// <param name="key">The key of the association to remove.</param>
        public void Remove(TKey key)
        {
            var item = new ConcurrentWeakDictionaryStrongKeysKey<TKey>(GetHashCode(key), key);
            ConcurrentWeakDictionaryStrongKeysItem<TKey> oldItem;

            base.RemoveItem(ref item, out oldItem);
        }

        /// <summary>
        /// Try to find an association with the specified key and return the value.
        /// </summary>
        /// <param name="key">The key to find the association with.</param>
        /// <param name="value">An out reference to assign the value of the found association to. If no association can be found the reference will be set to default(<typeparamref name="TValue"/>).</param>
        /// <returns>True if an association can be found; otherwise false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var item = new ConcurrentWeakDictionaryStrongKeysKey<TKey>(GetHashCode(key), key);
            ConcurrentWeakDictionaryStrongKeysItem<TKey> oldItem;

            if (base.FindItem(ref item, out oldItem))
            {
                value = (TValue)oldItem._Value.Target;
                return value != null;
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
        public bool TryPopValue(TKey key, out TValue value)
        {
            var item = new ConcurrentWeakDictionaryStrongKeysKey<TKey>(GetHashCode(key), key);
            ConcurrentWeakDictionaryStrongKeysItem<TKey> oldItem;

            if (base.RemoveItem(ref item, out oldItem))
            {
                value = (TValue)oldItem._Value.Target;
                return value != null;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Gets or sets a value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to find the association with or to associate the given value with.</param>
        /// <returns>The value found or default(<typeparamref name="TValue"/>) when getting or the specified value when setting.</returns>
        /// <exception cref="ArgumentNullException">Gets raised when the specified <paramref name="value"/> is null.</exception>
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
                    .Select(item => (TValue)item._Value.Target)
                    .Where(v => v != null)
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
                    .Select(item => item._Key)
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

#if !SILVERLIGHT
        #region ISerializable Members

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Comparer", _Comparer);
            info.AddValue(
                "Items", 
                (object)Items
                    .Select(item => new KeyValuePair<TKey,TValue>(item._Key,(TValue)item._Value.Target))
                    .Where(kvp => kvp.Value != null)
                    .ToList()
            );
        }

        #endregion
#endif
    }
}
