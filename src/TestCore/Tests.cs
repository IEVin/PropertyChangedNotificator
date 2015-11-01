using System;
using System.ComponentModel;
using System.Linq.Expressions;
using IEVin.PropertyChangedNotificator.TestCore.TestModels;
using NUnit.Framework;

namespace IEVin.PropertyChangedNotificator.TestCore
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void SimpleNotifyPropertyTest()
        {
            var model = new TestModelBase();

            var counter = 0;
            model.SetChangedAction(x => x.NotifyProperty, () => counter++);

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
            var model = new TestModelBase();

            var counter = 0;
            model.SetChangedAction(x => x.NotNotifyProperty, () => counter++);

            model.NotifyProperty = 1;

            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void VirtualNotNotifyPropertyTest()
        {
            var model = new TestModelBase();

            var counter = 0;
            model.SetChangedAction(x => x.VirtualNotNotifyProperty, () => counter++);

            model.VirtualNotNotifyProperty = 1;

            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void FailedModelTest()
        {
            // ReSharper disable ObjectCreationAsStatement
            Assert.Throws(typeof(InvalidOperationException), () => new NotVirtualModel());
            Assert.Throws(typeof(InvalidOperationException), () => new InternalGetModel());
            Assert.Throws(typeof(InvalidOperationException), () => new InternalSetModel());
            Assert.Throws(typeof(InvalidOperationException), () => new PrivateGetModel());
            Assert.Throws(typeof(InvalidOperationException), () => new PrivateSetModel());

            Assert.DoesNotThrow(() => new ModelWithInterfaceProperty());
            Assert.DoesNotThrow(() => new NotPublicModel());
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void FailedInvocatorTest()
        {
            // ReSharper disable ObjectCreationAsStatement
            Assert.Throws(typeof(InvalidOperationException), () => new ModelWithoutInvocator());
            Assert.Throws(typeof(InvalidOperationException), () => new ModelWithInvalideInvocator());
            Assert.Throws(typeof(InvalidOperationException), () => new ModelWithInvalideInvocator2());
            Assert.Throws(typeof(InvalidOperationException), () => new ModelWithNotPublicInvocator());
            Assert.Throws(typeof(InvalidOperationException), () => new ModelWithMultyInvocator());

            Assert.DoesNotThrow(() => new ModelWithCorrectInvocator());
            // ReSharper restore ObjectCreationAsStatement
        }

        [Test]
        public void FailedTypeTest()
        {
            Assert.Throws(typeof(ArgumentNullException), () => Notificator.Create<INotifyPropertyChanged>(null));
        }

        [Test]
        public void MultyNoifyPropertyTest()
        {
            var model = new TestModelBase();

            var doubleCounter = 0;
            var otherCounter = 0;
            model.SetChangedAction(x => x.MultyNotifyProperty, () => doubleCounter++);
            model.SetChangedAction(x => x.OtherNotifyProperty, () => otherCounter++);

            model.MultyNotifyProperty = 1;

            Assert.AreEqual(doubleCounter, 1);
            Assert.AreEqual(otherCounter, 1);
        }

        [Test]
        public void DefaultPrecisionDoubleNoifyPropertyTest()
        {
            var model = new TestModelBase();

            var counter = 0;
            model.SetChangedAction(x => x.DoubleNotifyProperty, () => counter++);

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
            var model = new TestModelBase();

            var counter = 0;
            model.SetChangedAction(x => x.FloatNotifyProperty, () => counter++);

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
            var model = new TestModel();
            var counter = 0;
            model.SetChangedAction(x => x.VirtualNotNotifyProperty, () => counter++);

            model.VirtualNotNotifyProperty = 1;
            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void StringNotifyPropertyTest()
        {
            var model = new TestModel();

            var counter = 0;
            var comboCounter = 0;
            model.SetChangedAction(x => x.StringNotifyProperty, () => counter++);
            model.SetChangedAction(x => x.ComboProperty, () => comboCounter++);

            model.StringNotifyProperty = "Test";
            Assert.AreEqual(counter, 1);
            Assert.AreEqual(comboCounter, 1);
        }

        [Test]
        public void PrecisionDoubleNotifyPropertyTest()
        {
            var model = new TestModel();

            var counter = 0;
            model.SetChangedAction(x => x.DoublePrecisionNotifyProperty, () => counter++);

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
            var model = new TestModel();

            var counter = 0;
            model.SetChangedAction(x => x.FloatPrecisionNotifyProperty, () => counter++);

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
            var model = new TestModel();

            var counter = 0;
            model.SetChangedAction(x => x.DecimalPrecisionNotifyProperty, () => counter++);

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
        public void AbstructChildTest()
        {
            var model = new ChildAbstructModel();

            var overrideCounter = 0;
            var baseCounter = 0;
            model.SetChangedAction(x => x.OverrideProperty, () => overrideCounter++);
            model.SetChangedAction(x => x.BaseProperty, () => baseCounter++);

            model.OverrideProperty = "OverrideProperty";
            Assert.AreEqual(overrideCounter, 1);

            model.BaseProperty = "BaseProperty";
            Assert.AreEqual(baseCounter, 1);
        }

        [Test]
        public void ConstructorOfTest()
        {
            Func<TestModelBase> ctor = () => new TestModelBase();

            var model1 = ctor();
            var model2 = ctor();

            Assert.NotNull(model1);
            Assert.NotNull(model2);

            Assert.AreNotEqual(model1, model2);
            Assert.AreEqual(model1.GetType(), model2.GetType());

            var counter1 = 0;
            var counter2 = 0;
            model1.SetChangedAction(x => x.NotifyProperty, () => counter1++);
            model2.SetChangedAction(x => x.NotifyProperty, () => counter2++);

            model1.NotifyProperty = 1;
            Assert.AreEqual(counter1, 1);
            Assert.AreEqual(counter2, 0);

            model2.NotifyProperty = 1;
            model2.NotifyProperty = 2;
            Assert.AreEqual(counter1, 1);
            Assert.AreEqual(counter2, 2);
        }

        [Test]
        public void RefletionTest()
        {
            var model = new TestModelBase();

            var counter = 0;
            model.SetChangedAction(x => x.NotifyProperty, () => counter++);

            Assert.DoesNotThrow(() =>
                                    {
                                        var prop = model.GetType().GetProperty("NotifyProperty");
                                        prop.SetValue(model, 1, null);
                                    });

            Assert.AreEqual(counter, 1);
        }

        [Test]
        public void MemoryLeaksTest()
        {
            var weak = CreateWeakRef();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(weak.IsAlive);
        }

        static WeakReference CreateWeakRef()
        {
            var model = new TestModelBase();

            var counter = 0;
            model.SetChangedAction(x => x.NotifyProperty, () => counter++);

            model.NotifyProperty = 1;
            Assert.AreEqual(counter, 1);

            return new WeakReference(model);
        }
    }

    static class TestsExt
    {
        public static void SetChangedAction<T, TValue>(this T model, Expression<Func<T, TValue>> property, Action onChanged)
            where T : INotifyPropertyChanged
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