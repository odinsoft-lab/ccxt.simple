using System;
using System.Collections.Generic;

namespace CCXT.Simple.Core.Extensions
{
    public static class DictionaryExtensions
    {
    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));
            return dict.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
