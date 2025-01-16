namespace System.Collections.Generic
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// A shortcut for if(MyArray.Any()) { var first = MyArray.First(); }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="defaultIsNone"></param>
        /// <returns></returns>
        public static Maybe<T> FirstOrMaybe<T>(this IEnumerable<T> collection, bool defaultIsNone = true)
        {
            if (collection.Any())
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (defaultIsNone && collection.First().Equals(default(T)))
                {
                    return Maybe.None;
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                return Maybe.From(collection.First());
            }
            else
            {
                return Maybe.None;
            }
        }

        /// <summary>
        /// A shortcut for if(MyArray.Any(x => predicate(x))) { var first = MyArray.First(); }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <param name="defaultIsNone"></param>
        /// <returns></returns>
        public static Maybe<T> FirstOrMaybe<T>(this IEnumerable<T> collection, Func<T, bool> predicate, bool defaultIsNone = true)
        {
            var items = collection.Where(predicate);
            if (items.Any())
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (defaultIsNone && items.First().Equals(default(T)))
                {
                    return Maybe.None;
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                return Maybe.From(items.First());
            }
            else
            {
                return Maybe.None;
            }
        }

        /// <summary>
        /// A shortcut for if(MyArray.Any()) { var first = MyArray.First(); }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value">The value that will be set to the first item if correct</param>
        /// <param name="defaultIsNone"></param>
        /// <returns></returns>
        public static bool TryGetFirst<T>(this IEnumerable<T> collection, out T value, bool defaultIsNone = true)
        {
            if (collection.Any())
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (defaultIsNone && collection.First().Equals(default(T)))
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
                    return false;
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                value =collection.First();
                return true;
            }
#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
            return false;
        }

        /// <summary>
        /// A shortcut for if(MyArray.Any(x => predicate(x))) { var first = MyArray.First(); }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <param name="defaultIsNone"></param>
        /// <returns></returns>
        public static bool TryGetFirst<T>(this IEnumerable<T> collection, Func<T, bool> predicate, out T value, bool defaultIsNone = true)
        {
            var items = collection.Where(predicate);
            if (items.Any())
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (defaultIsNone && items.First().Equals(default(T)))
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
                    return false;
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                value = items.First();
                return true;
            }
#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
            return false;
        }

        /// <summary>
        /// Similar to FirstOrMaybe() but handles null arrays safetly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Maybe<IEnumerable<T>> WithEmptyAsNone<T>(this IEnumerable<T> value) =>
            value == null || !value.Any() ?
            Maybe<IEnumerable<T>>.None :
            Maybe.From(value);

        /// <summary>
        /// Shortcut for string.Join(separator, array[string])
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string StringJoin(this IEnumerable<string> collection, string separator)
        {
            return string.Join(separator, collection.ToArray());
        }
    }
}
