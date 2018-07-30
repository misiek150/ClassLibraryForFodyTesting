using System;

namespace ConsoleApp451
{
    class Program
    {
        static void Main(string[] args)
        {
            ClassLibrary3.Class1 class11 = new ClassLibrary3.Class1();
            class11.WriteSomething();
            class11.WriteSomething("Additional info here");
            Console.ReadLine();
        }
    }
}
