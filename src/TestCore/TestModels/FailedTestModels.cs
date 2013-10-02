using System.ComponentModel;
using IEVin.NotifyAutoImplementer.Core;

namespace IEVin.NotifyAutoImplementer.TestCore.TestModels
{
    public class NotVirtualModel : NotificationObject
    {
        [NotifyProperty]
        public int NotifyProperty { get; set; }
    }

    public class InternalGetModel : NotificationObject
    {
        [NotifyProperty]
        public virtual int NotifyProperty { internal get; set; }
    }

    public class PrivateGetModel : NotificationObject
    {
        [NotifyProperty]
        public virtual int NotifyProperty { private get; set; }
    }

    public class InternalSetModel: NotificationObject
    {
        [NotifyProperty]
        public virtual int NotifyProperty { get; internal set; }
    }

    public class PrivateSetModel : NotificationObject
    {
        [NotifyProperty]
        public virtual int NotifyProperty { get; private set; }
    }

    public class ModelWithoutInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ModelWithInvalideInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyInvocator]
        public void RaisePropertyChanged()
        {
        }
    }

    public class ModelWithInvalideInvocator2 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyInvocator]
        public string RaisePropertyChanged(string str)
        {
            return null;
        }
    }

    public class ModelWithNotPublicInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyInvocator]
        internal void RaisePropertyChanged(string str)
        {
        }
    }

    public class ModelWithMultyInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyInvocator]
        public void RaisePropertyChanged(string str)
        {
        }

        [NotifyInvocator]
        public void OnPropertyChanged(string str)
        {
        }
    }

    public abstract class ModelWithAbstractInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyInvocator]
        protected void OnPropertyChanged(string propertyName)
        {
        }
    }
}