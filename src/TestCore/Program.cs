using System;
using IEVin.NotifyAutoImplementer.Core;

namespace IEVin.NotifyAutoImplementer.TestCore
{
    class Program
    {
        public static void Main(string[] args)
        {
            var model = NotifyImplementer.CreateInstance<TestModel>();
            NotifyImplementer.CreateInstance(typeof(TestModel));

            var ageChangeCount = 0;
            var nameChangeCount = 0;
            var debugChangeCount = 0;
            var emptyChangeCount = 0;

            model.PropertyChanged += (o, e) =>
                                         {
                                             if(e.PropertyName == "Age")
                                                 ageChangeCount++;
                                             if(e.PropertyName == "Name")
                                                 nameChangeCount++;
                                             if(e.PropertyName == "Debug")
                                                 debugChangeCount++;
                                             if(e.PropertyName == "IsEmpty")
                                                 emptyChangeCount++;
                                         };

            model.Age = 1;
            model.Age += 1e-16; // no change

            model.Name = "Test";
            model.Name = "Test";
            model.Name = "Test";

            model.Age++;


            model.IsEmpty = true;
            model.IsEmpty = true;
            model.IsEmpty = false;

            Console.WriteLine("Age change count = {0} (must 2)", ageChangeCount);
            Console.WriteLine("Name change count = {0} (must 1)", nameChangeCount);
            Console.WriteLine("Debug change count = {0} (must 3)", debugChangeCount);
            Console.WriteLine("IsEmpty change count = {0} (must 0)", emptyChangeCount);
            Console.WriteLine("Name={0} (must Test); Age={1} (must 2);", model.Name, model.Age);
            Console.WriteLine("Debug = '{0}'", model.Debug);
        }
    }
}