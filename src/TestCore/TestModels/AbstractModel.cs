namespace IEVin.PropertyChangedNotificator.TestCore.TestModels
{
    public abstract class AbstractModel : NotificationObject
    {
        [NotifyProperty]
        public virtual string BaseProperty { get; set; }

        [NotifyProperty]
        public virtual string OverrideProperty { get; set; }
    }

    public class ChildAbstructModel : AbstractModel
    {
        public override string OverrideProperty { get; set; }
    }
}