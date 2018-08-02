using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
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
            var filesToModify = Directory.GetFiles(path, "ClassLibrary3.dll", SearchOption.AllDirectories);
            foreach (string file in filesToModify)
            {
                Console.WriteLine(file);
                //tools.PrintTypes(file);
                //Console.WriteLine("- - - - - - - - - - - - -");
                tools.AddLogger(file, "F:\\LogFile.txt");
                //tools.PrintTypes(file);
            }
            //tools.PrintTypes(FileName);

            //tools.ModifyAssembly(FileName);

            //Console.ReadLine();
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
                TypeReference loggerReference = module.ImportReference(typeof(BasicLogger.Logger));

                // Modify the assembly
                //TypeDefinition type = new TypeDefinition("NameSpace", "Name", Mono.Cecil.TypeAttributes.ExplicitLayout);
                //type.Properties.Add(new PropertyDefinition("Name", Mono.Cecil.PropertyAttributes.None, ))
                TypeDefinition[] types = module.Types.ToArray();

                //MethodReference methodReference = module.ImportReference(writeLineMethod);

                foreach (var type in types)
                {
                    if (type.Name == "Class2")
                    {
                        continue;
                    }

                    //type.Fields.Add(new FieldDefinition("CecilLogger", Mono.Cecil.FieldAttributes.Private, loggerReference));
                    type.Fields.Add(new FieldDefinition("CecilLogger", Mono.Cecil.FieldAttributes.Private, module.ImportReference(typeof(Int32))));

                    //type.Properties.Add(new PropertyDefinition("AddedLogger", Mono.Cecil.PropertyAttributes.None, loggerReference));
                    ConstructorInfo loggerConstructor = typeof(BasicLogger.Logger).GetConstructor(new Type[1] { typeof(string) });
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
                            //ILProcessor ilProcessor = methodToChange.Body.GetILProcessor();

                            //Instruction loadArgumentInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
                            //Instruction loadStringInstruction = ilProcessor.Create(OpCodes.Ldstr, logPath);
                            //Instruction createObjectInstruction = ilProcessor.Create(OpCodes.Call, constructorReference);

                            //int instructionCount = methodToChange.Body.Instructions.Count;
                            //var lastInstruction = methodToChange.Body.Instructions[instructionCount - 1];


                            //ilProcessor.InsertBefore(lastInstruction, loadArgumentInstruction);
                            //ilProcessor.InsertAfter(loadArgumentInstruction, loadStringInstruction);
                            //ilProcessor.InsertAfter(loadStringInstruction, createObjectInstruction);
                        }
                        //string sentence = String.Concat("Code added in ", methodToChange.Name);
                        //Mono.Cecil.Cil.ILProcessor ilProcessor = methodToChange.Body.GetILProcessor();

                        //Mono.Cecil.Cil.Instruction loadStringInstruction = ilProcessor.Create(OpCodes.Ldstr, sentence);
                        //Mono.Cecil.Cil.Instruction callInstruction = ilProcessor.Create(OpCodes.Call, methodReference);

                        //Mono.Cecil.Cil.Instruction methodFirstInstruction = methodToChange.Body.Instructions[0];

                        //ilProcessor.InsertBefore(methodFirstInstruction, loadStringInstruction);
                        //ilProcessor.InsertAfter(loadStringInstruction, callInstruction);
                    }
                }

                module.Write(); // Write to the same file that was used to open the file
            }

        }


    }
}
