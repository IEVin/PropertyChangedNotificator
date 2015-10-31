using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace IEVin.PropertyChangedNotificator.Helper
{
    public static class PropertyChangedNotificatorHelper
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
            if(type == typeof(double))
            {
                precision = precision ?? 1E-15;
                return GetMethodInfo<double>(EqualsDouble);
            }

            if(type == typeof(float))
            {
                precision = precision ?? 1E-7;
                return GetMethodInfo<float>(EqualsSingle);
            }

            if(type == typeof(decimal))
            {
                precision = precision ?? 1E-28;
                return GetMethodInfo<decimal>(EqualsDecimal);
            }

            // precision only for double, float and decimal
            precision = null;

            if(type == typeof(string))
                return GetMethodInfo<string>(string.Equals);

            var method = type.IsClass || type.IsInterface ? "EqualsRef" : "EqualsVal";

            return typeof(PropertyChangedNotificatorHelper)
                .GetMethod(method, BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(type);
        }

        internal static MethodInfo GetRaise(Type type)
        {
            MethodInfo mi;

            try
            {
                mi = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .Where(x => x.IsPublic | x.IsFamily | x.IsFamilyOrAssembly)
                         .Single(x => x.GetCustomAttributes(typeof(NotificationInvocatorAttribute), true).Any());
            }
            catch(Exception ex)
            {
                var msg = string.Format("Type '{0}' contains no single method marked NotificationInvocatorAttribute.", type.FullName);
                throw new InvalidOperationException(msg, ex);
            }

            var prms = mi.GetParameters();

            // Check signature
            if(mi.ReturnType != typeof(void) || prms.Length != 1 || prms[0].ParameterType != typeof(string))
                throw new InvalidOperationException("Invalide signature of notify property changed invocator.");

            return mi;
        }

        [DebuggerStepThrough]
        public static bool EqualsDouble(double a, double b, double eps)
        {
            return Math.Abs(a - b) < eps;
        }

        [DebuggerStepThrough]
        public static bool EqualsSingle(float a, float b, double eps)
        {
            return Math.Abs(a - b) < eps;
        }

        [DebuggerStepThrough]
        public static bool EqualsDecimal(decimal a, decimal b, double eps)
        {
            return Math.Abs(a - b) < (decimal)eps;
        }

        [DebuggerStepThrough]
        public static bool EqualsRef<T>(T a, T b)
            where T : class
        {
            return a != null
                       ? a.Equals(b)
                       : b == null;
        }

        [DebuggerStepThrough]
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