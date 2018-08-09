using System;
using System.Reflection;

namespace ConsoleApp451
{
    class Program
    {


        static void Main(string[] args)
        {
            try
            {
                ////AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
                ////AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                //Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                //Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                ////Assembly assembly = Assembly.LoadFrom(@"C:\Users\Rodzinka\Documents\GitRepos\MonoCecilPlayground\ClassLibraryForFodyTesting\ClassLibrary3\bin\Debug\ClassLibrary3.dll");

                ClassLibrary3.Class1 class1 = new ClassLibrary3.Class1();
                foreach (var field in class1.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    Console.WriteLine(string.Format("{0} {1}", field.Name, field.FieldType));
                }


                //class1.DoCalculations();
                //class1.DoCalculations();
                //class1.DoSomeJob();
                //class1.DoSomeOtherJob();
                //class1.DoFancyWork();
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

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Unhalndedl");
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Console.WriteLine(args.LoadedAssembly.FullName);
        }
    }
}
