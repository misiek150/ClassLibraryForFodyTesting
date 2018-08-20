using BasicLogger;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

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
            using (ModuleDefinition module = ModuleDefinition.ReadModule(fileName, new ReaderParameters { ReadWrite = true }))
            {
                TypeDefinition[] types = module.Types.ToArray();
                foreach (TypeDefinition type in types)
                {
                    if (type.Name != "Class1")
                    {
                        continue;
                    }

                    FieldDefinition loggerField = new FieldDefinition("CecilLogger", Mono.Cecil.FieldAttributes.Private, module.ImportReference(typeof(Logger)));
                    type.Fields.Add(loggerField);

                    ConstructorInfo loggerConstructor = typeof(Logger).GetConstructor(new Type[1] { typeof(string) });
                    MethodInfo loggerLogMethod = typeof(Logger).GetMethod("LogMessage", new Type[1] { typeof(string) });

                    MethodReference constructorReference = module.ImportReference(loggerConstructor);
                    MethodReference calledMethodReference = module.ImportReference(loggerLogMethod);

                    foreach (MethodDefinition methodToChange in type.Methods)
                    {
                        ILProcessor ilProcessor = methodToChange.Body.GetILProcessor();

                        if (methodToChange.IsConstructor)
                        {
                            Instruction nopInstruction = ilProcessor.Create(OpCodes.Nop);
                            Instruction loadArgumentInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
                            Instruction loadStringInstruction = ilProcessor.Create(OpCodes.Ldstr, logPath);
                            Instruction createObjectInstruction = ilProcessor.Create(OpCodes.Newobj, constructorReference);

                            int instructionCount = methodToChange.Body.Instructions.Count;
                            var lastInstruction = methodToChange.Body.Instructions[instructionCount - 1];

                            ilProcessor.InsertBefore(lastInstruction, nopInstruction);
                            ilProcessor.InsertAfter(nopInstruction, loadArgumentInstruction);
                            ilProcessor.InsertAfter(loadArgumentInstruction, loadStringInstruction);
                            ilProcessor.InsertAfter(loadStringInstruction, createObjectInstruction);
                            ilProcessor.InsertAfter(createObjectInstruction, ilProcessor.Create(OpCodes.Stfld, loggerField));
                        }
                        else if (methodToChange.Name.Equals("DoSomeJob", StringComparison.InvariantCultureIgnoreCase) || methodToChange.Name.Equals("DoSomeOtherJob", StringComparison.InvariantCultureIgnoreCase))
                        {

                            Instruction loadArgumentInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
                            Instruction loadFieldInstruction = ilProcessor.Create(OpCodes.Ldfld, loggerField);
                            Instruction loadStringInstruction = ilProcessor.Create(OpCodes.Ldstr, string.Format("{0} method launched", methodToChange.FullName));
                            Instruction callMethodInstruction = ilProcessor.Create(OpCodes.Callvirt, calledMethodReference);
                            Instruction nopInstruction = ilProcessor.Create(OpCodes.Nop);

                            var firstInstruction = methodToChange.Body.Instructions[0];
                            ilProcessor.InsertAfter(firstInstruction, loadArgumentInstruction);
                            ilProcessor.InsertAfter(loadArgumentInstruction, loadFieldInstruction);
                            ilProcessor.InsertAfter(loadFieldInstruction, loadStringInstruction);
                            ilProcessor.InsertAfter(loadStringInstruction, callMethodInstruction);
                            ilProcessor.InsertAfter(callMethodInstruction, nopInstruction);

                            Instruction loadArgumentInstruction2 = ilProcessor.Create(OpCodes.Ldarg_0);
                            Instruction loadFieldInstruction2 = ilProcessor.Create(OpCodes.Ldfld, loggerField);
                            Instruction loadStringInstruction2 = ilProcessor.Create(OpCodes.Ldstr, string.Format("{0} method finished", methodToChange.FullName));
                            Instruction callMethodInstruction2 = ilProcessor.Create(OpCodes.Callvirt, calledMethodReference);
                            Instruction nopInstruction2 = ilProcessor.Create(OpCodes.Nop);

                            int instructionCount = methodToChange.Body.Instructions.Count;
                            var lastInstruction = methodToChange.Body.Instructions[instructionCount - 1];
                            ilProcessor.InsertBefore(lastInstruction, loadArgumentInstruction2);
                            ilProcessor.InsertAfter(loadArgumentInstruction2, loadFieldInstruction2);
                            ilProcessor.InsertAfter(loadFieldInstruction2, loadStringInstruction2);
                            ilProcessor.InsertAfter(loadStringInstruction2, callMethodInstruction2);
                            ilProcessor.InsertAfter(callMethodInstruction2, nopInstruction2);
                        }
                        else if (methodToChange.Name.Equals("AddIntegers", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string methodParameters = "[No Parameters]";
                            if (methodToChange.HasParameters)
                            {
                                StringBuilder sb = new StringBuilder();
                                for (int i = 0; i < methodToChange.Parameters.Count; i++)
                                {
                                    ParameterDefinition param = methodToChange.Parameters[i];
                                    sb.AppendFormat("Parameter[{0}]: {1} - {2};", i, param.Name, param.ParameterType.Name);
                                }
                            }

                            Instruction loadArgumentInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
                            Instruction loadFieldInstruction = ilProcessor.Create(OpCodes.Ldfld, loggerField);
                            Instruction loadStringInstruction = ilProcessor.Create(OpCodes.Ldstr, string.Format("{0} method launched: {1}", methodToChange.FullName, methodParameters));
                            Instruction callMethodInstruction = ilProcessor.Create(OpCodes.Callvirt, calledMethodReference);
                            Instruction nopInstruction = ilProcessor.Create(OpCodes.Nop);

                            var firstInstruction = methodToChange.Body.Instructions[0];
                            ilProcessor.InsertAfter(firstInstruction, loadArgumentInstruction);
                            ilProcessor.InsertAfter(loadArgumentInstruction, loadFieldInstruction);
                            ilProcessor.InsertAfter(loadFieldInstruction, loadStringInstruction);
                            ilProcessor.InsertAfter(loadStringInstruction, callMethodInstruction);
                            ilProcessor.InsertAfter(callMethodInstruction, nopInstruction);

                            Instruction loadArgumentInstruction2 = ilProcessor.Create(OpCodes.Ldarg_0);
                            Instruction loadFieldInstruction2 = ilProcessor.Create(OpCodes.Ldfld, loggerField);
                            Instruction loadStringInstruction2 = ilProcessor.Create(OpCodes.Ldstr, string.Format("{0} method finished", methodToChange.FullName));
                            Instruction callMethodInstruction2 = ilProcessor.Create(OpCodes.Callvirt, calledMethodReference);
                            Instruction nopInstruction2 = ilProcessor.Create(OpCodes.Nop);

                            int instructionCount = methodToChange.Body.Instructions.Count;
                            var lastInstruction = methodToChange.Body.Instructions[instructionCount - 1];
                            ilProcessor.InsertBefore(lastInstruction, loadArgumentInstruction2);
                            ilProcessor.InsertAfter(loadArgumentInstruction2, loadFieldInstruction2);
                            ilProcessor.InsertAfter(loadFieldInstruction2, loadStringInstruction2);
                            ilProcessor.InsertAfter(loadStringInstruction2, callMethodInstruction2);
                            ilProcessor.InsertAfter(callMethodInstruction2, nopInstruction2);
                        }
                    }
                }
                module.Write(); // Write to the same file that was used to open the file
            }

        }


    }
}
