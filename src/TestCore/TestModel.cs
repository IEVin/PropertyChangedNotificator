using IEVin.NotifyAutoImplementer.Core;

namespace IEVin.NotifyAutoImplementer.TestCore
{
    [NotifyAllVirtualProperty]
    public class TestModel : NotifyPropertyObject
    {
        [NotifyProperty("Debug")]
        public virtual string Name { get; set; }

        [NotifyProperty("Debug")]
        public virtual int Age { get; set; }

        [SuppressNotify]
        public virtual bool IsEmpty { get; set; }

        public string Debug
        {
            get { return string.Format("Name={0}; Age={1};", Name, Age); }
        }
    }
}