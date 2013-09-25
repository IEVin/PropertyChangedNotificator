using System;
using System.Reflection;

namespace IEVin.NotifyAutoImplementer.Core.Helper
{
    public static class NotifyAutoImplementerEqualsHelper
    {
        internal static MethodInfo GetRaise()
        {
            return typeof(NotificationObject)
                .GetMethod("RaisePropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        internal static MethodInfo GetEquals(Type type)
        {
            var method = type.IsClass ? "EqualsRef" : "EqualsVal";

            return typeof(NotifyAutoImplementerEqualsHelper)
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