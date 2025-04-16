namespace Core.Models
{
    public static class PageIdentityExtensions
    {
        public static PageIdentity<T> ToTypedPageIdentity<T>(this PageIdentity pageIdentity, T model) => new(model, pageIdentity);

        public static PageIdentity<T> ToTypedPageIdentity<T, TOriginalType>(this PageIdentity<TOriginalType> pageIdentity, T model) => new(model, pageIdentity);

        public static PageIdentity<T> ToTypedPageIdentity<T, TOriginalType>(this PageIdentity<TOriginalType> pageIdentity, Func<TOriginalType, T> converter) => new(converter.Invoke(pageIdentity.Data), pageIdentity);
    }
}
