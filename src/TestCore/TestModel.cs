using IEVin.NotifyAutoImplementer.Core;

namespace IEVin.NotifyAutoImplementer.TestCore
{
    [NotifyAllVirtualProperty]
    public class TestModel : TestModelBase
    {
        [NotifyProperty("ComboProperty")]
        public virtual string StringNotifyProperty { get; set; }

        [SuppressNotify]
        public virtual bool SuppressNotifyProperty { get; set; }

        public string ComboProperty
        {
            get { return string.Format("StringNotifyProperty={0};", StringNotifyProperty); }
        }
    }
}