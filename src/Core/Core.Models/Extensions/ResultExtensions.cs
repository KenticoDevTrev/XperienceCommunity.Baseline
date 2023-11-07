namespace Core.Extensions
{
    public static class ResultExtensions
    {
        public static Maybe<T> AsMaybeIfSuccessful<T>(this Result<T> value)
        {
            return value.IsSuccess ? value.Value : Maybe.None;
        }
    }
}
