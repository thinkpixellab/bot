using System.Collections.Generic;

namespace PixelLab.Common
{
    public static class TypedDictionaryExtensions
    {
        public static TypedDictionaryKey<TType, TKey> GetKey<TType, TKey>(TKey key)
        {
            return new TypedDictionaryKey<TType, TKey>(key);
        }

        public static TType GetValue<TType, TKey>(this IDictionary<TKey, object> dictionary, TypedDictionaryKey<TType, TKey> key)
        {
            return (TType)dictionary[key.Key];
        }

        public static TType GetValueOrDefault<TType, TKey>(this IDictionary<TKey, object> dictionary, TypedDictionaryKey<TType, TKey> key)
        {
            TType val;
            TryGetValue(dictionary, key, out val);
            return val;
        }

        public static bool TryGetValue<TType, TKey>(this IDictionary<TKey, object> dictionary, TypedDictionaryKey<TType, TKey> key, out TType value)
        {
            object val;
            if (dictionary.TryGetValue(key.Key, out val))
            {
                value = (TType)val;
                return true;
            }
            else
            {
                value = key.DefaultValue;
                return false;
            }
        }

        public static void SetValue<TType, TKey>(this IDictionary<TKey, object> dictionary, TypedDictionaryKey<TType, TKey> key, TType value, bool clearIfDefault = false)
        {
            if (clearIfDefault && EqualityComparer<TType>.Default.Equals(value, key.DefaultValue))
            {
                dictionary.Remove(key.Key);
            }
            else
            {
                dictionary[key.Key] = value;
            }
        }
    }

    public class TypedDictionaryKey<TType, TKey>
    {
        public TypedDictionaryKey(TKey key, TType defaultValue = default(TType))
        {
            m_key = key;
            m_defaultValue = defaultValue;
        }

        public TKey Key { get { return m_key; } }
        public TType DefaultValue { get { return m_defaultValue; } }

        private readonly TKey m_key;
        private readonly TType m_defaultValue;
    }
}
