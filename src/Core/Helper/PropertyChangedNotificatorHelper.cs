using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace IEVin.PropertyChangedNotificator.Helper
{
    public static class PropertyChangedNotificatorHelper
    {
        internal static MethodInfo GetEquals(Type type, out double? precision)
        {
            // precision only for double, float and decimal
            precision = null;
            string methodName;

            if(type == typeof(double))
            {
                precision = 1E-15;
                methodName = nameof(EqualsDouble);
            }
            else if(type == typeof(float))
            {
                precision = 1E-7;
                methodName = nameof(EqualsSingle);
            }
            else if(type == typeof(decimal))
            {
                precision = 1E-28;
                methodName = nameof(EqualsDecimal);
            }
            else if(type.IsClass || type.IsInterface)
            {
                methodName = nameof(EqualsRef);
            }
            else if(typeof(IEquatable<>).MakeGenericType(type).IsAssignableFrom(type))
            {
                methodName = nameof(EqualsValEquatable);
            }
            else
            {
                methodName = nameof(EqualsVal);
            }

            var method = typeof(PropertyChangedNotificatorHelper).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);

            if(precision == null)
                method = method.MakeGenericMethod(type);

            return method;
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
                var msg = $"Type '{type.FullName}' contains no single method marked NotificationInvocatorAttribute.";
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
            if(a == b)
                return true;
            if(a == null || b == null)
                return false;
            return a.Equals(b);
        }

        [DebuggerStepThrough]
        public static bool EqualsVal<T>(T a, T b)
            where T : struct
        {
            return a.Equals(b);
        }

        [DebuggerStepThrough]
        public static bool EqualsValEquatable<T>(T a, T b)
            where T : struct, IEquatable<T>
        {
            return a.Equals(b);
        }
    }
}