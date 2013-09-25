using System.ComponentModel;

namespace IEVin.NotifyAutoImplementer.Core
{
    public class NotifyPropertyObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}