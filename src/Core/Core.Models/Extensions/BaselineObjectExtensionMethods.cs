namespace System
{
    public static class BaselineObjectExtensionMethods
    {
        /// <summary>
        /// Returns true if the given string is null.  this does NOT check for whitespace / empty, use .AsNullOrWhitespaceTryGetValue instead for that.
        /// </summary>
        /// <param name="inValue"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool TryGetValue(this string? inValue, out string output)
        {
            if (inValue != null) {
                output = inValue;
                return true;
            }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            output = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return false;
        }

        public static bool TryGetValue<T>(this T? obj, out T output)
        {
            if (obj != null) {
                output = obj;
                return true;
            }

#pragma warning disable CS8601 // Possible null reference assignment, ignoring because this specifically should not reference the output if this returns false
            output = default;
#pragma warning restore CS8601 // Possible null reference assignment.
            return false;
        }
    }
}
