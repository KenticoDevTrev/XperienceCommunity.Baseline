namespace Core.Extensions
{
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts a Result<T> to a Maybe<T> based on if it's successful
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Maybe<T> AsMaybeIfSuccessful<T>(this Result<T> value)
        {
            return value.IsSuccess ? value.Value : Maybe.None;
        }
    }
}
