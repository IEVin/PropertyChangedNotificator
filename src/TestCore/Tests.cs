using System;
using System.ComponentModel;
using System.Linq.Expressions;
using IEVin.NotifyAutoImplementer.Core;
using IEVin.NotifyAutoImplementer.TestCore.TestModels;
using NUnit.Framework;

namespace IEVin.NotifyAutoImplementer.TestCore
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void SimpleNotifyPropertyTest()
        {
            var model = Notificator.Of<TestModelBase>();

            var counter = 0;
            SetChangedAction(model, (TestModelBase x) => x.NotifyProperty, () => counter++);

            // modify
            model.NotifyProperty = 1;
            Assert.AreEqual(counter, 1);

            // modify
            model.NotifyProperty += 2;
            Assert.AreEqual(counter, 2);

            // not modify
            model.NotifyProperty = 3;
            Assert.AreEqual(counter, 2);

            // modify other
            model.OtherNotifyProperty = 100;
            Assert.AreEqual(counter, 2);

            Assert.AreEqual(model.NotifyProperty, 3);
            Assert.AreEqual(model.OtherNotifyProperty, 100);
        }

        [Test]
        public void NotNotifyPropertyTest()
        {
            var model = Notificator.Of<TestModelBase>();

            var counter = 0;
            SetChangedAction(model, (TestModelBase x) => x.NotNotifyProperty, () => counter++);

            model.NotifyProperty = 1;

            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void VirtualNotNotifyPropertyTest()
        {
            var model = Notificator.Of<TestModelBase>();

            var counter = 0;
            SetChangedAction(model, (TestModelBase x) => x.VirtualNotNotifyProperty, () => counter++);

            model.VirtualNotNotifyProperty = 1;

            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void FailedModelTest()
        {
            Assert.Throws(typeof(InvalidOperationException), () => Notificator.Of<NotVirtualModel>());
            Assert.Throws(typeof(InvalidOperationException), () => Notificator.Of<InternalGetModel>());
            Assert.Throws(typeof(InvalidOperationException), () => Notificator.Of<InternalSetModel>());
            Assert.Throws(typeof(InvalidOperationException), () => Notificator.Of<PrivateGetModel>());
            Assert.Throws(typeof(InvalidOperationException), () => Notificator.Of<PrivateSetModel>());

            Assert.DoesNotThrow(() => Notificator.Of<NotPublicModel>());
        }

        [Test]
        public void FailedInvocatorTest()
        {
            Assert.Throws(typeof(InvalidOperationException), () => Notificator.Of<ModelWithoutInvocator>());
            Assert.Throws(typeof(InvalidOperationException), () => Notificator.Of<ModelWithInvalideInvocator>());
            Assert.Throws(typeof(InvalidOperationException), () => Notificator.Of<ModelWithInvalideInvocator2>());
            Assert.Throws(typeof(InvalidOperationException), () => Notificator.Of<ModelWithNotPublicInvocator>());
            Assert.Throws(typeof(InvalidOperationException), () => Notificator.Of<ModelWithMultyInvocator>());
            Assert.Throws(typeof(ArgumentException), () => Notificator.Of(typeof(ModelWithAbstractInvocator)));

            Assert.DoesNotThrow(() => Notificator.Of<ModelWithCorrectInvocator>());
        }

        [Test]
        public void FailedTypeTest()
        {
            Assert.Throws(typeof(ArgumentNullException), () => Notificator.Of(null));
            Assert.Throws(typeof(ArgumentException), () => Notificator.Of(typeof(object)));
        }

        [Test]
        public void MultyNoifyPropertyTest()
        {
            var model = Notificator.Of<TestModelBase>();

            var doubleCounter = 0;
            var otherCounter = 0;
            SetChangedAction(model, (TestModelBase x) => x.MultyNotifyProperty, () => doubleCounter++);
            SetChangedAction(model, (TestModelBase x) => x.OtherNotifyProperty, () => otherCounter++);

            model.MultyNotifyProperty = 1;

            Assert.AreEqual(doubleCounter, 1);
            Assert.AreEqual(otherCounter, 1);
        }

        [Test]
        public void DefaultPrecisionDoubleNoifyPropertyTest()
        {
            var model = Notificator.Of<TestModelBase>();

            var counter = 0;
            SetChangedAction(model, (TestModelBase x) => x.DoubleNotifyProperty, () => counter++);

            model.DoubleNotifyProperty = 1;
            Assert.AreEqual(counter, 1);

            // very small number
            model.DoubleNotifyProperty += 1e-17;
            Assert.AreEqual(counter, 1);

            model.DoubleNotifyProperty += 1e-14;
            Assert.AreEqual(counter, 2);
        }

        [Test]
        public void DefaultPrecisionFloatNoifyPropertyTest()
        {
            var model = Notificator.Of<TestModelBase>();

            var counter = 0;
            SetChangedAction(model, (TestModelBase x) => x.FloatNotifyProperty, () => counter++);

            model.FloatNotifyProperty = 1;
            Assert.AreEqual(counter, 1);

            // very small number
            model.FloatNotifyProperty += 1e-9f;
            Assert.AreEqual(counter, 1);

            model.FloatNotifyProperty += 1e-7f;
            Assert.AreEqual(counter, 2);
        }

        [Test]
        public void VirtualNotNotifyPropertyInDerivedClassTest()
        {
            var model = Notificator.Of<TestModel>();
            var counter = 0;
            SetChangedAction(model, (TestModel x) => x.VirtualNotNotifyProperty, () => counter++);

            model.VirtualNotNotifyProperty = 1;
            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void StringNotifyProperty()
        {
            var model = Notificator.Of<TestModel>();

            var counter = 0;
            var comboCounter = 0;
            SetChangedAction(model, (TestModel x) => x.StringNotifyProperty, () => counter++);
            SetChangedAction(model, (TestModel x) => x.ComboProperty, () => comboCounter++);

            model.StringNotifyProperty = "Test";
            Assert.AreEqual(counter, 1);
            Assert.AreEqual(comboCounter, 1);
        }

        [Test]
        public void PrecisionDoubleNotifyPropertyTest()
        {
            var model = Notificator.Of<TestModel>();

            var counter = 0;
            SetChangedAction(model, (TestModel x) => x.DoublePrecisionNotifyProperty, () => counter++);

            model.DoublePrecisionNotifyProperty = 1;
            Assert.AreEqual(counter, 1);

            model.DoublePrecisionNotifyProperty += 1e-10;
            Assert.AreEqual(counter, 2);

            model.DoublePrecisionNotifyProperty += 1e-11;
            Assert.AreEqual(counter, 2);

            model.DoublePrecisionNotifyProperty += 1e-9;
            Assert.AreEqual(counter, 3);
        }

        [Test]
        public void PrecisionFloatNotifyPropertyTest()
        {
            var model = Notificator.Of<TestModel>();

            var counter = 0;
            SetChangedAction(model, (TestModel x) => x.FloatPrecisionNotifyProperty, () => counter++);

            model.FloatPrecisionNotifyProperty = 1f;
            Assert.AreEqual(counter, 1);

            model.FloatPrecisionNotifyProperty += 1e-4f;
            Assert.AreEqual(counter, 1);

            model.FloatPrecisionNotifyProperty += 1e-3f;
            Assert.AreEqual(counter, 2);

            model.FloatPrecisionNotifyProperty = 1f;
            Assert.AreEqual(counter, 3);
        }

        [Test]
        public void PrecisionDecimalNotifyPropertyTest()
        {
            var model = Notificator.Of<TestModel>();

            var counter = 0;
            SetChangedAction(model, (TestModel x) => x.DecimalPrecisionNotifyProperty, () => counter++);

            model.DecimalPrecisionNotifyProperty = 2;
            Assert.AreEqual(counter, 1);

            model.DecimalPrecisionNotifyProperty += 1e-24m;
            Assert.AreEqual(counter, 2);

            model.DecimalPrecisionNotifyProperty += 1e-26m;
            Assert.AreEqual(counter, 2);

            model.DecimalPrecisionNotifyProperty += 1e-25m;
            Assert.AreEqual(counter, 3);
        }

        [Test]
        public void RefletionTest()
        {
            var model = Notificator.Of<TestModelBase>();

            var counter = 0;
            SetChangedAction(model, (TestModel x) => x.NotifyProperty, () => counter++);

            Assert.DoesNotThrow(() =>
                                    {
                                        var prop = model.GetType().GetProperty("NotifyProperty");
                                        prop.SetValue(model, 1, null);
                                    });

            Assert.AreEqual(counter, 1);
        }

        static void SetChangedAction<T, TValue>(INotifyPropertyChanged model, Expression<Func<T, TValue>> property, Action onChanged)
        {
            var name = ((MemberExpression)property.Body).Member.Name;

            model.PropertyChanged += (o, e) =>
                                         {
                                             if(e.PropertyName == name)
                                                 onChanged();
                                         };
        }
    }
}