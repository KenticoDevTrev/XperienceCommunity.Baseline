namespace System.Collections
{
    public static class HashTableExtensions
    {
        public static void AddOrReplace(this Hashtable table, string key, object value)
        {
            if (table.ContainsKey(key)) {
                table[key] = value;
            } else {
                table.Add(key, value);
            }
        }
    }
}
