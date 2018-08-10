using BasicLogger;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AssemblyModifier
{
    class Program
    {
        static void Main(string[] args)
        {
            Tools tools = new Tools();
            string path = @"C:\Users\Rodzinka\Documents\GitRepos\MonoCecilPlayground\ClassLibraryForFodyTesting\ConsoleApp451\bin\Debug";
            const string fileName = "ClassLibrary3.dll";
            var filesToModify = Directory.GetFiles(path, fileName, SearchOption.AllDirectories);
            foreach (string file in filesToModify)
            {
                Console.WriteLine(file);
                tools.AddLogger(file, "F:\\LogFile.txt");

                Process peVerifyProcess = new Process();
                peVerifyProcess.StartInfo.FileName = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\PEVerify.exe";
                peVerifyProcess.StartInfo.Arguments = file;
                peVerifyProcess.StartInfo.RedirectStandardOutput = true;
                peVerifyProcess.StartInfo.UseShellExecute = false;
                peVerifyProcess.Start();
                Console.WriteLine(peVerifyProcess.StandardOutput.ReadToEnd());
            }
            Console.ReadLine();
        }
    }

    public class Tools
    {
        public void PrintTypes(string fileName)
        {
            using (ModuleDefinition module = ModuleDefinition.ReadModule(fileName, new ReaderParameters { ReadWrite = true }))
            {
                foreach (TypeDefinition type in module.Types)
                {
                    if (!type.IsPublic)
                        continue;

                    Console.WriteLine(type.FullName);

                    Console.WriteLine("    --- Methods ---");
                    foreach (MethodDefinition method in type.Methods.ToArray())
                    {
                        Console.WriteLine(method.Name);
                        foreach (ParameterDefinition param in method.Parameters)
                        {
                            Console.WriteLine("      ---> " + param.Name + " " + param.ParameterType + (param.IsReturnValue ? "(return type)" : ""));
                        }
                    }

                    Console.WriteLine("   --- Fields ---");
                    foreach (var field in type.Fields.ToArray())
                    {
                        Console.WriteLine(string.Format("      {0} [{1}]", field.Name, field.FieldType));
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

        public void AddLogger(string fileName, string logPath)
        {
            //MethodInfo writeLineMethod = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });

            // ReaderParameters { ReadWrite = true } is necessary to later write the file
            using (ModuleDefinition module = ModuleDefinition.ReadModule(fileName, new ReaderParameters { ReadWrite = true }))
            {
                TypeDefinition[] types = module.Types.ToArray();
                foreach (TypeDefinition type in types)
                {
                    if (type.Name != "Class1")
                    {
                        continue;
                    }

                    FieldDefinition item = new FieldDefinition("CecilLogger", Mono.Cecil.FieldAttributes.Private, module.ImportReference(typeof(Logger)));
                    type.Fields.Add(item);

                    ConstructorInfo loggerConstructor = typeof(Logger).GetConstructor(new Type[1] { typeof(string) });
                    MethodReference constructorReference = module.ImportReference(loggerConstructor);


                    //foreach (MethodDefinition methodToChange in type.Methods)
                    //{
                    //    string sentence = String.Concat("Code added in ", methodToChange.Name);
                    //    Mono.Cecil.Cil.ILProcessor ilProcessor = methodToChange.Body.GetILProcessor();

                    //    Mono.Cecil.Cil.Instruction loadStringInstruction = ilProcessor.Create(OpCodes.Ldstr, sentence);
                    //    Mono.Cecil.Cil.Instruction callInstruction = ilProcessor.Create(OpCodes.Call, methodReference);

                    //    Mono.Cecil.Cil.Instruction methodFirstInstruction = methodToChange.Body.Instructions[0];

                    //    ilProcessor.InsertBefore(methodFirstInstruction, loadStringInstruction);
                    //    ilProcessor.InsertAfter(loadStringInstruction, callInstruction);
                    //} 

                    foreach (MethodDefinition methodToChange in type.Methods)
                    {
                        if (methodToChange.IsConstructor)
                        {
                            ILProcessor ilProcessor = methodToChange.Body.GetILProcessor();

                            Instruction loadArgumentInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
                            Instruction loadStringInstruction = ilProcessor.Create(OpCodes.Ldstr, logPath);
                            Instruction createObjectInstruction = ilProcessor.Create(OpCodes.Call, constructorReference);

                            int instructionCount = methodToChange.Body.Instructions.Count;
                            var lastInstruction = methodToChange.Body.Instructions[instructionCount - 1];


                            ilProcessor.InsertBefore(lastInstruction, loadArgumentInstruction);
                            ilProcessor.InsertAfter(loadArgumentInstruction, loadStringInstruction);
                            ilProcessor.InsertAfter(loadStringInstruction, createObjectInstruction);
                        }
                        //    //string sentence = String.Concat("Code added in ", methodToChange.Name);
                        //    //Mono.Cecil.Cil.ILProcessor ilProcessor = methodToChange.Body.GetILProcessor();

                        //    //Mono.Cecil.Cil.Instruction loadStringInstruction = ilProcessor.Create(OpCodes.Ldstr, sentence);
                        //    //Mono.Cecil.Cil.Instruction callInstruction = ilProcessor.Create(OpCodes.Call, methodReference);

                        //    //Mono.Cecil.Cil.Instruction methodFirstInstruction = methodToChange.Body.Instructions[0];

                        //    //ilProcessor.InsertBefore(methodFirstInstruction, loadStringInstruction);
                        //    //ilProcessor.InsertAfter(loadStringInstruction, callInstruction);
                        //}
                    }
                }

                module.Write(); // Write to the same file that was used to open the file
            }

        }


    }
}
