using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;
using IEVin.NotifyAutoImplementer.Core.Helper;

namespace IEVin.NotifyAutoImplementer.Core
{
    public static class NotifyImplementer
    {
        static readonly Lazy<ModuleBuilder> s_builder = new Lazy<ModuleBuilder>(CreateModule);

        static readonly ConcurrentDictionary<Guid, Func<INotifyPropertyChanged>> s_cache = new ConcurrentDictionary<Guid, Func<INotifyPropertyChanged>>();

        public static T CreateInstance<T>()
            where T : NotificationObject, new()
        {
            try
            {
                var ctor = GetOrCreateProxyTypeCtor(typeof(T));
                return (T)ctor();
            }
            catch(ArgumentException ex)
            {
                throw new ArgumentException(ex.Message, ex);
            }
        }

        public static object CreateInstance(Type type)
        {
            try
            {
                var ctor = GetOrCreateProxyTypeCtor(type);
                return ctor();
            }
            catch(ArgumentException ex)
            {
                throw new ArgumentException(ex.Message, "type", ex);
            }
        }

        static Func<INotifyPropertyChanged> GetOrCreateProxyTypeCtor(Type type)
        {
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

        static Type CreateProxyType(Type type)
        {
            var tb = s_builder.Value.DefineType(type.FullName + "_NotifyImplementation", type.Attributes, type);
            tb.AddInterfaceImplementation(typeof(INotifyPropertyChanged));

            foreach(var q in GetPropertyNames(type))
            {
                var attribs = q.GetCustomAttributes(typeof(NotifyPropertyAttribute), true);
                if(!attribs.Any())
                    continue;

                var name = q.Name;
                var getter = q.GetGetMethod(true);
                var setter = q.GetSetMethod(true);

                if(getter == null || getter.IsPrivate || getter.IsAssembly)
                {
                    var msg = string.Format("Getter property '{0}' of type '{1}' must be public or protected.", q.Name, type.FullName);
                    throw new ArgumentException(msg);
                }

                if(setter == null || setter.IsPrivate || setter.IsAssembly)
                {
                    var msg = string.Format("Setter property '{0}' of type '{1}' must be public or protected.", q.Name, type.FullName);
                    throw new ArgumentException(msg);
                }

                if(!setter.IsVirtual)
                {
                    var msg = string.Format("Setter property '{0}' of type '{1}' must be virtual.", q.Name, type.FullName);
                    throw new ArgumentException(msg);
                }

                var notifyNames = attribs.Cast<NotifyPropertyAttribute>()
                                         .Select(x => x.PropertyName ?? name)
                                         .Distinct();

                var newSetter = CreateSetMethod(tb, getter, setter, notifyNames);
                tb.DefineMethodOverride(newSetter, setter);
            }

            return tb.CreateType();
        }

        static MethodBuilder CreateSetMethod(TypeBuilder tb, MethodInfo getMi, MethodInfo setMi, IEnumerable<string> names)
        {
            var paramTypes = setMi.GetParameters()
                                  .Select(x => x.ParameterType)
                                  .ToArray();

            var mb = tb.DefineMethod(setMi.Name, setMi.Attributes, null, paramTypes);

            var equals = NotifyAutoImplementerEqualsHelper.GetEquals(getMi.ReturnType);
            var raise = NotifyAutoImplementerEqualsHelper.GetRaise();

            var il = mb.GetILGenerator();
            var label = il.DefineLabel();

            // get current value
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, getMi);

            // invoke equals
            il.Emit(OpCodes.Ldarg_1);
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

        static Func<INotifyPropertyChanged> CreateConstructior(Type type)
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if(ctor == null)
                return () => (INotifyPropertyChanged)Activator.CreateInstance(type);

            var dm = new DynamicMethod(type.FullName + "_ctor", type, Type.EmptyTypes, typeof(NotifyImplementer).Module);
            var il = dm.GetILGenerator();

            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);

            return (Func<INotifyPropertyChanged>)dm.CreateDelegate(typeof(Func<INotifyPropertyChanged>));
        }

        static IEnumerable<PropertyInfo> GetPropertyNames(Type type)
        {
            // TODO: verify implementation
            return type != null
                       ? type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty)
                       : Enumerable.Empty<PropertyInfo>();
        }

        static ModuleBuilder CreateModule()
        {
            var assemblyName = new AssemblyName(string.Format("NAImplementerAssembly_{0}", Guid.NewGuid().ToString()));

            return AppDomain.CurrentDomain
                            .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
                            .DefineDynamicModule(assemblyName.Name);
        }
    }
}