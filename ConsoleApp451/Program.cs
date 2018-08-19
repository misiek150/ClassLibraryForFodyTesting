﻿using System;
using System.Reflection;

namespace ConsoleApp451
{
    class Program
    {


        static void Main(string[] args)
        {
            try
            {

                ClassLibrary3.Class1 class1 = new ClassLibrary3.Class1();
                foreach (var field in class1.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    Console.WriteLine(string.Format("{0} {1}", field.Name, field.FieldType));
                }

                class1.DoCalculations();
                class1.DoSomeJob();
                class1.DoSomeOtherJob();
                class1.DoFancyWork();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("DONE! Press Enter");
                Console.ReadLine();
            }



        }

    }
}
