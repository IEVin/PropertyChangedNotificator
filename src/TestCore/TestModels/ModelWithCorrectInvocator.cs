using System.ComponentModel;
using IEVin.NotifyAutoImplementer.Core;

namespace IEVin.NotifyAutoImplementer.TestCore.TestModels
{
    public class ModelWithCorrectInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotificationInvocator]
        protected void OnPropertyChanged(string prop)
        {
        }
    }
}