using System;
using System.Threading;

namespace ClassLibrary3
{
    public class Class1
    {
        //private Logger logger;
        public Int32 testint;

        //public Class1()
        //{
        //    logger = new Logger("F:\\LogFile.txt");
        //}

        public void DoSomeJob()
        {
            Console.WriteLine("Doing some job for a few seconds...");
            Thread.Sleep(3000);
        }

        public void DoSomeOtherJob()
        {
            Console.WriteLine("Doing some other job for a few seconds...");
            Thread.Sleep(3000);
        }

        public void DoCalculations()
        {
            Console.WriteLine("Doing calculation for a few seconds...");
            Thread.Sleep(3000);
        }

        public void DoFancyWork()
        {
            Console.WriteLine("Doing fancy work for a few seconds...");
            Thread.Sleep(3000);
        }

    }
}
