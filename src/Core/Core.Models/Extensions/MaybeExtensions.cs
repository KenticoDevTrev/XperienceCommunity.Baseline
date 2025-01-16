namespace CSharpFunctionalExtensions
{
    public static class MaybeExtensions
    {
        /// <summary>
        /// As Maybe for items in index or out param, copies value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Maybe<bool> AsMaybeStatic(this bool? value)
        {
            if (value.HasValue)
            {
                return Maybe.From(value.Value);
            }
            return Maybe.None;
        }

        /// <summary>
        /// As Maybe for items in index or out param, copies value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Maybe<decimal> AsMaybeStatic(this decimal? value)
        {
            if (value.HasValue)
            {
                return Maybe.From(value.Value);
            }
            return Maybe.None;
        }

        /// <summary>
        /// As Maybe for items in index or out param, copies value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Maybe<float> AsMaybeStatic(this float? value)
        {
            if (value.HasValue)
            {
                return Maybe.From(value.Value);
            }
            return Maybe.None;
        }

        /// <summary>
        /// As Maybe for items in index or out param, copies value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Maybe<int> AsMaybeStatic(this int? value)
        {
            if (value.HasValue)
            {
                return Maybe.From(value.Value);
            }
            return Maybe.None;
        }

        /// <summary>
        /// Converts Maybe to a nullable value (useful in serialization)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T? AsNullableValue<T>(this Maybe<T> value) => value.HasValue ? (T?)value.Value : default;

        /// <summary>
        /// Converts Maybe to a nullable value (useful in serialization)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int? AsNullableIntValue(this Maybe<int> value) => value.HasValue ? (int?)value.Value : null;

        /// <summary>
        /// Converts Maybe to a nullable value (useful in serialization)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double? AsNullableDoubleValue(this Maybe<double> value) => value.HasValue ? (double?)value.Value : null;

        /// <summary>
        /// Converts Maybe to a nullable value (useful in serialization)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal? AsNullableDecimalValue(this Maybe<decimal> value) => value.HasValue ? (decimal?)value.Value : null;

        /// <summary>
        /// Converts Maybe to a nullable value (useful in serialization)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool? AsNullableBoolValue(this Maybe<bool> value) => value.HasValue ? (bool?)value.Value : null;

        /// <summary>
        /// Converts Maybe to a nullable value (useful in serialization)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime? AsNullableDateTimeValue(this Maybe<DateTime> value) => value.HasValue ? (DateTime?)value.Value : null;
        
        /// <summary>
        /// Converts Maybe to a nullable value (useful in serialization)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float? AsNullableFloatValue(this Maybe<float> value) => value.HasValue ? (float?)value.Value : null;

        /// <summary>
        /// Adds the Maybe<T>.GetValueOrDefault functionality to normal nullable values (so you don't need to parse to maybe first).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetValueOrDefault<T>(this T? value, T defaultValue) => value ?? defaultValue;

        /// <summary>
        /// Returns a Maybe.None if it's null or the default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Maybe<T> WithDefaultAsNone<T>(this T value) =>
            value == null || value.Equals(default(T))
                ? Maybe<T>.None
                : value;

        /// <summary>
        /// Returns a Maybe.None if the value is null, default, or matches the given value (Kentio often returns a '0' for integars if null, for example, so you can use this for those cases)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="noneValue"></param>
        /// <returns></returns>
        public static Maybe<T> WithMatchAsNone<T>(this T value, T noneValue) =>
            value == null || value.Equals(default(T)) || value.Equals(noneValue)
                ? Maybe<T>.None
                : value;


        /// <summary>
        /// String version of GetValueOrDefault that also accounts for an string that is empty or only contains White Spaces.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetValueOrDefaultIfEmpty(this Maybe<string> value, string defaultValue) => value.HasNonEmptyValue() ? value.Value : defaultValue;

        /// <summary>
        /// Gets the value if the Array has a value and the value is not an empty array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetValueOrDefaultIfEmpty<T>(this Maybe<IEnumerable<T>> value, IEnumerable<T> defaultValue) => value.HasNonEmptyValue() ? value.Value : defaultValue;

        /// <summary>
        /// Boolean check for Maybe<string> values that may have a value that is empty or whitespace, not widely used, you in practice should never have a Maybe<string> 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasNonEmptyValue(this Maybe<string> value) => value.HasValue && !string.IsNullOrWhiteSpace(value.Value);

        /// <summary>
        /// Boolean check for Maybe<string> values that may have a value that is empty or whitespace, not widely used, you in practice should never have a Maybe<string> 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasNonEmptyValue<T>(this Maybe<IEnumerable<T>> value) => value.HasValue && value.Value.Any();

        /// <summary>
        /// IEnumerable check in case it's a null, not widely used.
        /// </summary>
        /// <param name="maybeValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetValueNonEmpty(this Maybe<string> maybeValue, out string value)
        {
            if (maybeValue.HasNonEmptyValue())
            {
                value = maybeValue.Value;
                return true;
            }
            else
            {
                value = String.Empty;
                return false;
            }
        }

        /// <summary>
        /// IEnumerable check in case it's a null, not widely used.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maybeValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetValueNonEmpty<T>(this Maybe<IEnumerable<T>> maybeValue, out IEnumerable<T> value)
        {
            if (maybeValue.HasValue && maybeValue.Value.Any())
            {
                value = maybeValue.Value;
                return true;
            }
            else
            {
                value = [];
                return false;
            }
        }

        /// <summary>
        /// If the condition is true, it returns a Maybe of the given value, if false then a Maybe.None.  Will be Maybe.None if value is null as well.
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <param name="value">The value to check</param>
        /// <param name="condition">The condition to check</param>
        /// <returns>Maybe.None if the condition is false or if the value is null, Maybe of the value if true</returns>
        public static Maybe<T> AsMaybeIfTrue<T>(this T value, Func<T, bool> condition)
        {
            if (value != null && condition(value))
            {
                return value;
            }
            return Maybe.None;
        }

        /// <summary>
        /// Returns a Maybe of the dictionary value, Maybe.None if the key doesn't exist.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key">The lookup key</param>
        /// <returns>Maybe.None if the key doesn't exist, or the value if it does.</returns>
        public static Maybe<TValue> GetValueOrMaybe<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
        {
            return key != null && dictionary.TryGetValue(key, out var value) ? value : Maybe.None;
        }

        /// <summary>
        /// Returns a Maybe of the dictionary value, Maybe.None if the key doesn't exist.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key">The lookup key</param>
        /// <returns>Maybe.None if the key doesn't exist, or the value if it does.</returns>
        public static Maybe<TValue> GetValueOrMaybe<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
        {
            return key != null && dictionary.ContainsKey(key) ? dictionary[key] : Maybe.None;
        }

        /// <summary>
        /// Helper for when you need a sub-value of a Maybe object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TSpecificValType"></typeparam>
        /// <param name="value"></param>
        /// <param name="valueToGetIfTrue">Function that returns the value you want if it's true</param>
        /// <param name="val">The value if retrieved</param>
        /// <returns></returns>
        public static bool TryGetValue<T, TSpecificValType>(this Maybe<T> value, Func<T, TSpecificValType> valueToGetIfTrue, out TSpecificValType val)
        {
            if (value.HasNoValue)
            {
                val = (Maybe<TSpecificValType>.None).GetValueOrDefault();
                return false;
            }
            val = valueToGetIfTrue(value.Value);
            return true;
        }
    }
}
