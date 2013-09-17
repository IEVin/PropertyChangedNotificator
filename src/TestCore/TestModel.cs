using NotifyAutoImplementer.Core;

namespace NotifyAutoImplementer.TestCore
{
    public class TestModel : NotifyPropertyObject
    {
        public virtual string Name { get; set; }

        public virtual int Age { get; set; }
    }
}