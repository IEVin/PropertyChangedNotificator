using IEVin.NotifyAutoImplementer.Core;

namespace IEVin.NotifyAutoImplementer.TestCore.TestModels
{
    public class TestModel : TestModelBase
    {
        [NotifyProperty]
        [NotifyProperty("ComboProperty")]
        public virtual string StringNotifyProperty { get; set; }

        public string ComboProperty
        {
            get { return string.Format("StringNotifyProperty={0};", StringNotifyProperty); }
        }
    }
}