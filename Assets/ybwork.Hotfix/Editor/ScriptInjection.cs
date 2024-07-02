using Mono.Cecil;
using Mono.Cecil.Cil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;

namespace Hotfix.Editor
{
    internal static class ScriptInjection
    {
        public static HotfixMethodInfo[] GenerateHotfixIL(string dllName, string oldDllName)
        {
            using MemoryStream ms1 = LoadAssemblyFile(dllName);
            using MemoryStream ms2 = LoadAssemblyFile(oldDllName);
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(ms1);
            AssemblyDefinition oldAssembly = AssemblyDefinition.ReadAssembly(ms2);

            Dictionary<string, MethodDefinition> methods = new Dictionary<string, MethodDefinition>();

            /// 遍历程序所有方法，进行IL序列化
            foreach (TypeDefinition typeDef in assembly.MainModule.GetTypes())
            {
                TypeDefinition oldTypeRef = oldAssembly.MainModule.Types
                    .FirstOrDefault(type => type.FullName == typeDef.FullName);
                if (oldTypeRef == null)
                {
                    oldTypeRef = oldAssembly.MainModule.GetTypes()
                        .FirstOrDefault(type => type.FullName == typeDef.FullName);
                    if (oldTypeRef == null)
                    {
                        foreach (MethodDefinition methedDef in typeDef.Methods)
                        {
                            methods[methedDef.FullName] = methedDef;
                        }
                    }
                    else
                    {
                        bool changed = false;
                        foreach (MethodDefinition methedDef in typeDef.Methods)
                        {
                            MethodDefinition oldMethedDef = oldTypeRef.Methods
                                .FirstOrDefault(method => method.FullName == methedDef.FullName);
                            if (!Equals(methedDef, oldMethedDef))
                            {
                                changed = true;
                            }
                        }
                        if (changed)
                        {
                            foreach (MethodDefinition methedDef in typeDef.Methods)
                            {
                                //Regex regex = new Regex("^(\\S+)/<(\\S+)>");
                                //Match match = regex.Match(methedDef.DeclaringType.FullName);
                                //Debug.Log(regex);
                                //Debug.Log(methedDef.DeclaringType.FullName);
                                //Debug.Log(match);
                                methods[methedDef.FullName] = methedDef;
                            }
                        }
                    }
                }
                else
                {
                    foreach (MethodDefinition methedDef in typeDef.Methods)
                    {
                        MethodDefinition oldMethedDef = oldTypeRef.Methods
                            .FirstOrDefault(method => method.FullName == methedDef.FullName);
                        if (!Equals(methedDef, oldMethedDef))
                        {
                            methods[methedDef.FullName] = methedDef;
                        }
                    }
                }
            }
            return methods.Values
                .OrderBy(method => method.DeclaringType.FullName + "::" + method.Name)
                .Select(method => Convert(method))
                .ToArray();
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

        private static MemoryStream LoadAssemblyFile(string dllName)
        {
            using FileStream fsSourse = new(dllName, FileMode.Open);
            MemoryStream ms = new MemoryStream();
            fsSourse.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }

        private static bool Equals(MethodDefinition method1, MethodDefinition method2)
        {
            if (method1 == null || method2 == null)
                return false;

            if (method1.IsStatic != method2.IsStatic)
                return false;

            MethodBody body1 = method1.Body;
            MethodBody body2 = method2.Body;

            if (body1.CodeSize != body2.CodeSize)
                return false;

            if (body1.MaxStackSize != body2.MaxStackSize)
                return false;

            if (body1.Variables.Count != body2.Variables.Count)
                return false;

            for (int i = 0; i < body1.Variables.Count; i++)
            {
                if (body1.Variables[i].VariableType.FullName != body2.Variables[i].VariableType.FullName)
                    return false;
            }

            if (body1.Instructions.Count != body2.Instructions.Count)
                return false;
            for (int i = 0; i < body1.Instructions.Count; i++)
            {
                if (body1.Instructions[i].Offset != body2.Instructions[i].Offset)
                    return false;
                if (body1.Instructions[i].OpCode != body2.Instructions[i].OpCode)
                    return false;
                if (body1.Instructions[i].Operand?.ToString() != body2.Instructions[i].Operand?.ToString())
                    return false;
                if (body1.Instructions[i].Next?.Offset != body2.Instructions[i].Next?.Offset)
                    return false;
            }

            return true;
        }

        private static HotfixMethodInfo Convert(MethodDefinition methodDefinition)
        {
            string name = methodDefinition.FullName;
            string[] parameters = methodDefinition.Parameters
                .Select(p => p.ParameterType.FullName)
                .ToArray();
            string returnType = methodDefinition.ReturnType.FullName;
            HotfixMethodBodyInfo bodyInfo = Convert(methodDefinition.Body);
            return new HotfixMethodInfo
            {
                Name = name,
                IsStatic = methodDefinition.IsStatic,
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
            Dictionary<int, HotfixInstruction> instructions = methodBody.Instructions
                .ToDictionary(
                    instruction => instruction.Offset,
                    instruction =>
                {
                    int offset = instruction.Offset;
                    HotfixOpCode code = (HotfixOpCode)(int)instruction.OpCode.Code;
                    object operand = instruction.Operand;
                    HotfixInstruction result = new HotfixInstruction();
                    result.Code = code;
                    if (instruction.Next != null)
                        result.NextOffset = instruction.Next.Offset;
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
                        else if (operand is VariableDefinition variableDefinition)
                        {
                            result.OperandType = OperandType.Variable;
                            result.Operand = variableDefinition.Index;
                        }
                        else if (operand is FieldDefinition fieldDefinition)
                        {
                            result.OperandType = OperandType.Variable;
                            result.Operand = fieldDefinition.FullName;
                        }
                        else if (operand is Instruction targetInstruction)
                        {
                            result.OperandType = OperandType.Instruction;
                            result.Operand = targetInstruction.Offset;
                        }
                        else if (operand is ParameterDefinition parameterDefinition && code == HotfixOpCode.Ldarga_S)
                        {
                            result.Code = HotfixOpCode.Ldarg_S;
                            result.OperandType = OperandType.Int;
                            if (methodBody.Method.IsStatic)
                                result.Operand = parameterDefinition.Index;
                            else
                                result.Operand = parameterDefinition.Index + 1;
                        }
                        else if (operand is string str)
                        {
                            result.OperandType = OperandType.String;
                            result.Operand = str;
                        }
                        else if (operand is sbyte sbyteValue)
                        {
                            result.OperandType = OperandType.Int;
                            result.Operand = (int)sbyteValue;
                        }
                        else if (operand is int intValue)
                        {
                            result.OperandType = OperandType.Int;
                            result.Operand = intValue;
                        }
                        else if (operand is float floatValue)
                        {
                            result.OperandType = OperandType.Float;
                            result.Operand = floatValue;
                        }
                        else if (operand is Instruction[] instructions)
                        {
                            result.OperandType = OperandType.Instructions;
                            IEnumerable<int> offsets = instructions.Select(instruction => instruction.Offset);
                            result.Operand = JsonConvert.SerializeObject(offsets);
                        }
                        else
                        {
                            result.OperandType = OperandType.Other;
                            result.Operand = operand;
                            throw new Exception("错误的OperandType:" + operand.GetType() + "--" + result.ToString());
                        }
                    }
                    result.CodeString = result.ToString();
                    return result;
                });
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
                System.Reflection.MethodInfo runMethod = typeof(HotfixRunner).GetMethod(
                    "RunVoid",
                    new Type[] { typeof(StackTrace), typeof(object), typeof(object[]) });
                runMethodReference = assembly.MainModule.ImportReference(runMethod);
            }
            else
            {
                System.Reflection.MethodInfo runMethod = typeof(HotfixRunner).GetMethod(
                    "Run",
                    new Type[] { typeof(StackTrace), typeof(object), typeof(object[]) });
                runMethodReference = assembly.MainModule.ImportReference(runMethod);
                runMethodReference = MakeGenericMethod(runMethodReference, methodDefinition.ReturnType);
            }

            System.Reflection.MethodInfo judgeMethod = typeof(HotfixRunner).GetMethod(
                "IsHotfixMethod",
                new Type[] { typeof(StackTrace) });
            MethodReference judgeMethodReference = assembly.MainModule.ImportReference(judgeMethod);

            System.Reflection.ConstructorInfo stackTraceConstructor = typeof(StackTrace).GetConstructor(Array.Empty<Type>());
            MethodReference stacktraceType = assembly.MainModule.ImportReference(stackTraceConstructor);
            TypeReference objectType = assembly.MainModule.ImportReference(typeof(object));

            // if (HotfixRunner.IsHotfixMethod(new StackTrace()))
            //     执行嵌入逻辑
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Newobj, stacktraceType));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Call, judgeMethodReference));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Brfalse_S, firstInstruction));

            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Newobj, stacktraceType));

            // 获取this值
            if (methodDefinition.IsStatic)
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldnull));
            else
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_0));

            //var paras = new object[真实参数数量];
            int paraCount = methodDefinition.Parameters.Count;
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldc_I4_S, (sbyte)paraCount));
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Newarr, objectType));
            for (int i = 0; i < paraCount; i++)
            {
                int paraIndex = methodDefinition.IsStatic ? i : i + 1;
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Dup));
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldc_I4_S, (sbyte)i));
                switch (paraIndex)
                {
                    // paras[i]=真实参数[i]
                    case 0:
                        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_0));
                        break;
                    case 1:
                        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_1));
                        break;
                    case 2:
                        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_2));
                        break;
                    case 3:
                        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_3));
                        break;
                    default:
                        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_S, (sbyte)paraIndex));
                        break;
                }
                if (methodDefinition.Parameters[i].ParameterType.IsValueType)
                {
                    var parameterType = methodDefinition.Parameters[i].ParameterType;
                    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Box, parameterType));
                }
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Stelem_Ref));
            }

            //return HotfixRunner.Run<int>(new StackTrace(), this, paras)
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
