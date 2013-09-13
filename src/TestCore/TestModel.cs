using System.ComponentModel;

namespace NotifyAutoImplementer.TestCore
{
	public class TestModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public virtual string Name { get; set; }

		public virtual int Age { get; set; }
	}
}