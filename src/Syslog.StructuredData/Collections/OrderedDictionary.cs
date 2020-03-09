using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Syslog.Collections
{
    internal class OrderedDictionary<TKey, TValue> :
        KeyedCollection<TKey, KeyValuePair<TKey, TValue>>,
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
    {
        public OrderedDictionary()
        {
        }

        public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            foreach (var kvp in keyValuePairs)
            {
                Add(kvp);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var keys = new Collection<TKey>();
                foreach (var item in this)
                {
                    keys.Add(item.Key);
                }

                return keys;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public ICollection<TValue> Values
        {
            get
            {
                var values = new Collection<TValue>();
                foreach (var item in this)
                {
                    values.Add(item.Value);
                }

                return values;
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key].Value;
            }
            set
            {
                if (ContainsKey(key))
                {
                    Remove(key);
                }

                Add(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key].Value;

        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return Contains(key);
        }

        protected override TKey GetKeyForItem(KeyValuePair<TKey, TValue> item)
        {
            return item.Key;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (Contains(key))
            {
                value = this[key].Value;
                return true;
            }

            value = default!;
            return false;
        }
    }
}
