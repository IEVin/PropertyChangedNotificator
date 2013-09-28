using IEVin.NotifyAutoImplementer.Core;

namespace IEVin.NotifyAutoImplementer.TestCore
{
    public class FailedTestModel : TestModelBase
    {
        [NotifyProperty]
        public int NotVirtualNotifyProperty { get; set; }
    }
}