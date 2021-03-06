﻿using System.ComponentModel;

namespace IEVin.PropertyChangedNotificator.TestCore.TestModels
{
    public class ModelWithCorrectInvocator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        [NotificationInvocator]
        protected void OnPropertyChanged(string prop)
        {
        }

        public ModelWithCorrectInvocator()
        {
            Notificator.Create(this);
        }
    }
}