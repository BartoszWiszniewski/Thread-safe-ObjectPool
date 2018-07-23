namespace ObjectPool.Extensions
{
    using System;

    public static class TypeExtensions
    {
        public static bool HasEmptyConstructor(this Type type)
        {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsNullable(this Type type)
        {
            if (type.IsValueType)
            {
                return false;
            }

            return type.IsClass || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static T New<T>(this Type type)
        {
            return (T)type.TypeInitializer.Invoke(null);
        }

        public static T New<T>(this Type type, object[] parameters)
        {
            return (T)type.TypeInitializer.Invoke(parameters);
        }
    }
}