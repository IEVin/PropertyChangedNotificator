using System;
using NotifyAutoImplementer.Core;

namespace NotifyAutoImplementer.TestCore
{
    class Program
    {
        public static void Main(string[] args)
        {
            var model = DynamicBuilder.CreateInstanceProxy<TestModel>();

            var ageChangeCount = 0;
            var nameChangeCount = 0;

            model.PropertyChanged += (o, e) =>
                                         {
                                             if(e.PropertyName == "Age")
                                                 ageChangeCount++;
                                             if(e.PropertyName == "Name")
                                                 nameChangeCount++;
                                         };

            model.Age = 1;
            model.Name = "Test";
            model.Name = "Test";
            model.Name = "Test";
            model.Age++;

            Console.WriteLine("Age change count = {0} (must 2)", ageChangeCount);
            Console.WriteLine("Name change count = {0} (must 1)", nameChangeCount);
            Console.WriteLine("Name={0} (must Test); Age={1} (must 2);", model.Name, model.Age);
        }
    }
}