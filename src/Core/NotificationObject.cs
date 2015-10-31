using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace IEVin.PropertyChangedNotificator
{
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        [DebuggerStepThrough]
        [NotificationInvocator]
        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        [DebuggerStepThrough]
        protected void RaisePropertyChanged<T>(Expression<Func<T>> expression)
        {
            var name = ((MemberExpression)expression.Body).Member.Name;
            RaisePropertyChanged(name);
        }
    }
}