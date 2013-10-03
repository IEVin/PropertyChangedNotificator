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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class NotificationInvocatorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class SetPrecisionAttribute : Attribute
    {
        readonly double _precision;

        public double Precision
        {
            get { return _precision; }
        }

        public SetPrecisionAttribute(double precision)
        {
            _precision = precision;
        }
    }
}