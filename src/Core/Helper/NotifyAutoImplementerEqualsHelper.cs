using System;
using System.Linq;
using System.Reflection;

namespace IEVin.NotifyAutoImplementer.Core.Helper
{
    public static class NotifyAutoImplementerEqualsHelper
    {
        static MethodInfo GetMethodInfo<T>(Func<T, T, bool> func)
        {
            return func.Method;
        }

        internal static MethodInfo GetRaise(Type type)
        {
            var mi = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .Where(x => x.IsPublic | x.IsFamily | x.IsFamilyOrAssembly)
                         .Single(x => x.GetCustomAttributes(typeof(NotifyInvocatorAttribute), true).Any());

            var prms = mi.GetParameters();

            // Check signature
            if(mi.ReturnType != typeof(void) || prms.Length != 1 || prms[0].ParameterType != typeof(string))
                throw new InvalidOperationException("Invalide signature of notify property changed invocator.");

            return mi;
        }

        internal static MethodInfo GetEquals(Type type)
        {
            if(type == typeof(String))
                return GetMethodInfo<String>(String.Equals);

            if(type == typeof(Double))
                return GetMethodInfo<Double>(EqualsDouble);

            if(type == typeof(Single))
                return GetMethodInfo<Single>(EqualsSingle);

            var method = type.IsClass ? "EqualsRef" : "EqualsVal";

            return typeof(NotifyAutoImplementerEqualsHelper)
                .GetMethod(method, BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(type);
        }


        public static bool EqualsDouble(Double a, Double b)
        {
            var la = BitConverter.DoubleToInt64Bits(a);
            var lb = BitConverter.DoubleToInt64Bits(b);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            return (la >> 63) == (lb >> 63)
                       ? Math.Abs(la - lb) <= 1
                       : a == b; // compare -0 and +0
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public static bool EqualsSingle(Single a, Single b)
        {
            return EqualsDouble(a, b);
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