namespace System
{
    public static class ToObjectIdentityHelper
    {
        public static ObjectIdentity ToObjectIdentity(this int value)
        {
            return new ObjectIdentity()
            {
                Id = value
            };
        }

        public static IEnumerable<ObjectIdentity> ToObjectIdentity(this IEnumerable<int> values)
        {
            return values.Select(x => new ObjectIdentity()
            {
                Id = x
            });
        }

        public static ObjectIdentity ToObjectIdentity(this string value)
        {
            return new ObjectIdentity()
            {
                CodeName = value
            };
        }

        public static IEnumerable<ObjectIdentity> ToObjectIdentity(this IEnumerable<string> values)
        {
            return values.Select(x => new ObjectIdentity()
            {
                CodeName = x
            });
        }

        public static ObjectIdentity ToObjectIdentity(this Guid value)
        {
            return new ObjectIdentity()
            {
                Guid = value
            };
        }

        public static IEnumerable<ObjectIdentity> ToObjectIdentity(this IEnumerable<Guid> values)
        {
            return values.Select(x => new ObjectIdentity()
            {
                Guid = x
            });
        }
    }
}
