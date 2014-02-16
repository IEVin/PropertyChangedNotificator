using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;
using IEVin.PropertyChangedNotificator.Helper;

namespace IEVin.PropertyChangedNotificator
{
    public static class Notificator
    {
        static readonly Lazy<ModuleBuilder> s_builder = new Lazy<ModuleBuilder>(CreateModule);

        static readonly ConcurrentDictionary<Guid, Func<INotifyPropertyChanged>> s_cache = new ConcurrentDictionary<Guid, Func<INotifyPropertyChanged>>();


        public static T Of<T>()
            where T : INotifyPropertyChanged, new()
        {
            var ctor = GetOrCreateProxyTypeCtor(typeof(T));
            return (T)ctor();
        }

        public static Func<T> ConstructorOf<T>()
            where T : INotifyPropertyChanged, new()
        {
            var ctor = GetOrCreateProxyTypeCtor(typeof(T));
            return () => (T)ctor();
        }

        public static Func<INotifyPropertyChanged> ConstructorOf(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (!typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                throw new ArgumentException("Type must implement INotifyPropertyChanged.");

            return GetOrCreateProxyTypeCtor(type);
        }

        [DebuggerHidden]
        static Func<INotifyPropertyChanged> GetOrCreateProxyTypeCtor(Type type)
        {
            if(type.IsAbstract || type.IsSealed)
                throw new ArgumentException("Type сannot be abstract or sealed.");

            var guid = type.GUID;

            Func<INotifyPropertyChanged> ctor;
            if(s_cache.TryGetValue(guid, out ctor))
                return ctor;

            return s_cache.GetOrAdd(guid, x =>
                                              {
                                                  var proxyType = CreateProxyType(type);
                                                  return CreateConstructior(proxyType);
                                              });
        }

        [DebuggerHidden]
        static Type CreateProxyType(Type type)
        {
            var tb = s_builder.Value.DefineType(type.FullName + "_NotifyImplementation", type.Attributes, type);
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
                    var msg = string.Format("Getter property '{0}' of type '{1}' must be public or protected.", q.Name, type.FullName);
                    throw new InvalidOperationException(msg);
                }

                if(setter == null || setter.IsPrivate || setter.IsAssembly)
                {
                    var msg = string.Format("Setter property '{0}' of type '{1}' must be public or protected.", q.Name, type.FullName);
                    throw new InvalidOperationException(msg);
                }

                if(!setter.IsVirtual)
                {
                    var msg = string.Format("Setter property '{0}' of type '{1}' must be virtual.", q.Name, type.FullName);
                    throw new InvalidOperationException(msg);
                }


                var precision = q.GetCustomAttributes(typeof(SetPrecisionAttribute), true)
                                 .Cast<SetPrecisionAttribute>()
                                 .Select(x => (double?)x.Precision)
                                 .FirstOrDefault();

                var equalsMi = PropertyChangedNotificatorHelper.GetEquals(getter.ReturnType, ref precision);

                var notifyNames = attribs.Cast<NotifyPropertyAttribute>()
                                         .Select(x => x.PropertyName ?? name)
                                         .Distinct();

                var newSetter = CreateSetMethod(tb, getter, setter, notifyNames, raiseMi, equalsMi, precision);
                tb.DefineMethodOverride(newSetter, setter);
            }

            return tb.CreateType();
        }

        [DebuggerHidden]
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

        [DebuggerHidden]
        static Func<INotifyPropertyChanged> CreateConstructior(Type type)
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if(ctor == null)
                return () => (INotifyPropertyChanged)Activator.CreateInstance(type);

            var dm = new DynamicMethod(type.FullName + "_ctor", type, Type.EmptyTypes, typeof(Notificator).Module);
            var il = dm.GetILGenerator();

            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);

            return (Func<INotifyPropertyChanged>)dm.CreateDelegate(typeof(Func<INotifyPropertyChanged>));
        }

        [DebuggerHidden]
        static IEnumerable<PropertyInfo> GetPropertyNames(Type type)
        {
            return type != null
                       ? type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty)
                       : Enumerable.Empty<PropertyInfo>();
        }

        [DebuggerHidden]
        static ModuleBuilder CreateModule()
        {
            var assemblyName = new AssemblyName(string.Format("NAImplementerAssembly_{0}", Guid.NewGuid().ToString()));

            return AppDomain.CurrentDomain
                            .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
                            .DefineDynamicModule(assemblyName.Name);
        }
    }
}