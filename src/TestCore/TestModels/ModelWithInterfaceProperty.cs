using System.Collections.Generic;

namespace IEVin.PropertyChangedNotificator.TestCore.TestModels
{
    public class ModelWithInterfaceProperty : NotificationObject
    {
        [NotifyProperty]
        public virtual IEnumerable<int> Test { get; set; }
    }
}
