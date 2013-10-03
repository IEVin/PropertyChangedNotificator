using IEVin.NotifyAutoImplementer.Core;

namespace IEVin.NotifyAutoImplementer.TestCore.TestModels
{
    public class NotPublicModel : NotificationObject
    {
        [NotifyProperty]
        public virtual int ProtectedGetNotifyProperty { protected get; set; }

        [NotifyProperty]
        public virtual int ProtectedSetNotifyProperty { get; protected set; }

        [NotifyProperty]
        protected virtual int ProtectedNotifyProperty { get; set; }
    }
}