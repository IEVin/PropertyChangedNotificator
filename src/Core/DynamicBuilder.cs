using System;

namespace NotifyAutoImplementer.Core
{
	public static class DynamicBuilder
	{
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