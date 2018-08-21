using BasicLogger;
using CommonComponents;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            using (ModuleDefinition module = ModuleDefinition.ReadModule(fileName, new ReaderParameters { ReadWrite = true }))
            {
                Console.WriteLine(string.Format("Weaving attempt on: {0}", fileName));
                TypeDefinition[] types = module.Types.ToArray();
                foreach (TypeDefinition type in types)
                {
                    //if (type.Name != "Class1")
                    //{
                    //    continue;
                    //}


                    List<MethodDefinition> methods = type.Methods.ToArray().ToList();

                    if (methods.All(x => !x.HasCustomAttributes))
                    {
                        continue;
                    }

                    var methodsToWave = methods
                        .Where(x => !x.IsConstructor)
                        .Where(x =>
                    {
                        if (!x.HasCustomAttributes)
                        {
                            return false;
                        }
                        IEnumerable<CustomAttribute> logAttributes = x.CustomAttributes.Where(attribute => attribute.AttributeType.Name.Equals(typeof(LogMethodAttribute).Name));
                        return logAttributes.Any();
                    });

                    if (!methodsToWave.Any())
                    {
                        continue;
                    }

                    FieldDefinition nonStaticLoggerField = new FieldDefinition("CecilLogger", Mono.Cecil.FieldAttributes.Private, module.ImportReference(typeof(Logger)));

                    MethodInfo loggerLogMethod = typeof(Logger).GetMethod("LogMessage", new Type[1] { typeof(string) });
                    MethodReference calledMethodReference = module.ImportReference(loggerLogMethod);

                    if (methodsToWave.Any(x => x.IsStatic))
                    {
                        //add static logger field and initialize it in static constructor
                    }
                    if (methodsToWave.Any(x => !x.IsStatic))
                    {
                        //add non-static logger field and initialize it in constructors. ideally we should make this clean i.e. check if constructor call other constructor and don't do the weaving if the other one is already waved
                        type.Fields.Add(nonStaticLoggerField);

                        IEnumerable<MethodDefinition> constructors = type.Methods.ToArray().Where(x => x.IsConstructor && !x.IsStatic);

                        foreach (MethodDefinition constructor in constructors)
                        {
                            IEnumerable<string> constructorParameters = constructor.Parameters.ToArray().Select(constructorParam => string.Format("{0} {1}", constructorParam.ParameterType.Name, constructorParam.Name));
                            string paramsString = constructorParameters.Any() ? string.Join(", ", constructorParameters) : "[parametrless]";
                            Console.WriteLine(string.Format("Waving non-static {0} constructor with parameters: ", type.Name, paramsString));

                            ConstructorInfo loggerConstructor = typeof(Logger).GetConstructor(new Type[1] { typeof(string) });
                            MethodReference constructorReference = module.ImportReference(loggerConstructor);

                            ILProcessor ilProcessor = constructor.Body.GetILProcessor();

                            Instruction nopInstruction = ilProcessor.Create(OpCodes.Nop);
                            Instruction loadArgumentInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
                            Instruction loadStringInstruction = ilProcessor.Create(OpCodes.Ldstr, logPath);
                            Instruction createObjectInstruction = ilProcessor.Create(OpCodes.Newobj, constructorReference);

                            int instructionCount = constructor.Body.Instructions.Count;
                            var lastInstruction = constructor.Body.Instructions[instructionCount - 1];

                            ilProcessor.InsertBefore(lastInstruction, nopInstruction);
                            ilProcessor.InsertAfter(nopInstruction, loadArgumentInstruction);
                            ilProcessor.InsertAfter(loadArgumentInstruction, loadStringInstruction);
                            ilProcessor.InsertAfter(loadStringInstruction, createObjectInstruction);
                            ilProcessor.InsertAfter(createObjectInstruction, ilProcessor.Create(OpCodes.Stfld, nonStaticLoggerField));
                        }
                    }

                    foreach (MethodDefinition methodToChange in methodsToWave)
                    {
                        Console.WriteLine("Weaving method: " + methodToChange.Name);

                        ILProcessor ilProcessor = methodToChange.Body.GetILProcessor();

                        Instruction loadArgumentInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
                        Instruction loadFieldInstruction = ilProcessor.Create(OpCodes.Ldfld, nonStaticLoggerField);
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
                        Instruction loadFieldInstruction2 = ilProcessor.Create(OpCodes.Ldfld, nonStaticLoggerField);
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
                module.Write(); // Write to the same file that was used to open the file
            }

        }


    }
}
