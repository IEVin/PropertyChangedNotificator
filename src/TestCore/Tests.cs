using System;
using System.ComponentModel;
using System.Linq.Expressions;
using IEVin.NotifyAutoImplementer.Core;
using NUnit.Framework;

namespace IEVin.NotifyAutoImplementer.TestCore
{
    [TestFixture]
    public class Tests
    {
        [TestFixtureSetUp]
        public void Init()
        {
            // precompile test model
            NotifyImplementer.CreateInstance<TestModel>();
            NotifyImplementer.CreateInstance<TestModelBase>();
        }

        [Test]
        public void SimpleNotifyPropertyTest()
        {
            var model = NotifyImplementer.CreateInstance<TestModelBase>();

            var counter = 0;
            SetChangedCounter(model, (TestModelBase x) => x.NotifyProperty, () => counter++);

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
            var model = NotifyImplementer.CreateInstance<TestModelBase>();

            var counter = 0;
            SetChangedCounter(model, (TestModelBase x) => x.NotNotifyProperty, () => counter++);

            model.NotifyProperty = 1;

            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void VirtualNotNotifyPropertyTest()
        {
            var model = NotifyImplementer.CreateInstance<TestModelBase>();

            var counter = 0;
            SetChangedCounter(model, (TestModelBase x) => x.VirtualNotNotifyProperty, () => counter++);

            model.VirtualNotNotifyProperty = 1;

            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void NotVirtualNotifyPropertyTest()
        {
            var model = NotifyImplementer.CreateInstance<TestModelBase>();

            var counter = 0;
            SetChangedCounter(model, (TestModelBase x) => x.NotVirtualNotifyProperty, () => counter++);

            model.NotVirtualNotifyProperty = 1;

            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void DoubleNoifyPropertyTest()
        {
            var model = NotifyImplementer.CreateInstance<TestModelBase>();

            var doubleCounter = 0;
            var otherCounter = 0;
            SetChangedCounter(model, (TestModelBase x) => x.MultyNotifyProperty, () => doubleCounter++);
            SetChangedCounter(model, (TestModelBase x) => x.OtherNotifyProperty, () => otherCounter++);

            model.MultyNotifyProperty = 1;

            Assert.AreEqual(doubleCounter, 1);
            Assert.AreEqual(otherCounter, 1);
        }

        [Test]
        public void PrecisionDoubleTest()
        {
            var model = NotifyImplementer.CreateInstance<TestModelBase>();

            var counter = 0;
            SetChangedCounter(model, (TestModelBase x) => x.DoublePropertyForPrecisionTest, () => counter++);

            model.DoublePropertyForPrecisionTest = 1;
            Assert.AreEqual(counter, 1);

            // very small number
            model.DoublePropertyForPrecisionTest += 1e-17;
            Assert.AreEqual(counter, 1);

            model.DoublePropertyForPrecisionTest += 1e-14;
            Assert.AreEqual(counter, 2);
        }

        [Test]
        public void VirtualNotNotifyPropertyInDerivedClassTest()
        {
            var model = NotifyImplementer.CreateInstance<TestModel>();
            var counter = 0;
            SetChangedCounter(model, (TestModel x) => x.VirtualNotNotifyProperty, () => counter++);

            model.VirtualNotNotifyProperty = 1;
            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void SuppressNotifyPropertyTest()
        {
            var model = NotifyImplementer.CreateInstance<TestModel>();
            var counter = 0;
            SetChangedCounter(model, (TestModel x) => x.SuppressNotifyProperty, () => counter++);

            model.SuppressNotifyProperty = true;
            Assert.AreEqual(counter, 0);

            model.SuppressNotifyProperty = false;
            Assert.AreEqual(counter, 0);
        }

        [Test]
        public void StringNotifyProperty()
        {
            var model = NotifyImplementer.CreateInstance<TestModel>();

            var counter = 0;
            var comboCounter = 0;
            SetChangedCounter(model, (TestModel x) => x.StringNotifyProperty, () => counter++);
            SetChangedCounter(model, (TestModel x) => x.ComboProperty, () => comboCounter++);

            model.StringNotifyProperty = "1";
            Assert.AreEqual(counter, 1);
            Assert.AreEqual(comboCounter, 1);
        }

        static void SetChangedCounter<T, TValue>(INotifyPropertyChanged model, Expression<Func<T, TValue>> property, Action onChanged)
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