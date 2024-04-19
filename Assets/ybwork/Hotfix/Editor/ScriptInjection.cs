using Mono.Cecil;
using Mono.Cecil.Cil;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hotfix.Editor
{
    public static class ScriptInjection
    {
        public static void GenerateHotfixIL(string dllName)
        {
            using FileStream fsSourse = new(dllName, FileMode.Open);
            using MemoryStream ms = new MemoryStream();
            fsSourse.CopyTo(ms);
            ms.Position = 0;

            AssemblyDefinition assembiy = AssemblyDefinition.ReadAssembly(ms);

            /// 遍历程序所有方法，进行代码注入
            /// 对所有方法，嵌入一段代码，检查该方法是否为热更新代码，如果是热更新代码，执行热更新逻辑
            /// 对标记了<see cref="HotfixAttribute"/>的方法，将其IL序列化
            foreach (TypeDefinition typeDef in assembiy.MainModule.Types)
            {
                foreach (MethodDefinition methedDef in typeDef.Methods)
                {
                    string hotFixAttribute = typeof(HotfixAttribute).FullName;
                    if (methedDef.CustomAttributes.Any(attr => attr.AttributeType.FullName == hotFixAttribute))
                    {
                        // IL序列化
                        GenerateIL(methedDef);
                    }
                }
            }
        }

        public static void ILInjection(string dllName)
        {
            using FileStream fsSourse = new(dllName, FileMode.Open);
            using MemoryStream ms = new MemoryStream();
            fsSourse.CopyTo(ms);
            ms.Position = 0;

            AssemblyDefinition assembiy = AssemblyDefinition.ReadAssembly(ms);

            /// 遍历程序所有方法，进行代码注入
            /// 对所有方法，嵌入一段代码，检查该方法是否为热更新代码，如果是热更新代码，执行热更新逻辑
            /// 对标记了<see cref="HotfixAttribute"/>的方法，将其IL序列化
            foreach (TypeDefinition typeDef in assembiy.MainModule.Types)
            {
                foreach (MethodDefinition methedDef in typeDef.Methods)
                {
                    // 代码注入
                    InjectionHotfix(assembiy, methedDef);
                }
            }
            assembiy.Write(fsSourse);
        }

        private static void GenerateIL(MethodDefinition methodDefinition)
        {
            HotfixMethodInfo methodInfo = Convert(methodDefinition);
            string content = JsonConvert.SerializeObject(methodInfo, Formatting.Indented);
            Directory.CreateDirectory(Application.streamingAssetsPath);
            string name = methodDefinition.DeclaringType.FullName + "." + methodDefinition.Name;
            string path = Path.Combine(HotfixRunner.RootPath, name + ".json");
            File.WriteAllText(path, content);
        }

        private static HotfixMethodInfo Convert(MethodDefinition methodDefinition)
        {
            string name = methodDefinition.Name;
            string[] parameters = methodDefinition.Parameters
                .Select(p => p.ParameterType.FullName)
                .ToArray();
            string returnType = methodDefinition.ReturnType.FullName;
            HotfixMethodBodyInfo bodyInfo = Convert(methodDefinition.Body);
            return new HotfixMethodInfo
            {
                Name = name,
                Parameters = parameters,
                ReturnType = returnType,
                Body = bodyInfo,
            };
        }

        private static HotfixMethodBodyInfo Convert(MethodBody methodBody)
        {
            int maxStackSize = methodBody.MaxStackSize;
            string[] variables = methodBody.Variables
                .Select(v => v.VariableType.FullName)
                .ToArray();
            HotfixInstruction[] instructions = methodBody.Instructions
                .Select(instruction =>
                {
                    int offset = instruction.Offset;
                    HotfixOpCode code = (HotfixOpCode)(int)instruction.OpCode.Code;
                    object operand = instruction.Operand;
                    HotfixInstruction result = new HotfixInstruction();
                    result.Offset = offset;
                    result.Code = code;
                    if (operand != null)
                    {
                        if (operand is TypeReference typeReference)
                        {
                            result.OperandType = OperandType.Type;
                            result.Operand = typeReference.FullName;
                        }
                        else if (operand is MethodReference methodReference)
                        {
                            result.OperandType = OperandType.Method;
                            result.Operand = methodReference.FullName;
                        }
                        else if (operand is Instruction targetInstruction)
                        {
                            result.OperandType = OperandType.Instruction;
                            result.Operand = targetInstruction.Offset;
                        }
                        else if (operand is string str)
                        {
                            result.OperandType = OperandType.String;
                            result.Operand = str;
                        }
                        else
                            throw new Exception("错误的OperandType:" + operand.GetType());
                    }
                    return result;
                })
                .ToArray();
            return new HotfixMethodBodyInfo(maxStackSize, variables, instructions);
        }

        private static void InjectionHotfix(AssemblyDefinition assembly, MethodDefinition methodDefinition)
        {
            // 找到需要AOP的函数
            ILProcessor processor = methodDefinition.Body.GetILProcessor();

            if (processor.Body.Instructions.Count == 0)
                return;

            Instruction firstInstruction = processor.Body.Instructions[0];
            //Instruction lastInstruction = processor.Body.Instructions[^1];

            MethodReference runMethodReference;
            if (methodDefinition.ReturnType.FullName == typeof(void).FullName)
            {
                System.Reflection.MethodInfo runMethod = typeof(HotfixRunner).GetMethod("RunVoid");
                runMethodReference = assembly.MainModule.ImportReference(runMethod);
            }
            else
            {
                System.Reflection.MethodInfo runMethod = typeof(HotfixRunner).GetMethod("Run");
                runMethodReference = assembly.MainModule.ImportReference(runMethod);
                runMethodReference = MakeGenericMethod(runMethodReference, methodDefinition.ReturnType);
            }

            System.Reflection.MethodInfo judgeMethod = typeof(HotfixRunner).GetMethod("IsHotfixMethod");
            MethodReference judgeMethodReference = assembly.MainModule.ImportReference(judgeMethod);

            System.Reflection.ConstructorInfo stackTraceConstructor = typeof(System.Diagnostics.StackTrace).GetConstructor(Array.Empty<Type>());
            MethodReference stacktraceType = assembly.MainModule.ImportReference(stackTraceConstructor);
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Newobj, stacktraceType));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Call, judgeMethodReference));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Brfalse_S, firstInstruction));

            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Newobj, stacktraceType));
            if (methodDefinition.IsStatic)
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldnull));
            else
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_0));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Call, runMethodReference));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ret));
        }

        private static MethodReference MakeGenericMethod(MethodReference runMethodReference, params TypeReference[] types)
        {
            // 创建泛型方法引用
            GenericInstanceMethod genericMethod = new GenericInstanceMethod(runMethodReference);
            // 添加泛型参数
            foreach (var type in types)
            {
                genericMethod.GenericArguments.Add(type);
            }
            runMethodReference = genericMethod;
            return runMethodReference;
        }
    }
}
