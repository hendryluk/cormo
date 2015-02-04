using System.Collections.Generic;

namespace Cormo.Impl.Utils
{
    public static class DictionaryUtils
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue val;
            if (dictionary.TryGetValue(key, out val))
                return val;

            return default(TValue);
        }
    }
}