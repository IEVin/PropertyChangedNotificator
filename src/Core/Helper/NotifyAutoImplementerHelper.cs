using System;
using System.Linq;
using System.Reflection;

namespace IEVin.NotifyAutoImplementer.Core.Helper
{
    public static class NotifyAutoImplementerHelper
    {
        static MethodInfo GetMethodInfo<T>(Func<T, T, bool> func)
        {
            return func.Method;
        }

        static MethodInfo GetMethodInfo<T>(Func<T, T, double, bool> func)
        {
            return func.Method;
        }

        internal static MethodInfo GetEquals(Type type, ref double? precision)
        {
            if(type == typeof(Double))
            {
                precision = precision ?? 1E-15;
                return GetMethodInfo<Double>(EqualsDouble);
            }

            if(type == typeof(Single))
            {
                precision = precision ?? 1E-7;
                return GetMethodInfo<Single>(EqualsSingle);
            }

            if(type == typeof(Decimal))
            {
                precision = precision ?? 1E-28;
                return GetMethodInfo<Decimal>(EqualsDecimal);
            }

            // precision only for double, float and decimal
            precision = null;

            if (type == typeof(String))
                return GetMethodInfo<String>(String.Equals);

            var method = type.IsClass ? "EqualsRef" : "EqualsVal";

            return typeof(NotifyAutoImplementerHelper)
                .GetMethod(method, BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(type);
        }

        internal static MethodInfo GetRaise(Type type)
        {
            var mi = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .Where(x => x.IsPublic | x.IsFamily | x.IsFamilyOrAssembly)
                         .Single(x => x.GetCustomAttributes(typeof(NotificationInvocatorAttribute), true).Any());

            var prms = mi.GetParameters();

            // Check signature
            if (mi.ReturnType != typeof(void) || prms.Length != 1 || prms[0].ParameterType != typeof(string))
                throw new InvalidOperationException("Invalide signature of notify property changed invocator.");

            return mi;
        }

        public static bool EqualsDouble(Double a, Double b, double eps)
        {
            return Math.Abs(a - b) < eps;
        }

        public static bool EqualsSingle(Single a, Single b, double eps)
        {
            return Math.Abs(a - b) < eps;
        }

        public static bool EqualsDecimal(Decimal a, Decimal b, double eps)
        {
            return Math.Abs(a - b) < (Decimal)eps;
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