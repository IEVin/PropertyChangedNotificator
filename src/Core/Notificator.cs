using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using IEVin.PropertyChangedNotificator.Helper;

namespace IEVin.PropertyChangedNotificator
{
    public static class Notificator
    {
        static readonly Lazy<ModuleBuilder> s_builder = new Lazy<ModuleBuilder>(CreateModule);

        public static void Create<T>(T obj)
            where T : INotifyPropertyChanged
        {
            var type = GetTypeAndCheck(obj);
            if(type != typeof(T))
                return;

            var proxyType = TypeCache<T>.GetOrCreate(CreateProxyType);
            SetType(obj, proxyType);
        }

        static Type GetTypeAndCheck(object obj)
        {
            if(obj == null)
                throw new ArgumentNullException(nameof(obj));

            var type = obj.GetType();

            if(type.IsAbstract || type.IsSealed)
                throw new ArgumentException("Type сannot be abstract or sealed.");

            return type;
        }

        [DebuggerStepThrough]
        static void SetType(object obj, Type proxyType)
        {
            unsafe
            {
                var conv = new ObjectToStructConv { From = new ObjWrap { Value = obj } };
                conv.To.Value->MethodTable = proxyType.TypeHandle.Value;
            }
        }

        [DebuggerStepThrough]
        static Type CreateProxyType(Type type)
        {
            var tb = s_builder.Value.DefineType(type.FullName, type.Attributes, type);
            tb.AddInterfaceImplementation(typeof(INotifyPropertyChanged));

            var raiseMi = PropertyChangedNotificatorHelper.GetRaise(type);

            foreach(var q in GetPropertyNames(type))
            {
                // MethodInfo.GetCustomAttributes has error (ignore 'inherit = true' for properties). Attribute.GetCustomAttributes is correct.
                var attribs = Attribute.GetCustomAttributes(q, typeof(NotifyPropertyAttribute), inherit: true);
                if(!attribs.Any())
                    continue;

                var name = q.Name;
                var getter = q.GetGetMethod(true);
                var setter = q.GetSetMethod(true);

                if(getter == null || getter.IsPrivate || getter.IsAssembly)
                {
                    var msg = $"Getter property '{q.Name}' of type '{type.FullName}' must be public or protected.";
                    throw new InvalidOperationException(msg);
                }

                if(setter == null || setter.IsPrivate || setter.IsAssembly)
                {
                    var msg = $"Setter property '{q.Name}' of type '{type.FullName}' must be public or protected.";
                    throw new InvalidOperationException(msg);
                }

                if(!setter.IsVirtual)
                {
                    var msg = $"Setter property '{q.Name}' of type '{type.FullName}' must be virtual.";
                    throw new InvalidOperationException(msg);
                }


                double? precision;
                var equalsMi = PropertyChangedNotificatorHelper.GetEquals(getter.ReturnType, out precision);

                if(precision != null)
                {
                    precision = q.GetCustomAttributes(typeof(SetPrecisionAttribute), true)
                                 .Cast<SetPrecisionAttribute>()
                                 .Select(x => (double?)x.Precision)
                                 .FirstOrDefault() ?? precision;
                }

                var notifyNames = attribs.Cast<NotifyPropertyAttribute>()
                                         .Select(x => x.PropertyName ?? name);

                var newSetter = CreateSetMethod(tb, getter, setter, notifyNames, raiseMi, equalsMi, precision);
                tb.DefineMethodOverride(newSetter, setter);
            }

            return tb.CreateType();
        }

        [DebuggerStepThrough]
        static MethodBuilder CreateSetMethod(TypeBuilder tb, MethodInfo getMi, MethodInfo setMi,
                                             IEnumerable<string> names, MethodInfo raise, MethodInfo equals, double? eps)
        {
            var paramTypes = setMi.GetParameters()
                                  .Select(x => x.ParameterType)
                                  .ToArray();

            var mb = tb.DefineMethod(setMi.Name, setMi.Attributes, null, paramTypes);

            var il = mb.GetILGenerator();
            var label = il.DefineLabel();

            // get current value
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, getMi);

            // invoke equals
            il.Emit(OpCodes.Ldarg_1);
            if(eps != null)
                il.Emit(OpCodes.Ldc_R8, eps.Value);
            il.Emit(OpCodes.Call, equals);

            // jump if equals
            il.Emit(OpCodes.Brtrue_S, label);

            // set new value
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, setMi);

            // raise OnPropertyChanged events
            foreach(var q in names)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, q);
                il.Emit(OpCodes.Call, raise);
            }

            // exit
            il.MarkLabel(label);
            il.Emit(OpCodes.Ret);

            return mb;
        }

        [DebuggerStepThrough]
        static IEnumerable<PropertyInfo> GetPropertyNames(Type type)
        {
            return type?.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty)
                   ?? Enumerable.Empty<PropertyInfo>();
        }

        [DebuggerStepThrough]
        static ModuleBuilder CreateModule()
        {
            var assemblyName = new AssemblyName($"NAImplementerAssembly_{Guid.NewGuid().ToString()}");

            return AppDomain.CurrentDomain
                            .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
                            .DefineDynamicModule(assemblyName.Name);
        }
    }
}