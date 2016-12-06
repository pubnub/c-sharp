using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public sealed class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly object syncRoot = new object();
        private Dictionary<TKey, TValue> d = new Dictionary<TKey, TValue>();

        #region IDictionary<TKey,TValueMembers>

        public void Add(TKey key, TValue value)
        {
            lock (syncRoot)
            {
                d.Add(key, value);
            }
        }

        public TValue AddOrUpdate(TKey key, TValue value, Func<TKey, TValue, TValue> f)
        {
            lock (syncRoot)
            {
                TValue findValue;

                if (d.TryGetValue(key, out findValue))
                {
                    d[key] = f(key, findValue);
                }
                else
                {
                    d.Add(key, value);
                }

                return d[key];
            }
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            bool updated = false;
            if (key == null) throw new ArgumentNullException("key");

            lock (syncRoot)
            {
                if (d.ContainsKey(key))
                {
                    d[key] = newValue;
                    updated = true;
                }

                return updated;
            }
        }
        public TValue GetOrAdd(TKey key, TValue value)
        {
            lock (syncRoot)
            {
                TValue val;
                if (d.TryGetValue(key, out val))
                {
                    return val;
                }
                else
                {
                    d.Add(key, value);
                    return d[key];
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            return d.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (syncRoot)
                {
                    return d.Keys;
                }
            }
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKey key, out TValue value)
        {
            lock (syncRoot)
            {
                d.TryGetValue(key, out value);
                return d.Remove(key);
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            return Remove(key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (syncRoot)
            {
                return d.TryGetValue(key, out value);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (syncRoot)
                {
                    return d.Values;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return d[key];
            }
            set
            {
                lock (syncRoot)
                {
                    d[key] = value;
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)d).Add(item);
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                d.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey,
                TValue>>)d).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int
            arrayIndex)
        {
            lock (syncRoot)
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)d).CopyTo(array,
                    arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                return d.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)d).Remove(item);
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey,
                TValue>>)d).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)d).GetEnumerator();
        }

        #endregion
    }
}
