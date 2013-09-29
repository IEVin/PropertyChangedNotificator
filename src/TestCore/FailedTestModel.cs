using IEVin.NotifyAutoImplementer.Core;

namespace IEVin.NotifyAutoImplementer.TestCore
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
}