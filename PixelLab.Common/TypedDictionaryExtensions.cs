using System.Collections.Generic;

namespace PixelLab.Common {
  public static class TypedDictionaryExtensions {
    public static TypedDictionaryKey<TType, TKey> GetKey<TType, TKey>(TKey key) {
      return new TypedDictionaryKey<TType, TKey>(key);
    }

    public static TType GetValue<TType, TKey>(this IDictionary<TKey, object> dictionary, TypedDictionaryKey<TType, TKey> key) {
      return (TType)dictionary[key.Key];
    }

    public static bool TryGetValue<TType, TKey>(this IDictionary<TKey, object> dictionary, TypedDictionaryKey<TType, TKey> key, out TType value) {
      object val;
      if (dictionary.TryGetValue(key.Key, out val)) {
        value = (TType)val;
        return true;
      }
      else {
        value = default(TType);
        return false;
      }
    }

    public static void SetValue<TType, TKey>(this IDictionary<TKey, object> dictionary, TypedDictionaryKey<TType, TKey> key, TType value) {
      dictionary[key.Key] = value;
    }
  }

  public class TypedDictionaryKey<TType, TKey> {
    public TypedDictionaryKey(TKey key) {
      m_key = key;
    }

    public TKey Key { get { return m_key; } }

    private readonly TKey m_key;

  }
}
