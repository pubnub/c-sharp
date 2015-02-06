using System;
using System.Collections.Generic;

namespace PubNubMessaging.Core
{
    public class SafeDictionary<TKey, TValue>: IDictionary<TKey, TValue>
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
            //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Locking", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
            lock (syncRoot)
            {
                //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, insynsc", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
                if (d.ContainsKey (key)) {
                    //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, ContainsKey", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);    
                    d [key] = value;
                } else {
                    //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, adding key ", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
                    d.Add (key, value);
                    //LoggingMethod.WriteToLog (string.Format ("DateTime {0},  key added ", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
                }

                return d [key];
            }
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            lock (syncRoot)
            {
                TValue val;
                if (d.TryGetValue (key, out val)) {
                    //LoggingMethod.WriteToLog (string.Format ("DateTime {0},  in try get value ", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
                    return val;
                } else {
                    d.Add(key, value);
                    //LoggingMethod.WriteToLog (string.Format ("DateTime {0},  key added2 ", DateTime.Now.ToString ()), LoggingMethod.LevelInfo);
                    return d [key];
                }
                /*if (d.ContainsKey (key)) {
                    return d [key];
                } else {
                    TValue val;
                    if (d.TryGetValue (key, out val)) {
                        return val;
                    } else {
                        return val;
                    }
                }*/
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

        public bool Remove(TKey key){
            throw new NotImplementedException ();
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
            return Remove (key, out value);
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

        public void Add (KeyValuePair<TKey,TValue> item)
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

        public bool Contains(KeyValuePair<TKey, TValue>item)
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

        public bool Remove(KeyValuePair<TKey, TValue>item)
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
            return ((System.Collections.IEnumerable)d).GetEnumerator( );
        }

        #endregion
    }
}

