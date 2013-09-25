using System;
using System.Reflection;

namespace IEVin.NotifyAutoImplementer.Core
{
    public static class DynamicHelper
    {
        internal static MethodInfo GetRaise()
        {
            return typeof(NotifyPropertyObject)
                .GetMethod("RaisePropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        internal static MethodInfo GetEquals(Type type)
        {
            var method = type.IsClass ? "EqualsRef" : "EqualsVal";

            return typeof(DynamicHelper)
                .GetMethod(method, BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(type);
        }

        public static bool EqualsRef<T>(T a, T b)
            where T : class
        {
            return a != null
                       ? a.Equals(b)
                       : b == null;
        }

        public static bool EqualsVal<T>(T a, T b)
            where T : struct
        {
            var eq = a as IEquatable<T>;
            return eq != null
                       ? eq.Equals(b)
                       : a.Equals(b);
        }
    }
}