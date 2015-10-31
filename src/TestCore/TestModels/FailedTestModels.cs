using System.ComponentModel;

namespace IEVin.PropertyChangedNotificator.TestCore.TestModels
{
    public class NotVirtualModel : NotificationObject
    {
        [NotifyProperty]
        public int NotifyProperty { get; set; }

        public NotVirtualModel()
        {
            Notificator.Create(this);
        }
    }

    public class InternalGetModel : NotificationObject
    {
        [NotifyProperty]
        public virtual int NotifyProperty { internal get; set; }

        public InternalGetModel()
        {
            Notificator.Create(this);
        }
    }

    public class PrivateGetModel : NotificationObject
    {
        [NotifyProperty]
        public virtual int NotifyProperty { private get; set; }

        public PrivateGetModel()
        {
            Notificator.Create(this);
        }
    }

    public class InternalSetModel: NotificationObject
    {
        [NotifyProperty]
        public virtual int NotifyProperty { get; internal set; }

        public InternalSetModel()
        {
            Notificator.Create(this);
        }
    }

    public class PrivateSetModel : NotificationObject
    {
        [NotifyProperty]
        public virtual int NotifyProperty { get; private set; }

        public PrivateSetModel()
        {
            Notificator.Create(this);
        }
    }

    public class ModelWithoutInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ModelWithoutInvocator()
        {
            Notificator.Create(this);
        }
    }

    public class ModelWithInvalideInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotificationInvocator]
        public void RaisePropertyChanged()
        {
        }

        public ModelWithInvalideInvocator()
        {
            Notificator.Create(this);
        }
    }

    public class ModelWithInvalideInvocator2 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotificationInvocator]
        public string RaisePropertyChanged(string str)
        {
            return null;
        }

        public ModelWithInvalideInvocator2()
        {
            Notificator.Create(this);
        }
    }

    public class ModelWithNotPublicInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotificationInvocator]
        internal void RaisePropertyChanged(string str)
        {
        }

        public ModelWithNotPublicInvocator()
        {
            Notificator.Create(this);
        }
    }

    public class ModelWithMultyInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotificationInvocator]
        public void RaisePropertyChanged(string str)
        {
        }

        [NotificationInvocator]
        public void OnPropertyChanged(string str)
        {
        }

        public ModelWithMultyInvocator()
        {
            Notificator.Create(this);
        }
    }
}