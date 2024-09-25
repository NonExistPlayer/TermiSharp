namespace TermiSharp
{
    public static class DictionaryExtensions
    {
        public static TKey? FindKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value) where TKey : notnull where TValue : notnull
        {
            foreach (KeyValuePair<TKey, TValue> keyvalue in dictionary)
                if (keyvalue.Value.Equals(value)) return keyvalue.Key;
            return default;
        }
    }
}