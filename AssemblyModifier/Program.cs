using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyModifier
{
    class Program
    {
        static void Main(string[] args)
        {
            Tools tools = new Tools();
            string FileName = @"C:\Users\Rodzinka\source\repos\ClassLibraryForFodyTesting\ClassLibrary3\bin\Debug\ClassLibrary3.dll";
            //tools.PrintTypes(FileName);

            tools.ModifyAssembly(FileName);

            //Console.ReadLine();
        }
    }

    public class Tools
    {
        public void PrintTypes(string fileName)
        {
            ModuleDefinition module = ModuleDefinition.ReadModule(fileName);
            foreach (TypeDefinition type in module.Types)
            {
                if (!type.IsPublic)
                    continue;

                Console.WriteLine(type.FullName);

                foreach (MethodDefinition method in type.Methods.ToArray())
                {
                    Console.WriteLine(" ---> " + method.Name);
                    foreach (ParameterDefinition param in method.Parameters)
                    {
                        Console.WriteLine("    ---> " + param.Name + " " + param.ParameterType + (param.IsReturnValue ? "(return type)" : ""));
                    }
                }
            }
        }

        public void ModifyAssembly(string fileName)
        {
            MethodInfo writeLineMethod = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });

            // ReaderParameters { ReadWrite = true } is necessary to later write the file
            using (ModuleDefinition module = ModuleDefinition.ReadModule(fileName, new ReaderParameters { ReadWrite = true }))
            {
                // Modify the assembly
                TypeDefinition[] types = module.Types.ToArray();

                MethodReference methodReference = module.ImportReference(writeLineMethod);

                foreach (var type in types)
                {
                    foreach (MethodDefinition methodToChange in type.Methods)
                    {
                        string sentence = String.Concat("Code added in ", methodToChange.Name);
                        Mono.Cecil.Cil.ILProcessor ilProcessor = methodToChange.Body.GetILProcessor();

                        Mono.Cecil.Cil.Instruction loadStringInstruction = ilProcessor.Create(OpCodes.Ldstr, sentence);
                        Mono.Cecil.Cil.Instruction callInstruction = ilProcessor.Create(OpCodes.Call, methodReference);

                        Mono.Cecil.Cil.Instruction methodFirstInstruction = methodToChange.Body.Instructions[0];

                        ilProcessor.InsertBefore(methodFirstInstruction, loadStringInstruction);
                        ilProcessor.InsertAfter(loadStringInstruction, callInstruction);
                    }
                }

                module.Write(); // Write to the same file that was used to open the file
            }

        }
    }
}
