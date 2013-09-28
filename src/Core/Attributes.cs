using System;

namespace IEVin.NotifyAutoImplementer.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class NotifyPropertyAttribute : Attribute
    {
        readonly string _propertyName;

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public NotifyPropertyAttribute()
        {
        }

        public NotifyPropertyAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }
    }
}