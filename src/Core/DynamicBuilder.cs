using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;

namespace NotifyAutoImplementer.Core
{
    public static class DynamicBuilder
    {
        static readonly Lazy<ModuleBuilder> s_builder = new Lazy<ModuleBuilder>(CreateModule);

        static readonly Dictionary<Guid, Type> s_cache = new Dictionary<Guid, Type>();

        public static T CreateInstanceProxy<T>()
            where T : NotifyPropertyObject, new()
        {
            var guid = typeof(T).GUID;

            //TODO: Add sync
            Type type;
            if(!s_cache.TryGetValue(guid, out type))
            {
                type = CreateProxyType(typeof(T));
                s_cache[guid] = type;
            }

            //TODO: Add dynamic method
            return (T)Activator.CreateInstance(type);
        }

        static Type CreateProxyType(Type type)
        {
            var tb = s_builder.Value.DefineType(type.Name + "_NotifyImplementation", type.Attributes, type);

            tb.AddInterfaceImplementation(typeof(INotifyPropertyChanged));

            var properties = GetPropertyNames(type);

            foreach(var q in properties)
            {
                var gettet = q.GetGetMethod(true);
                var setter = q.GetSetMethod(false);
                if(setter == null || !setter.IsVirtual)
                    continue;

                var prop = tb.DefineProperty(q.Name, q.Attributes, q.PropertyType, Type.EmptyTypes);
                var newSetter = CreateSetMethod(tb, q.Name, gettet, setter);

                tb.DefineMethodOverride(newSetter, setter);
                prop.SetSetMethod(newSetter);
            }

            return tb.CreateType();
        }

        static MethodBuilder CreateSetMethod(TypeBuilder tb, string name, MethodInfo getMi, MethodInfo setMi)
        {
            var paramTypes = setMi.GetParameters()
                                  .Select(x => x.ParameterType)
                                  .ToArray();

            var mb = tb.DefineMethod(setMi.Name, setMi.Attributes, null, paramTypes);

            var equals = DynamicHelper.GetEquals(getMi.ReturnType);
            var raise = DynamicHelper.GetRaise();

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

            // raise OnPropertyChanged event
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, name);
            il.Emit(OpCodes.Call, raise);

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
            var assemblyName = new AssemblyName(string.Format("DA_{0}", Guid.NewGuid().ToString()));

            return AppDomain.CurrentDomain
                            .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
                            .DefineDynamicModule(assemblyName.Name);
        }
    }
}