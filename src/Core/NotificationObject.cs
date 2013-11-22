using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace IEVin.NotifyAutoImplementer.Core
{
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotificationInvocator]
        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> expression)
        {
            var name = ((MemberExpression)expression.Body).Member.Name;
            RaisePropertyChanged(name);
        }
    }
}