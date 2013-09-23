using NotifyAutoImplementer.Core;

namespace NotifyAutoImplementer.TestCore
{
    [NotifyAllProperty(ThrowOnNoVirtual = true)]
    public class TestModel : NotifyPropertyObject
    {
        [NotifyProperty("Debug")]
        public virtual string Name { get; set; }

        [NotifyProperty("Debug")]
        public virtual int Age { get; set; }

        [SuppressNotify]
        public string Debug
        {
            get { return string.Format("Name={0}; Age={1};", Name, Age); }
        }
    }
}