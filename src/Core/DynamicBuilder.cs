using System;
using System.Reflection;
using System.Reflection.Emit;

namespace NotifyAutoImplementer.Core
{
	public static class DynamicBuilder
	{
		const string AssemblyBuilderName = "DynamicAssembly_871";

		static Lazy<AssemblyBuilder> s_assemblyBuilder = new Lazy<AssemblyBuilder>(
			() => AppDomain.CurrentDomain.DefineDynamicAssembly( new AssemblyName( AssemblyBuilderName ), AssemblyBuilderAccess.Run ) );

		public static T CreateInstanceProxy<T>()
		{
			return (T)CreateProxy( typeof(T) );
		}

		static object CreateProxy( Type type )
		{
			// stub
			return Activator.CreateInstance( type );
		}
	}
}