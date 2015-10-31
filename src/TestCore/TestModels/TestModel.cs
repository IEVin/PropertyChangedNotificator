namespace IEVin.PropertyChangedNotificator.TestCore.TestModels
{
    public class TestModel : TestModelBase
    {
        [NotifyProperty]
        [NotifyProperty("ComboProperty")]
        public virtual string StringNotifyProperty { get; set; }

        [NotifyProperty]
        [SetPrecision(1E-10)]
        public virtual double DoublePrecisionNotifyProperty { get; set; }

        [NotifyProperty]
        [SetPrecision(1E-3)]
        public virtual float FloatPrecisionNotifyProperty { get; set; }

        [NotifyProperty]
        [SetPrecision(1E-25)]
        public virtual decimal DecimalPrecisionNotifyProperty { get; set; }

        public string ComboProperty
        {
            get { return string.Format("StringNotifyProperty={0};", StringNotifyProperty); }
        }

        public TestModel()
        {
            Notificator.Create(this);
        }
    }
}