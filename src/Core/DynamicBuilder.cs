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

        public static T CreateInstanceProxy<T>()
            where T : class, INotifyPropertyChanged, new()
        {
            var type = CreateProxyType(typeof(T));
            return (T)Activator.CreateInstance(type);
        }

        static Type CreateProxyType(Type type)
        {
            var tb = s_builder.Value.DefineType(type.Name + "_wrap", type.Attributes, type);

            tb.AddInterfaceImplementation(typeof(INotifyPropertyChanged));

            var properties = GetPropertyNames(type);

            foreach(var q in properties)
            {
                //var gettet = q.GetGetMethod(true);
                var setter = q.GetSetMethod(false);
                if(setter == null || !setter.IsVirtual)
                    continue;

                var prop = tb.DefineProperty(q.Name, q.Attributes, q.PropertyType, Type.EmptyTypes);

                var newSetter = CreateSetMethod(tb, setter); //, gettet);

                tb.DefineMethodOverride(newSetter, setter);

                prop.SetSetMethod(newSetter);
            }

            return tb.CreateType();
        }

        static MethodBuilder CreateSetMethod(TypeBuilder tb, MethodInfo setMi) //, MethodInfo getMi)
        {
            var paramTypes = setMi.GetParameters()
                                  .Select(x => x.ParameterType)
                                  .ToArray();

            var mb = tb.DefineMethod(setMi.Name, setMi.Attributes, null, paramTypes);

            var il = mb.GetILGenerator();

            il.EmitWriteLine(setMi.Name);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, setMi);
            il.Emit(OpCodes.Ret);

            return mb;
        }

        static IEnumerable<PropertyInfo> GetPropertyNames(Type type)
        {
            // TODO: verify implementation
            if(type == null)
                return Enumerable.Empty<PropertyInfo>();

            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                       .Concat(GetPropertyNames(type.BaseType));
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