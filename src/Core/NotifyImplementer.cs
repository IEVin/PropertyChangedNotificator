using System;
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

        static readonly Dictionary<Guid, Type> s_cache = new Dictionary<Guid, Type>();

        public static T CreateInstance<T>()
            where T : NotificationObject, new()
        {
            try
            {
                var newType = GetOrCreateProxyType(typeof(T));
                return (T)Activator.CreateInstance(newType);
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
                var newType = GetOrCreateProxyType(type);
                return Activator.CreateInstance(newType);
            }
            catch(ArgumentException ex)
            {
                throw new ArgumentException(ex.Message, "type", ex);
            }
        }

        static Type GetOrCreateProxyType(Type baseType)
        {
            var guid = baseType.GUID;

            //TODO: Add sync
            Type type;
            if(!s_cache.TryGetValue(guid, out type))
            {
                type = CreateProxyType(baseType);
                s_cache[guid] = type;
            }

            //TODO: Add dynamic method
            return type;
        }

        static Type CreateProxyType(Type type)
        {
            var tb = s_builder.Value.DefineType(type.FullName + "_NotifyImplementation", type.Attributes, type);
            tb.AddInterfaceImplementation(typeof(INotifyPropertyChanged));


            foreach(var q in GetPropertyNames(type))
            {
                var name = q.Name;
                var gettet = q.GetGetMethod(true);
                var setter = q.GetSetMethod(true);
                if(setter == null)
                    continue;

                var attribs = q.GetCustomAttributes(typeof(NotifyPropertyAttribute), true);

                if(!setter.IsVirtual)
                {
                    if(!attribs.Any())
                        continue;

                    var msg = string.Format("Property '{0}' of type '{1}' must be virtual", q.Name, type.FullName);
                    throw new ArgumentException(msg);
                }

                var notifyNames = attribs.Cast<NotifyPropertyAttribute>()
                                         .Select(x => x.PropertyName ?? name)
                                         .Distinct();

                var prop = tb.DefineProperty(name, q.Attributes, q.PropertyType, Type.EmptyTypes);
                var newSetter = CreateSetMethod(tb, gettet, setter, notifyNames);

                tb.DefineMethodOverride(newSetter, setter);
                prop.SetSetMethod(newSetter);
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

        static IEnumerable<PropertyInfo> GetPropertyNames(Type type)
        {
            // TODO: verify implementation
            return type != null
                       ? type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
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