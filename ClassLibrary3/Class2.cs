using System;
using System.Threading;

namespace ClassLibrary3
{
    public class Class2
    {
        public void DoSomeJobInClass2()
        {
            Console.WriteLine("Doing some job for a few seconds...");
            Thread.Sleep(3000);
        }
    }
}
