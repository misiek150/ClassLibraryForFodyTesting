using CommonComponents;
using System;
using System.Threading;

namespace ClassLibrary3
{
    public class Class1
    {
        private int threadSleepTime = 2000;

        [LogMethodAttribute]
        public void DoSomeJob()
        {
            Console.WriteLine("Doing some job for a few seconds...");
            Thread.Sleep(threadSleepTime);
        }

        public void DoSomeJob2()
        {
            Console.WriteLine("Doing some job for a few seconds...");
            Thread.Sleep(threadSleepTime);
        }

        public void DoSomeOtherJob(bool switchCase)
        {
            if (switchCase)
            {
                Console.WriteLine("TRUE - Doing some other job for a few seconds...");
                Thread.Sleep(threadSleepTime);
            }
            else
            {
                Console.WriteLine("FALSE - Doing some other job for a few seconds...");
                Thread.Sleep(threadSleepTime);
            }

        }

        [LogMethodAttribute]
        public void DoCalculations()
        {
            Console.WriteLine("Doing calculation for a few seconds...");
            Thread.Sleep(threadSleepTime);
        }

        public int AddIntegers(int A, int B, bool? adding)
        {
            Thread.Sleep(threadSleepTime);
            if (adding.HasValue)
            {
                if (adding.Value)
                {
                    return A + B;
                }
                if (!adding.Value)
                {
                    return A - B;
                }
            }
            return A;
        }

        public void DoFancyWork()
        {
            Console.WriteLine("Doing fancy work for a few seconds...");
            Thread.Sleep(threadSleepTime);
        }

        public void DoAllJobs()
        {
            DoSomeJob();
            DoSomeOtherJob(true);
            DoSomeOtherJob(false);
            DoFancyWork();
        }

    }
}
