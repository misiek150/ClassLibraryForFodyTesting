using System;

namespace ConsoleApp451
{
    class Program
    {
        static void Main(string[] args)
        {
            ClassLibrary3.Class1 class1 = new ClassLibrary3.Class1();
            class1.DoCalculations();
            class1.DoSomeJob();
            class1.DoSomeOtherJob();
            class1.DoFancyWork();
            Console.ReadLine();
        }
    }
}
