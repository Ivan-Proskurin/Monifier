using System.Collections.Generic;

namespace Monifier.Common.Extensions
{
    public static class DictionayExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var value) ? value : default(TValue);
        }
    }
}