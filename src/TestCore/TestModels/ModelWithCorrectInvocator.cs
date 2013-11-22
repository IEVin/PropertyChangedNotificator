using System.ComponentModel;

namespace IEVin.PropertyChangedNotificator.TestCore.TestModels
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